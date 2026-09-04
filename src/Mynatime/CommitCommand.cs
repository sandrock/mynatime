
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Domain;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using Spectre.Console;
using System;
using System.Globalization;

/// <summary>
/// Saves pending changes to the service.
/// </summary>
public sealed class CommitCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "commit", };

    public CommitCommand(IConsoleApp app, IManatimeWebClient client, IAnsiConsole console)
        : base(app, console)
    {
        this.client = client;
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Commit";
        describe.AddCommandPattern(Args[0], "saves pending changes");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
        var i = -1;
        if (++i >= args.Length || !this.MatchArg(args[i]))
        {
            goto error;
        }

        if (++i < args.Length)
        {
            goto error;
        }

        consumedArgs = args.Length;
        command = this;
        return true;

        error:
        consumedArgs = 0;
        command = null;
        return false;
    }

    public override async Task Run()
    {
        var profile = this.App.CurrentProfile;
        if (profile == null)
        {
            this.Console.MarkupLine("[red]No current profile.[/]");
            return;
        }

        var operationsCopy = new List<MynatimeProfileTransactionItem>(0);
        if (profile.Transaction != null)
        {
            operationsCopy = profile.Transaction.Items.ToList();
        }

        if (profile.Transaction == null || operationsCopy.Count == 0)
        {
            this.Console.WriteLine("No pending operation. ");
            return;
        }

        var latestTag = await UpdateChecker.GetLatestTagAsync();
        if (UpdateChecker.IsNewer(latestTag, UpdateChecker.GetCurrentVersion()))
        {
            var current = UpdateChecker.GetCurrentVersion() ?? "unknown";
            this.Console.MarkupLine($"[yellow]A new version is available: {Markup.Escape(latestTag!)} (current: {Markup.Escape(current)})[/]");
            this.Console.MarkupLine("[yellow]The update may include important fixes. Please update before committing.[/]");
            this.Console.MarkupLine("[dim]Run mynatime-update to upgrade.[/]");
            this.Console.WriteLine(string.Empty);
            var proceed = this.Console.Prompt(new ConfirmationPrompt("Proceed anyway?") { DefaultValue = false });
            if (!proceed)
            {
                return;
            }

            this.Console.WriteLine(string.Empty);
        }

        BaseResult? homePage = null;
        string? sessionError = null;
        await this.Console.Status().StartAsync("Connecting...", async ctx =>
        {
            homePage = await this.client.GetHomepage();
            if (homePage.Succeed)
            {
                // fine
            }
            else if (homePage.Errors?.Any(x => x.Code == ErrorCode.LoggedOut) ?? false)
            {
                if (profile.LoginUsername == null || profile.LoginPassword == null)
                {
                    sessionError = "Missing authentication information.";
                    return;
                }

                ctx.Status("Renewing session...");
                var loginPage = await this.client.PrepareEmailPasswordAuthenticate();
                if (loginPage.Succeed)
                {
                    var loginResultPage = await this.client.EmailPasswordAuthenticate(profile.LoginUsername, profile.LoginPassword);
                    if (loginResultPage.Succeed)
                    {
                        ctx.Status("Connecting...");
                        homePage = await this.client.GetHomepage();
                        if (!homePage.Succeed)
                        {
                            sessionError = "Auto log-in succeeded but reloading the homepage failed: " + homePage.FormatErrors();
                        }
                    }
                    else
                    {
                        sessionError = "Auto log-in failed (authentication rejected): " + loginResultPage.FormatErrors();
                    }
                }
                else
                {
                    sessionError = "Auto log-in failed (could not open the login page): " + loginPage.FormatErrors();
                }
            }
            else
            {
                sessionError = "Could not connect to Manatime: " + homePage.FormatErrors();
            }
        });

        if (sessionError != null)
        {
            this.Console.MarkupLine("[red]" + Markup.Escape(sessionError) + "[/]");
            return;
        }

        profile.Cookies = this.client.GetCookies();

        long nextCommitId = 1, nextCommitItemId = 1;
        if (profile.Commits != null)
        {
            foreach (var item in profile.Commits.Items)
            {
                if (item.CommitId != null && item.CommitId >= nextCommitId)
                {
                    nextCommitId = item.CommitId.Value + 1;
                }

                if (item.CommitItemId != null && item.CommitItemId >= nextCommitItemId)
                {
                    nextCommitItemId = item.CommitItemId.Value + 1;
                }
            }
        }

        int i = -1;
        var visitor = new CommitTransactionItem(this.App, this.Console, this.client, profile);
        var helper = MynatimeProfileTransactionManager.Default;
        var okayItems = new List<MynatimeProfileTransactionItem>();
        foreach (var operation in operationsCopy)
        {
            i++;

            this.Console.Write(i.ToString());
            this.Console.Write("\t");
            var item = helper.GetInstanceOf(operation);

            if (profile.ConfirmServiceSave == true)
            {
                var confirmed = this.Console.Prompt(new ConfirmationPrompt("Send item " + i + " to service?"));
                if (!confirmed)
                {
                    this.Console.WriteLine("Skipped.");
                    continue;
                }
            }

            visitor.Prepare(i, nextCommitId, nextCommitItemId);
            await item.Accept(visitor);
            if (visitor.Committed)
            {
                okayItems.Add(operation);
                if (visitor.IsRemovable)
                {
                    profile.Transaction.Remove(operation);
                }
                else
                {
                    // Partial commit: write the pruned in-memory state (failed events only)
                    // back to the raw JSON element so the next commit doesn't re-send
                    // activities that already reached the server.
                    item.ToTransactionItem(operation, this.App.TimeNowUtc);
                }

                operation.TimeCommittedUtc = this.App.TimeNowUtc;
                operation.CommitId = nextCommitId;
                operation.CommitItemId = nextCommitItemId++;

                profile.Commits.Add(operation);
            }
        }

        await this.App.PersistProfile(profile);
    }

    private sealed class CommitTransactionItem : ITransactionItemVisitor
    {
        private readonly IConsoleApp app;
        private readonly IAnsiConsole console;
        private readonly IManatimeWebClient client;
        private readonly MynatimeProfile profile;
        private int i;
        private long nextCommitId;
        private long nextCommitItemId;

        public bool Committed { get; set; }

        public bool IsRemovable { get; set; }

        public void Prepare(int i, long nextCommitId, long nextCommitItemId)
        {
            this.i = i;
            this.Committed = false;
            this.IsRemovable = false;
            this.nextCommitId = nextCommitId;
            this.nextCommitItemId = nextCommitItemId;
        }

        public CommitTransactionItem(IConsoleApp app, IAnsiConsole console, IManatimeWebClient client, MynatimeProfile profile)
        {
            this.app = app;
            this.console = console;
            this.client = client;
            this.profile = profile;
        }

        public async Task Visit(ActivityStartStop thing)
        {
            var manager = new ActivityStartStopManager(thing);
            manager.GenerateItems();
            if (manager.Errors.Any())
            {
                this.console.MarkupLine("[red]Activity tracker has errors:[/]");
                foreach (var error in manager.Errors)
                {
                    this.console.MarkupLine("[red]- " + Markup.Escape(error.ToString()) + "[/]");
                }

                this.Committed = false;
                return;
            }

            var trackerActivities = manager.Activities.Where(a => a.IsStartAndStop).ToList();
            int activitiesSaved = 0, activitiesConverted = 0;

            // Guard: an activity spanning multiple days is almost always a missing or wrong
            // stop, and the server rejects it ("end must be after start"). Ask before sending;
            // when declined (or non-interactive) skip it and leave it — with its events — in
            // the tracker so it can be fixed, instead of freezing a rejection into a standalone.
            var toCommit = new List<ActivityItemWrapper>();
            foreach (var activity in trackerActivities)
            {
                if (activity.Item.DateStart?.Date != activity.Item.DateEnd?.Date
                    && !this.ConfirmMultiDay(activity.Item))
                {
                    continue;
                }

                toCommit.Add(activity);
            }

            // Activities resolved by this commit: saved to the server, or converted to a
            // standalone item. Everything else — transient-stopped, skipped multi-day, or
            // still-open activities — stays in the tracker, so its events (including
            // boundaries shared with a resolved neighbour) must be kept.
            var resolved = new List<NewActivityItemPage>();
            foreach (var activity in toCommit)
            {
                await this.Visit(activity.Item);
                if (!activity.Item.HasError())
                {
                    resolved.Add(activity.Item);
                    activitiesSaved++;
                    continue;
                }

                if (IsTransientError(activity.Item))
                {
                    // Session/connectivity problem: stop now and leave this activity and the
                    // remaining ones in the tracker for a later retry.
                    break;
                }

                // Business rejection (e.g. category not authorized): the tracker cannot save
                // this as-is. Convert it to a standalone pending item, flagged with the reason,
                // so it survives intact and can be fixed, and drop it from the tracker.
                this.ConvertToStandalone(activity.Item);
                resolved.Add(activity.Item);
                activitiesConverted++;
            }

            if (activitiesSaved == 0 && activitiesConverted == 0)
            {
                // Nothing reached the server and nothing was converted: leave the tracker whole.
                this.Committed = false;
                this.IsRemovable = false;
                return;
            }

            manager.RemoveEventsForResolved(resolved);
            this.Committed = true;
            this.IsRemovable = !thing.Events.Any();
        }

        private static bool IsTransientError(NewActivityItemPage activity)
        {
            var code = activity.GetErrorCode();
            if (code == null)
            {
                return false;
            }

            return code == ErrorCode.LoggedOut
                || code == ErrorCode.UnknownError
                || code.StartsWith("InvalidPage/", StringComparison.Ordinal);
        }

        private void ConvertToStandalone(NewActivityItemPage activity)
        {
            activity.SaveFailedReason = activity.GetErrorMessage() ?? "Commit failed.";
            var item = activity.ToTransactionItem(null, this.app.TimeNowUtc);
            this.profile.Transaction!.Add(item);
        }

        /// <summary>
        /// Asks whether to commit an activity that spans multiple days (likely a missing or
        /// wrong stop). Returns false — skip — when declined or when the console is
        /// non-interactive, so a script never sends a suspicious span unattended.
        /// </summary>
        private bool ConfirmMultiDay(NewActivityItemPage activity)
        {
            var name = activity.ActivityId != null
                ? (this.profile.Data?.GetActivityById(activity.ActivityId)?.Name ?? activity.ActivityId)
                : "(no category)";
            var start = (activity.DateStart?.Date ?? default) + (activity.InAt ?? TimeSpan.Zero);
            var end = (activity.DateEnd?.Date ?? default) + (activity.OutAt ?? TimeSpan.Zero);
            var span = start.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                + " -> " + end.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            this.console.MarkupLine("[yellow]Activity \"" + Markup.Escape(name) + "\" spans multiple days (" + Markup.Escape(span) + ").[/]");

            bool proceed = this.console.Profile.Capabilities.Interactive
                && this.console.Prompt(new ConfirmationPrompt("Commit it anyway?") { DefaultValue = false });
            if (!proceed)
            {
                this.console.MarkupLine("[yellow]Skipped — left in the tracker. Fix it (e.g. correct the stop) and commit again.[/]");
            }

            return proceed;
        }

        public async Task Visit(NewActivityItemPage thing)
        {
            // Carry the sync footer only in the POST payload; leave the persisted comment clean.
            thing.CommentForCommit = (thing.Comment ?? string.Empty)
              + "\n\n-- \nMNTM;T=" + this.app.TimeNowUtc.ToInvariantString()
              + ";M=" + Environment.MachineName
              + ";C=" + this.nextCommitItemId.ToInvariantString();

            thing.Arrange();
            if (thing.HasError())
            {
                this.console.MarkupLine("[red]FAILED: " + Markup.Escape(thing.GetErrorMessage() ?? string.Empty) + "[/]");
                return;
            }

            var page1 = await this.client.GetNewActivityItemPage();
            if (!page1.Succeed)
            {
                thing.AddError(page1.Errors!.First());
                this.console.MarkupLine("[red]FAILED: " + Markup.Escape(page1.ToString()!) + "[/]");
                return;
            }

            thing.Token = page1.Token;
            thing.User = page1.User ?? this.profile.UserId;

            if (thing.ActivityId != null && page1.Categories?.Count > 0
                && !page1.Categories.Any(c => c.Id == thing.ActivityId))
            {
                thing.AddError(new BaseError(ErrorCode.InvalidForm.TaskNotAvailable, "Activity '" + thing.ActivityId + "' is not in the server list. It may have been disabled or deleted."));
                this.console.MarkupLine("[red]FAILED: " + Markup.Escape(thing.GetErrorMessage()!) + "[/]");
                return;
            }

            var page2 = await this.client.PostNewActivityItemPage(thing);
            if (!page2.Succeed)
            {
                thing.AddError(page2.Errors!.First());
                this.console.MarkupLine("[red]FAILED: " + Markup.Escape(page2.GetErrorMessage() ?? page2.ToString()!) + "[/]");
                return;
            }

            this.console.MarkupLine("[green]Saved.[/]");
            this.Committed = true;
            this.IsRemovable = true;
        }

        public Task Visit(ITransactionItem thing)
        {
            this.console.Write(i.ToString());
            this.console.Write("\t");
            this.console.WriteLine("Transaction item type is not supported. ");
            this.Committed = false;
            return Task.CompletedTask;
        }
    }
}
