
namespace Mynatime.CLI;

using Spectre.Console;

public sealed class ProfileListCommand : Command
{
    public ProfileListCommand(IConsoleApp consoleApp, IAnsiConsole console)
        : base(consoleApp, console)
    {
    }

    public static string[] Args { get; } = new string[] { "list", "get", };

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Profiles";
        describe.AddCommandPattern(ProfileCommand.Args[0] + " " + Args[0], "lists user profiles");
        return describe;
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, ProfileCommand.Args);
    }

    public override bool ParseArgs(IConsoleApp consoleApp, string[] args, out int consumedArgs, out Command? command)
    {
        var i = -1;
        if (++i >= args.Length || !this.MatchArg(args[i]))
        {
            goto error;
        }

        if (++i >= args.Length)
        {
            // okay
        }
        else if (!ConsoleApp.MatchArg(args[i], Args))
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
        if (!this.App.AvailableProfiles.Any())
        {
            this.Console.WriteLine("No profiles found. ");
        }

        foreach (var profile in this.App.AvailableProfiles)
        {
            this.Console.Write("- ");
            this.Console.WriteLine(profile.ToString() ?? string.Empty);
            if (profile.FilePath != null)
            {
                this.Console.Write("  - ");
                this.Console.WriteLine(profile.FilePath);
            }
        }

        return Task.CompletedTask;
    }
}
