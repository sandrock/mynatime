namespace Mynatime.CLI;

using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using Spectre.Console;

/// <summary>
/// Displays the status of the current profile.
/// </summary>
public sealed class ProfileStatusCommand : Command
{
    public static string[] Args { get; } = new string[] { "status", "sta", };

    public ProfileStatusCommand(IConsoleApp consoleApp, IAnsiConsole console)
        : base(consoleApp, console)
    {
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Profiles";
        describe.AddCommandPattern(ProfileCommand.Args[0] + " " + Args[0], "displays current profile status");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ProfileCommand.Args);
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

    public override Task Run()
    {
        var profile = this.App.CurrentProfile;
        if (profile == null)
        {
            this.Console.WriteLine("No profile loaded. ");
            return Task.CompletedTask;
        }

        // ── identity ──────────────────────────────────────────────────────
        this.Console.WriteLine("Profile:    " + (profile.FilePath != null ? Path.GetFileName(profile.FilePath) : "(unsaved)"));
        this.Console.WriteLine("  File:     " + (profile.FilePath ?? "(none)"));
        this.Console.WriteLine("  Username: " + (profile.LoginUsername ?? "(unknown)"));

        var identity = profile.Identity;
        if (identity != null)
        {
            var name = identity.Value<string>("name");
            if (!string.IsNullOrEmpty(name))
            {
                this.Console.WriteLine("  Name:     " + name);
            }
        }

        if (profile.UserId != null)
        {
            this.Console.WriteLine("  User ID:  " + profile.UserId);
        }

        var companyName = GetCompanyName(profile);
        if (companyName != null)
        {
            this.Console.WriteLine("  Company:  " + companyName);
        }

        var roles = identity?["roles"] as JArray;
        if (roles != null && roles.Count > 0)
        {
            this.Console.WriteLine("  Roles:    " + string.Join(", ", roles.Select(r => r.Value<string>())));
        }

        // ── session ───────────────────────────────────────────────────────
        var cookies = profile.Cookies;
        if (cookies != null && cookies.Count > 0)
        {
            this.Console.WriteLine("  Session:  active (" + cookies.Count + " cookies)");
        }
        else
        {
            this.Console.WriteLine("  Session:  none");
        }

        // ── cached data ───────────────────────────────────────────────────
        var categories = profile.Data?.ActivityCategories;
        var categoryCount = categories?.Items.Count() ?? 0;
        if (categoryCount > 0)
        {
            var lastUpdated = categories!.LastUpdated;
            this.Console.WriteLine("  Categories: " + categoryCount + " categories"
                + (lastUpdated != null ? ", last refreshed " + lastUpdated.Value.ToLocalTime().ToString("yyyy-MM-dd") : string.Empty));
        }
        else
        {
            this.Console.WriteLine("  Categories: none (run 'act cat refresh')");
        }

        // ── transaction ───────────────────────────────────────────────────
        var pendingCount = profile.Transaction?.Items.Count() ?? 0;
        this.Console.WriteLine("  Pending:    " + pendingCount + " item" + (pendingCount != 1 ? "s" : string.Empty));

        var commitCount = profile.Commits?.Items.Count() ?? 0;
        this.Console.WriteLine("  Commits:    " + commitCount + " item" + (commitCount != 1 ? "s" : string.Empty));

        return Task.CompletedTask;
    }

    private static string? GetCompanyName(MynatimeProfile profile)
    {
        // try Identity.company.name (old analytics path)
        var company = profile.Identity?["company"] as JObject;
        if (company != null)
        {
            var name = company.Value<string>("name");
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        // try Group.name (old analytics path)
        var group = profile.Group;
        if (group != null)
        {
            var name = group.Value<string>("name");
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        return null;
    }
}
