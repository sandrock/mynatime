﻿
namespace Mynatime.CLI;

using System.Globalization;
using Mynatime.Client;
using Mynatime.Domain;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using System;

/// <summary>
/// Lists pending changes. 
/// </summary>
public class StatusCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "status", };

    public StatusCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.client = client;
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
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

        var operations = new List<MynatimeProfileTransactionItem>(0);
        if (profile.Transaction != null)
        {
            operations = profile.Transaction.Items.ToList();
        }

        if (profile.Transaction == null || operations.Count == 0)
        {
            Console.WriteLine("No pending operation. ");
            return;
        }

        int i = -1;
        var visitor = new ConsoleDescribeTransactionItem(this.App, profile);
        var helper = MynatimeProfileTransactionManager.Default;
        
        foreach (var operation in operations)
        {
            i++;

            Console.Write(i);
            Console.Write("\t");
            var item = helper.GetInstanceOf(operation);
            await item.Accept(visitor);
        }

    }

    public class ConsoleDescribeTransactionItem : ITransactionItemVisitor
    {
        private readonly IConsoleApp app;
        private readonly MynatimeProfile profile;

        public ConsoleDescribeTransactionItem(IConsoleApp app, MynatimeProfile profile)
        {
            this.app = app;
            this.profile = profile;
        }

        public Task Visit(ActivityStartStop state)
        {
            Console.WriteLine("Activity tracker");
            var manager = new ActivityStartStopManager(state);
            manager.GenerateItems();

            if (manager.Errors.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var error in manager.Errors)
                {
                    Console.WriteLine("- " + error);
                }
            }

            if (manager.Warnings.Any())
            {
                Console.WriteLine("Warnings:");
                foreach (var error in manager.Warnings)
                {
                    Console.WriteLine("- " + error);
                }
            }

            Console.WriteLine("Activities:");
            foreach (var entry in manager.Activities)
            {
                Console.WriteLine("- " + entry.Item.ToDisplayString(profile.Data!));
            }

            {
                foreach (var item in state.Events.Except(manager.UsedEvents))
                {
                    Console.WriteLine("- " + item.ToDisplayString(profile.Data!));
                }
            }

            return Task.CompletedTask;
        }

        public Task Visit(NewActivityItemPage thing)
        {
            Console.WriteLine("Activity item ");
            Console.Write(thing.DateStart!.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
            Console.Write(" ");
            if (thing.DateEnd != null && thing.Duration == null && thing.InAt != null && thing.OutAt != null)
            {
                Console.Write(thing.InAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
                Console.Write(" ");
                if (thing.DateStart.Value.Date != thing.DateEnd.Value.Date)
                {
                    Console.Write(thing.DateEnd.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
                }
                else
                {
                    Console.Write(string.Empty.PadLeft(ClientConstants.DateInputFormat.Length));
                }
                
                Console.Write(" ");
                Console.Write(thing.OutAt.Value.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
                Console.Write(" ");   
            }
            else if (thing.DateEnd != null && thing.Duration != null && thing.InAt == null && thing.OutAt == null)
            {
                var spaces = ClientConstants.DateInputFormat.Length + ClientConstants.HourMinuteTimeFormat.Length*2 + 1;
                var duration = "for " + thing.Duration?.ToString() + " h";
                Console.Write(duration.PadRight(spaces, ' '));
            }
            else
            {
                Console.Write(" INVALID ACTIVITY");
            }

            if (thing.ActivityId != null)
            {
                var activity = this.profile.Data!.GetActivityById(thing.ActivityId);
                if (activity != null)
                {
                    Console.Write(activity.Name);
                }
                else
                {
                    Console.Write(thing.ActivityId);
                }
            }
            
            Console.Write(" ");
            Console.WriteLine();
            return Task.CompletedTask;
        }

        public Task Visit(ITransactionItem thing)
        {
            Console.WriteLine(thing.ToString());
            return Task.CompletedTask;
        }
    }
}
