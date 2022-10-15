
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Domain;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using System;

/// <summary>
/// Saves pending changes to the service. 
/// </summary>
public sealed class CommitCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "commit", };

    public CommitCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.client = client;
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
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

        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;
            {
                goto error;
            }
        }

        ok:
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
            Console.WriteLine("No current profile. ");
            return;
        }

        var operationsCopy = new List<MynatimeProfileTransactionItem>(0);
        if (profile.Transaction != null)
        {
            operationsCopy = profile.Transaction.Items.ToList();
        }

        if (profile.Transaction == null || operationsCopy.Count == 0)
        {
            Console.WriteLine("No pending operation. ");
            return;
        }

        var homePage = await this.client.GetHomepage();
        if (homePage.Succeed)
        {
            // fine
        }
        else if (homePage.Errors?.Any(x => x.Code == "LoggedOut") ?? false)
        {
            // session expired: renew
            Console.Write("  Renewing session... ");

            if (profile.LoginUsername == null || profile.LoginPassword == null)
            {
                Console.WriteLine("ERROR: Missing authentication information. ");
            }

            var loginPage = await this.client.PrepareEmailPasswordAuthenticate();
            if (loginPage.Succeed)
            {
                var loginResultPage = await this.client.EmailPasswordAuthenticate(profile.LoginUsername, profile.LoginPassword);
                if (loginResultPage.Succeed)
                {
                    homePage = await this.client.GetHomepage();
                    if (homePage.Succeed)
                    {
                        // yeah!
                        Console.WriteLine("OK. ");
                    }
                    else
                    {
                        Console.WriteLine("Auto log-in failed: something went wrong 3. ");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Auto log-in failed: something went wrong 2. ");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Auto log-in failed: something went wrong 1. ");
                return;
            }
        }
        else
        {
            Console.WriteLine("Auto log-in failed: something went wrong 0. ");
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
        var visitor = new CommitTransactionItem(this.App, this.client, profile);
        var helper = MynatimeProfileTransactionManager.Default;
        var okayItems = new List<MynatimeProfileTransactionItem>();
        foreach (var operation in operationsCopy)
        {
            i++;
            
            Console.Write(i);
            Console.Write("\t");
            var item = helper.GetInstanceOf(operation);
            visitor.Prepare(i, nextCommitId, nextCommitItemId);
            await item.Accept(visitor);
            if (visitor.Committed)
            {
                okayItems.Add(operation);
                if (visitor.IsRemovable)
                {
                    profile.Transaction.Remove(operation);
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

        public CommitTransactionItem(IConsoleApp app, IManatimeWebClient client, MynatimeProfile profile)
        {
            this.app = app;
            this.client = client;
            this.profile = profile;
        }

        public async Task Visit(ActivityStartStop thing)
        {
            var manager = new ActivityStartStopManager(thing);
            manager.GenerateItems();
            if (manager.Errors.Any())
            {
                Console.WriteLine("Activity tracker has errors: ");
                foreach (var error in manager.Errors)
                {
                    Console.WriteLine("- " + error);
                }
                
                this.Committed = false;
                return;
            }

            foreach (var activity in manager.Activities)
            {
                await this.Visit(activity);
            }

            foreach (var usedEvent in manager.UsedEvents)
            {
                thing.Remove(usedEvent);
            }
            
            Console.WriteLine("Transaction item type is not supported. ");
            this.Committed = true;
            this.IsRemovable = !thing.Events.Any();
        }

        public async Task Visit(NewActivityItemPage thing)
        {
            thing.Comment += "\n\n-- \nMNTM;T=" + this.app.TimeNowUtc.ToInvariantString()
              + ";M=" + Environment.MachineName
              + ";C=" + this.nextCommitItemId.ToInvariantString();

            thing.Arrange();
            if (thing.HasError())
            {
                Console.WriteLine("FAILED: " + thing.GetErrorMessage());
                return;
            }
            
            var page1 = await this.client.GetNewActivityItemPage();
            if (!page1.Succeed)
            {
                Console.WriteLine("FAILED: " + page1);
                return;
            }

            thing.Token = page1.Token;
            var page2 = await this.client.PostNewActivityItemPage(thing);
            if (!page2.Succeed)
            {
                Console.WriteLine("FAILED: " + page2);
                return;
            }

            Console.WriteLine("Saved. ");
            this.Committed = true;
            this.IsRemovable = true;
        }

        public Task Visit(ITransactionItem thing)
        {
            Console.Write(i);
            Console.Write("\t");
            Console.WriteLine("Transaction item type is not supported. ");
            this.Committed = false;
            return Task.CompletedTask;
        }
    }
}
