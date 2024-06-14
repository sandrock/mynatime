
namespace Mynatime.CLI;

using System;
using System.Globalization;
using Mynatime.Client;
using static Mynatime.Infrastructure.MynatimeConstants;

public sealed class ActivitySearchCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "search", };

    public ActivitySearchCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.client = client;
        this.TimeZoneLocal = app.TimeZoneLocal;
    }

    public TimeZoneInfo TimeZoneLocal { get; set; }

    public DateTime? MinDate { get; set; }
    public DateTime? MaxDate { get; set; }
    public List<string> Usernames { get; } = new();
    public List<string> Categories { get; } = new();

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Search published activity";
        describe.AddCommandPattern(ActivityCommand.Args[0] + " " + Args[0] + " [date-min] [date-max] [-u <user>] [-c <category>]", "searches for published activity");
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

        if (++i >= args.Length || !ConsoleApp.MatchArg(args[i], Args))
        {
            goto error;
        }

        var errors = 0;
        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null, key = null;
            DateTime date;
            if (ConsoleApp.MatchArg(arg, key = "-u"))
            {
                if (nextArg != null)
                {
                    this.Usernames.Add(nextArg);
                }
                else
                {
                    errors++;
                    Console.WriteLine($"Argument {key} must be followed by a user name");
                }
            }
            else if (ConsoleApp.MatchArg(arg, key = "-c"))
            {
                if (nextArg != null)
                {
                    this.Categories.Add(nextArg);
                }
                else
                {
                    errors++;
                    Console.WriteLine($"Argument {key} must be followed by a user name");
                }
            }
            else if (!arg.StartsWith("-") && (this.MinDate == null || this.MaxDate == null) && DateTime.TryParseExact(arg, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                if (this.MinDate == null)
                {
                    this.MinDate = date;
                }
                else
                {
                    this.MaxDate = date;
                }
            }
            else
            {
                errors++;
                Console.WriteLine($"Unknown argument \"{arg}\"");
            }
        }

        if (errors > 0)
        {
            goto error;
        }

        ////ok:
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

        var page = await this.client.GetActivityListPage();
    }
}
