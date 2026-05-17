namespace Mynatime.CLI;

using Spectre.Console;

public sealed class HelpCommand : Command
{
    public HelpCommand(IConsoleApp app, IAnsiConsole console)
        : base(app, console)
    {
    }

    public static string[] Args { get; } = new string[] { "help", "--help", "-h", "/?", "/help", "h", };

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Help";
        describe.AddCommandPattern(Args[0], "displays help");
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
        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine("Mynatime help");
        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine("Usage: mynatime [app options] [command] [command options]");
        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine("App options:");
        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine("  --config-directory <directory>   specifies the directory to open profiles from");
        this.Console.WriteLine("  --profile <profile>              specifies the profile to use");
        this.Console.WriteLine("  -p <profile>");
        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine("Commands: ");
        this.Console.WriteLine(string.Empty);
        foreach (var item in this.App.Commands)
        {
            int patternLength = 32;
            var describe = item.Describe();
            if (describe.CommandPatterns.Count == 0)
            {
                continue;
            }

            foreach (var pattern in describe.CommandPatterns)
            {
                if (pattern.Id.Length > patternLength)
                {
                    patternLength = pattern.Id.Length;
                }
            }

            this.Console.MarkupLine("  [bold]## " + Markup.Escape(describe?.Title ?? item.GetType().Name) + "[/]");
            foreach (var pattern in describe!.CommandPatterns)
            {
                if (pattern.Id.Length > patternLength)
                {
                    patternLength = pattern.Id.Length;
                }

                this.Console.WriteLine("  " + pattern.Id.PadRight(patternLength + 1, ' ') + pattern.DisplayName);
            }

            this.Console.WriteLine(string.Empty);
        }

        this.Console.WriteLine(string.Empty);
        this.Console.WriteLine(string.Empty);
        return Task.CompletedTask;
    }
}
