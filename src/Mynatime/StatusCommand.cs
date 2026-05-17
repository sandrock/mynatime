
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
            this.console.WriteLine("Activity tracker");
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
                foreach (var error in manager.Warnings)
                {
                    this.console.MarkupLine("[yellow]- " + Markup.Escape(error.ToString()) + "[/]");
                }
            }

            this.console.WriteLine("Activities:");
            foreach (var entry in manager.Activities)
            {
                this.console.WriteLine("- " + entry.Item.ToDisplayString(profile.Data!));
            }

            {
                foreach (var item in state.Events.Except(manager.UsedEvents))
                {
                    this.console.WriteLine("- " + item.ToDisplayString(profile.Data!));
                }
            }

            return Task.CompletedTask;
        }

        public Task Visit(NewActivityItemPage thing)
        {
            this.console.WriteLine("Activity item ");
            this.console.Write(thing.DateStart!.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
            this.console.Write(" ");
            if (thing.DateEnd != null && thing.Duration == null && thing.InAt != null && thing.OutAt != null)
            {
                this.console.Write(thing.InAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
                this.console.Write(" ");
                if (thing.DateStart.Value.Date != thing.DateEnd.Value.Date)
                {
                    this.console.Write(thing.DateEnd.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
                }
                else
                {
                    this.console.Write(string.Empty.PadLeft(ClientConstants.DateInputFormat.Length));
                }

                this.console.Write(" ");
                this.console.Write(thing.OutAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
                this.console.Write(" ");
            }
            else if (thing.DateEnd != null && thing.Duration != null && thing.InAt == null && thing.OutAt == null)
            {
                var spaces = ClientConstants.DateInputFormat.Length + ClientConstants.HourMinuteTimeFormat.Length * 2 + 1;
                var duration = "for " + thing.Duration?.ToString() + " h";
                this.console.Write(duration.PadRight(spaces, ' '));
            }
            else
            {
                this.console.MarkupLine("[red] INVALID ACTIVITY[/]");
            }

            if (thing.ActivityId != null)
            {
                var activity = this.profile.Data!.GetActivityById(thing.ActivityId);
                if (activity != null)
                {
                    this.console.Write(activity.Name ?? string.Empty);
                }
                else
                {
                    this.console.Write(thing.ActivityId);
                }
            }

            this.console.Write(" ");
            this.console.WriteLine(string.Empty);
            return Task.CompletedTask;
        }

        public Task Visit(ITransactionItem thing)
        {
            this.console.WriteLine(thing.ToString() ?? string.Empty);
            return Task.CompletedTask;
        }
    }
}
