
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Domain;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static Mynatime.Infrastructure.MynatimeConstants;

public class ActivityTrackingCommand : Command
{
    public ActivityTrackingCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.TimeZoneLocal = app.TimeZoneLocal;
    }

    public static string[] StartArgs { get; } = new string[] { "start", };
    public static string[] StopArgs { get; } = new string[] { "stop", };
    public static string[] StatusArgs { get; } = new string[] { "status", };
    public static string[] ClearArgs { get; } = new string[] { "clear", };

    public bool IsStart { get; set; }
    public bool IsStop { get; set; }
    public bool IsStatus { get; set; }
    public bool IsClear { get; set; }
    public TimeZoneInfo TimeZoneLocal { get; set; }
    public DateTime DateLocal { get; set; }
    public DateTime? TimeLocal { get; set; }
    public decimal? DurationHours { get; set; }
    public string? CategoryArg { get; set; }
    public string? Comment { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Activity tracker";
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StartArgs[0] + " [time] [category]", "starts an activity");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StopArgs[0] + " [time] [category]", "stops  an activity");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StatusArgs[0], "lists current activities");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + ClearArgs[0], "removes all events");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StartArgs[0] + "/" + StopArgs[0] + " [date] [time] [category]", "you can specify a date");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ActivityCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
        if (args.Length == 1 && this.MatchArg(args[0]))
        {
            // status!
            this.IsStatus = true;
            goto ok;
        }

        var i = -1;
        if (++i >= args.Length || !this.MatchArg(args[i]))
        {
            goto error;
        }

        if (++i >= args.Length)
        {
            goto error;
        }
        else if (ConsoleApp.MatchArg(args[i], StartArgs))
        {
            this.IsStart = true;
            this.TimeLocal = this.App.TimeNowLocal;
        }
        else if (ConsoleApp.MatchArg(args[i], StopArgs))
        {
            this.IsStop = true;
            this.TimeLocal = this.App.TimeNowLocal;
        }
        else if (ConsoleApp.MatchArg(args[i], StatusArgs))
        {
            this.IsStatus = true;
        }
        else if (ConsoleApp.MatchArg(args[i], ClearArgs))
        {
            this.IsClear = true;
        }
        else
        {
            goto error;
        }

        this.DateLocal = this.App.TimeNowLocal.Date;
        bool acceptStartTime = true, acceptStartDate = true, acceptCategory = true;
        DateTime date;
        Match match;
        var errors = 0;
        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;

            if (ConsoleApp.MatchArg(arg, "--message") || ConsoleApp.MatchShortArg(arg, "-m", out value))
            {
                if (value != null)
                {
                    this.Comment = value;
                }
                else if (nextArg != null)
                {
                    this.Comment = nextArg;
                    i++;
                }
                else
                {
                    errors++;
                    Console.WriteLine("Argument must be followed by a message. ");
                }
            }
            else if (acceptStartDate && DateTime.TryParseExact(arg, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                this.DateLocal = date.Date;
                acceptStartDate = false;
            }
            else if (acceptStartTime && (match = TimeRegex.Match(arg)).Success)
            {
                var hours = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                var minutes = int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                this.TimeLocal = this.DateLocal.AddHours(hours).AddMinutes(minutes);
                acceptStartTime = false;
            }
            else if (acceptCategory)
            {
                this.CategoryArg = arg;
                acceptCategory = false;
            }
            else
            {
                goto error;
            }
        }

        if (errors > 0)
        {
            goto error;
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

        var transaction = profile.Transaction;
        if (transaction == null)
        {
            Console.WriteLine("Current profile does not allow changes. ");
            return;
        }

        if (this.IsStatus && !(this.IsStart || this.IsStop || this.IsClear))
        {
        }
        else if (this.IsStart && !(this.IsStatus || this.IsStop || this.IsClear))
        {
        }
        else if (this.IsStop && !(this.IsStart || this.IsStatus || this.IsClear))
        {
        }
        else if (this.IsClear && !(this.IsStart || this.IsStatus || this.IsStop))
        {
        }
        else
        {
            Console.WriteLine("Neither start or stop or status or clear? ");
            return;
        }

        // find transaction item
        MynatimeProfileTransactionItem? transactionItem = null;
        foreach (var item in transaction.Items)
        {
            if (MynatimeProfileTransactionManager.Default.OfClass<ActivityStartStop>(item))
            {
                if (transactionItem == null)
                {
                    transactionItem = item;
                }
                else
                {
                    // too many of the same item type
                    Console.WriteLine("Transaction corrupted. ");
                    return;
                }
            }
        }

        bool hasChanged = false;
        ActivityStartStop state;
        if (transactionItem != null)
        {
            state = new ActivityStartStop(transactionItem);
        }
        else
        {
            state = new ActivityStartStop();
        }

        MynatimeProfileDataActivityCategory? category = null;
        if (this.CategoryArg != null)
        {
            if (profile.Data?.ActivityCategories == null)
            {
                Console.WriteLine("Please run \"act cat refresh\" before adding activity items. ");
                return;
            }

            var categories = profile.Data.ActivityCategories.Items.ToList();

            var categoriesByAlias = categories
               .Where(x => this.CategoryArg.Equals(x.Alias, StringComparison.OrdinalIgnoreCase))
               .ToArray();
            if (categoriesByAlias.Length == 1)
            {
                category = categoriesByAlias[0];
            }

            if (category == null)
            {
                var searchResult = await ActivityCategoryCommand.SearchItems(categories, this.CategoryArg, true);
                var search = searchResult.Select(x => x.Item).ToList();
                if (search.Count == 0)
                {
                    Console.WriteLine("No such category " + this.CategoryArg);
                    return;
                }
                else if (search.Count == 1)
                {
                    category = search[0];
                }
                else
                {
                    Console.WriteLine("Too many possibilities for category \"" + this.CategoryArg + "\": " + string.Join(", ", search));
                    return;
                }
            }

            if (category != null)
            {
            }
            else
            {
                Console.WriteLine("Category selection failed");
                return;
            }
        }

        if (this.IsStart || this.IsStop)
        {
            state.Add(this.TimeLocal!.Value, this.IsStart ? "Start" : this.IsStop ? "Stop" : "???", category?.Id, this.Comment);
            hasChanged = true;
        }

        if (this.IsClear)
        {
            state.Clear();
            hasChanged = true;
        }

        var manager = new ActivityStartStopManager(state);

        if (this.IsStatus)
        {
            Console.WriteLine("Events:");
            Console.WriteLine(state.GetSummary());

            foreach (var item in transaction.Items)
            {
                if (MynatimeProfileTransactionManager.Default.OfClass<NewActivityItemPage>(item))
                {
                    var activityPage = (NewActivityItemPage)MynatimeProfileTransactionManager.Default.GetInstanceOf(item);
                    manager.ExtraActivities.Add(new ActivityItemWrapper(activityPage));
                }
            }
        }

        manager.GenerateItems();

        if (manager.Errors.Any())
        {
            Console.WriteLine("Errors:");
            foreach (var error in manager.Errors)
            {
                Console.WriteLine("- " + error);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Activities:");
        foreach (var entry in manager.AllActivities)
        {
            Console.WriteLine("- " + entry.Item.ToDisplayString(profile.Data!) + (entry.IsStartAndStop ? " (start-stop)" : " (activity)"));
        }

        ////{
        ////    foreach (var item in state.Events.Except(manager.UsedEvents))
        ////    {
        ////        Console.WriteLine("- " + item.ToDisplayString(profile.Data));
        ////    }
        ////}

        if (hasChanged)
        {
            if (transactionItem != null)
            {
                state.ToTransactionItem(transactionItem, this.App.TimeNowUtc);
            }
            else
            {
                transactionItem = state.ToTransactionItem(null, this.App.TimeNowLocal);
                transaction.Add(transactionItem);
            }

            await this.App.PersistProfile(profile);
        }
    }
}
