
namespace Mynatime;

public sealed class ListProfilesCommand : Command
{
    public ListProfilesCommand()
        : base()
    {
    }

    private ListProfilesCommand(IConsoleApp consoleApp)
        : base(consoleApp)
    {
    }

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, "profiles");
    }

    public override bool ParseArgs(IConsoleApp consoleApp, string[] args, out int consumedArgs, out Command? command)
    {
        consumedArgs = 0;
        command = null;

        if (args.Length < 1 || !this.MatchArg(args[0]))
        {
            return false;
        }

        command = new ListProfilesCommand(consoleApp);
        return true;
    }

    public override Task Run()
    {
        if (!this.App.AvailableProfiles.Any())
        {
            Console.WriteLine("No profiles found. ");
        }

        foreach (var profile in this.App.AvailableProfiles)
        {
            Console.Write("- ");
            Console.WriteLine(profile);
            Console.Write("  - ");
            Console.WriteLine(profile.FilePath);
        }

        return Task.CompletedTask;
    }
}
