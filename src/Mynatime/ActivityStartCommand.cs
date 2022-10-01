
namespace MynatimeCLI;

using Mynatime.Infrastructure;
using MynatimeClient;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class ActivityStartCommand : Command
{
    public ActivityStartCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.TimeZoneLocal = app.TimeZoneLocal;
    }

    public static string[] StartArgs { get; } = new string[] { "start", };
    public static string[] StopArgs { get; } = new string[] { "stop", };
    public static string[] StatusArgs { get; } = new string[] { "status", };

    public bool IsStart { get; set; }
    public bool IsStop { get; set; }
    public bool IsStatus { get; set; }
    public TimeZoneInfo TimeZoneLocal { get; set; }
    public DateTime? TimeLocal { get; set; }
    public decimal? DurationHours { get; set; }
    public string? CategoryArg { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StartArgs[0], "starts an activity");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StopArgs[0], "stops  an activity");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + StatusArgs[0], "lists current activities");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ActivityCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command)
    {
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
        else
        {
            goto error;
        }

        bool acceptDuration = true, acceptStartTime = true, acceptEndTime = false, acceptCategory = true;
        var timeRegex = new Regex("^(\\d?\\d)(\\d\\d)$");
        Match match;
        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;
            if (acceptStartTime && (match = timeRegex.Match(arg)).Success)
            {
                var hours = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                var minutes = int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                this.TimeLocal = this.App.TimeNowLocal.Date.AddHours(hours).AddMinutes(minutes);
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

        if (this.IsStatus && !(this.IsStart || this.IsStop))
        {
        }
        else if (this.IsStart && !(this.IsStatus || this.IsStop))
        {
        }
        else if (this.IsStop && !(this.IsStart || this.IsStatus))
        {
        }
        else
        {
            Console.WriteLine("Neither start or stop? ");
            return;
        }

        // find transaction item
        MynatimeProfileTransactionItem transactionItem = null;
        foreach (var item in transaction.Items)
        {
            if (item.ObjectType == typeof(ActivityStartStop).FullName)
            {
                if (transactionItem == null)
                {
                    transactionItem = item;
                }
                else
                {
                    Console.WriteLine("Transaction corrupted ");
                    return;
                }
            }
        }

        bool hasChanged = false, exists = false;
        ActivityStartStop state = null;
        if (transactionItem != null)
        {
            exists = true;
            state = new ActivityStartStop(transactionItem);
        }
        else
        {
            state = new ActivityStartStop();
        }
        
        
        MynatimeProfileDataActivityCategory category = null;
        if (this.CategoryArg != null)
        {
            if (profile.Data?.ActivityCategories == null)
            {
                Console.WriteLine("Please run \"act cat refresh\" before adding activity items. ");
                return;
            }

            var categories = profile.Data.ActivityCategories.Items.ToList();
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

        if (this.IsStart || this.IsStop)
        {

            state.Add(this.TimeLocal.Value, this.IsStart ? "start" : this.IsStop ? "stop" : "???", category?.Id);
            hasChanged = true;
        }

        Console.WriteLine(state.GetSummary());

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
