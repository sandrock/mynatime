namespace Mynatime;

public sealed class HelpCommand : Command
{
    public HelpCommand(ConsoleApp app)
        : base(app)
    {
    }

    public static string[] Args { get; } = new string[] { "--help", "help", "-h", "/?", "/help", };

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

        ok:
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
        Console.WriteLine("");
        Console.WriteLine("Mynatime help");
        Console.WriteLine("");
        Console.WriteLine("Usage: mynatime [app options] [command] [command options]");
        Console.WriteLine("");
        Console.WriteLine("App options:");
        Console.WriteLine("");
        Console.WriteLine("  --config-directory <directory>   specifies the directory to open profiles from");
        Console.WriteLine("  --profile <profile>              specifies the profile to use");
        Console.WriteLine("  -p <profile>");
        Console.WriteLine("");
        Console.WriteLine("Commands: ");
        Console.WriteLine("");
        foreach (var item in this.App.Commands)
        {
            Console.WriteLine("  (" + item.GetType().Name + ")");
        }

        Console.WriteLine("");
        Console.WriteLine("");
        return Task.CompletedTask;
    }
}
