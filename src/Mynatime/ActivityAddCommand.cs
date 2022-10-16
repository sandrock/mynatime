
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using static Mynatime.Infrastructure.MynatimeConstants;

public class ActivityAddCommand : Command
{
    public ActivityAddCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.TimeZoneLocal = app.TimeZoneLocal;
    }

    public static string[] Args { get; } = new string[] { "add", "create", };

    public TimeZoneInfo TimeZoneLocal { get; set; }
    public DateTime? StartTimeLocal { get; set; }
    public DateTime? EndTimeLocal { get; set; }
    public decimal? DurationHours { get; set; }
    public string? CategoryArg { get; set; }
    public bool IsStart { get; set; }
    public bool IsStop { get; set; }
    public string Comment { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Add activity";
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + Args[0] + " [date] <duration> [category]", "adds an activity");
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + Args[0] + " <date> <time-start> [date-end] <time-end> [category]", "adds an activity");
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

        if (++i >= args.Length || !ConsoleApp.MatchArg(args[i], Args))
        {
            goto error;
        }

        bool acceptStartDate = true, acceptEndDate = false, acceptDuration = true, acceptStartTime = true, acceptEndTime = false, acceptCategory = true;
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
                this.StartTimeLocal = date;
                acceptStartDate = false;
                acceptEndDate = true;
            }
            else if (acceptEndDate && DateTime.TryParseExact(arg, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                this.EndTimeLocal = date;
                acceptStartDate = false;
                acceptEndDate = false;
            }
            else if ((acceptStartTime || acceptEndTime) && (match = TimeRegex.Match(arg)).Success)
            {
                var hours = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                var minutes = int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                var duration = new TimeSpan(0, hours, minutes, 0);
                if (acceptStartTime)
                {
                    acceptStartDate = false;
                    acceptStartTime = false;
                    acceptEndTime = true;
                    acceptDuration = true;
                    acceptCategory = true;
                    if (this.StartTimeLocal != null)
                    {
                        this.StartTimeLocal = this.StartTimeLocal.Value.Add(duration);
                    }
                    else
                    {
                        this.StartTimeLocal = this.App.TimeNowLocal.Date.Add(duration);
                    }
                }
                else
                {
                    acceptEndTime = false;
                    acceptDuration = false;
                    acceptCategory = true;
                    if (this.EndTimeLocal != null)
                    {
                        this.EndTimeLocal = this.EndTimeLocal.Value.Add(duration);
                    }
                    else
                    {
                        this.EndTimeLocal = this.StartTimeLocal!.Value.Date.Add(duration);
                    }
                }
            }
            else if (acceptDuration && (match = DurationRegex.Match(arg)).Success)
            {
                this.DurationHours = (decimal)int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    this.DurationHours = this.DurationHours.Value 
                      + (decimal)(double.Parse(match.Groups[2].Value.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture) / Math.Pow(10, match.Groups[2].Value.Length - 1) );
                }

                acceptDuration = false;
                acceptCategory = true;
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
        
        if (this.IsStart || this.IsStop)
        {
            throw new NotImplementedException();
        }
        
        if (this.StartTimeLocal == null)
        {
            this.StartTimeLocal = this.App.TimeNowLocal;
            if (this.EndTimeLocal != null)
            {
                throw new InvalidOperationException();
            }
            else
            {
                this.EndTimeLocal = this.StartTimeLocal;
            }
        }

        var page = new NewActivityItemPage();
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
                page.ActivityId = category.Id;
            }
            else
            {
                Console.WriteLine("Too many possibilities for category \"" + this.CategoryArg + "\": " + string.Join(", ", search));
                return;
            }
        }

        page.DateStart = this.StartTimeLocal;
        page.DateEnd = this.EndTimeLocal;
        page.Comment = this.Comment;
        if (this.DurationHours == null)
        {
            page.InAt = this.StartTimeLocal.Value.TimeOfDay;
            page.OutAt = this.EndTimeLocal.Value.TimeOfDay;
        }
        else
        {
            page.Duration = this.DurationHours.Value.ToInvariantString();
        }

        transaction.Add(page.ToTransactionItem(null, this.App.TimeNowUtc));

        await this.App.PersistProfile(profile);
    }
}
