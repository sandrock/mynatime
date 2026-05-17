
namespace Mynatime.CLI;

using System.Globalization;
using Mynatime.Client;
using Mynatime.Domain;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using Spectre.Console;
using System;

/// <summary>
/// Lists pending changes. 
/// </summary>
public class StatusCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "status", };

    public StatusCommand(IConsoleApp app, IManatimeWebClient client, IAnsiConsole console)
        : base(app, console)
    {
        this.client = client;
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Status";
        describe.AddCommandPattern(Args[0], "lists pending changes");
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

        var operations = new List<MynatimeProfileTransactionItem>(0);
        if (profile.Transaction != null)
        {
            operations = profile.Transaction.Items.ToList();
        }

        if (profile.Transaction == null || operations.Count == 0)
        {
            this.Console.WriteLine("No pending operation. ");
            return;
        }

        int i = -1;
        var visitor = new ConsoleDescribeTransactionItem(this.App, this.Console, profile);
        var helper = MynatimeProfileTransactionManager.Default;
        
        foreach (var operation in operations)
        {
            i++;

            if (i > 0)
            {
                this.Console.WriteLine();
            }

            this.Console.Write(i.ToString());
            this.Console.Write("\t");
            var item = helper.GetInstanceOf(operation);
            await item.Accept(visitor);
        }

    }

    public class ConsoleDescribeTransactionItem : ITransactionItemVisitor
    {
        private readonly IConsoleApp app;
        private readonly IAnsiConsole console;
        private readonly MynatimeProfile profile;

        public ConsoleDescribeTransactionItem(IConsoleApp app, IAnsiConsole console, MynatimeProfile profile)
        {
            this.app = app;
            this.console = console;
            this.profile = profile;
        }

        public Task Visit(ActivityStartStop state)
        {
            var manager = new ActivityStartStopManager(state);
            manager.GenerateItems();

            if (manager.Errors.Any())
            {
                this.console.MarkupLine("[red]Errors:[/]");
                foreach (var error in manager.Errors)
                {
                    this.console.MarkupLine("[red]- " + Markup.Escape(error.ToString()) + "[/]");
                }
            }

            if (manager.Warnings.Any())
            {
                this.console.MarkupLine("[yellow]Warnings:[/]");
                foreach (var warning in manager.Warnings)
                {
                    this.console.MarkupLine("[yellow]- " + Markup.Escape(warning.ToString()) + "[/]");
                }
            }

            var table = new Table().Border(TableBorder.Simple);
            table.AddColumn("Date");
            table.AddColumn("In");
            table.AddColumn("Out");
            table.AddColumn("Duration");
            table.AddColumn("Category");
            table.AddColumn("Comment");
            DateTime? lastDate = null;
            foreach (var entry in manager.Activities)
            {
                var item = entry.Item;
                var actDate = item.DateStart ?? item.DateEnd;
                if (lastDate.HasValue && actDate.HasValue && actDate.Value.Date != lastDate.Value.Date)
                {
                    table.AddEmptyRow();
                }

                lastDate = actDate ?? lastDate;
                var dateStr = item.DateStart?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
                var inStr = item.InAt.HasValue ? item.InAt.Value.ToString(@"hh\:mm") : string.Empty;
                var outStr = ActivityFormat.OutDisplay(item);
                var durationStr = ActivityFormat.DurationDisplay(item);

                string categoryMarkup;
                if (item.ActivityId == null)
                {
                    categoryMarkup = string.Empty;
                }
                else
                {
                    var resolved = profile.Data?.GetActivityById(item.ActivityId)?.Name;
                    categoryMarkup = resolved != null
                        ? CliTheme.Tag(CliTheme.Category, resolved)
                        : "[grey](unknown)[/]";
                }

                var comment = item.Comment ?? string.Empty;
                table.AddRow(
                    CliTheme.Tag(CliTheme.Timestamp, dateStr),
                    CliTheme.Tag(CliTheme.Timestamp, inStr),
                    CliTheme.Tag(CliTheme.Timestamp, outStr),
                    CliTheme.Tag(CliTheme.Duration, durationStr),
                    categoryMarkup,
                    CliTheme.Tag(CliTheme.Comment, comment));
            }

            if (table.Rows.Count > 0)
            {
                this.console.Write(table);
            }
            else
            {
                this.console.MarkupLine("  none");
            }

            var openEvents = state.Events.Except(manager.UsedEvents).ToList();
            foreach (var ev in openEvents)
            {
                string openCatPart = string.Empty;
                if (ev.ActivityId != null)
                {
                    var openCatName = profile.Data?.GetActivityById(ev.ActivityId)?.Name ?? "(unknown)";
                    openCatPart = " " + CliTheme.Tag(CliTheme.Category, openCatName);
                }

                this.console.MarkupLine(
                    $"[yellow]Open event:[/] {CliTheme.Tag(CliTheme.Timestamp, ev.TimeLocal.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))}{openCatPart}");
            }

            return Task.CompletedTask;
        }

        public Task Visit(NewActivityItemPage thing)
        {
            var dateStr = thing.DateStart?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

            string timePart;
            if (thing.InAt.HasValue && thing.OutAt.HasValue)
            {
                timePart = CliTheme.Tag(CliTheme.Timestamp, thing.InAt.Value.ToString(@"hh\:mm"))
                    + "->"
                    + CliTheme.Tag(CliTheme.Timestamp, ActivityFormat.OutDisplay(thing));
            }
            else if (thing.Duration != null)
            {
                timePart = string.Empty;
            }
            else
            {
                timePart = "[red]INVALID[/]";
            }

            var durationPart = CliTheme.Tag(CliTheme.Duration, ActivityFormat.DurationDisplay(thing));

            string categoryPart;
            if (thing.ActivityId != null)
            {
                var activity = this.profile.Data?.GetActivityById(thing.ActivityId);
                categoryPart = activity != null
                    ? CliTheme.Tag(CliTheme.Category, activity.Name ?? string.Empty)
                    : "[grey](unknown)[/]";
            }
            else
            {
                categoryPart = string.Empty;
            }

            var commentPart = CliTheme.Tag(CliTheme.Comment, thing.Comment);

            var parts = new[] { CliTheme.Tag(CliTheme.Timestamp, dateStr), timePart, durationPart, categoryPart, commentPart }
                .Where(s => s.Length > 0);
            this.console.MarkupLine(string.Join(" ", parts));
            return Task.CompletedTask;
        }

        public Task Visit(ITransactionItem thing)
        {
            this.console.WriteLine(thing.ToString() ?? string.Empty);
            return Task.CompletedTask;
        }
    }
}
