
namespace MynatimeCLI;

using System;
using MynatimeClient;

public class ActivityCommand : Command
{
    public ActivityCommand(IConsoleApp consoleApp)
        : base(consoleApp)
    {
    }

    public static string[] Args { get; } = new string[] { "act", "acti", "activ", "activit", "activity", "activities", };

    public override bool MatchArg(string arg)
    {
        return ConsoleApp.MatchArg(arg, Args);
    }

    public override bool ParseArgs(IConsoleApp consoleApp, string[] args, out int consumedArgs, out Command? command)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (i == 0 && this.MatchArg(arg))
            {
                // ok
            }
            else
            {
                goto error;
            }
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
        throw new NotImplementedException();
    }
}
