
namespace Mynatime;

using System;
using MynatimeClient;

public sealed class ActivityCategoryCommand : Command
{
    private readonly IManatimeWebClient manatimeWebClient;

    public ActivityCategoryCommand(IConsoleApp consoleApp, IManatimeWebClient manatimeWebClient)
        : base(consoleApp)
    {
        this.manatimeWebClient = manatimeWebClient;
    }

    public static string[] Args { get; } = new string[] { "cat", "cate", "categ", "category", "categories", };

    public bool DoRefresh { get; set; } = true;

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

        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            if (ConsoleApp.MatchArg(arg, "--cached") || ConsoleApp.MatchShortArg(arg, "-c"))
            {
                this.DoRefresh = false;
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

    public override Task Run()
    {
        if (this.DoRefresh)
        {
            throw new NotImplementedException();
        }

        if (this.App.CurrentProfile == null)
        {
            throw new InvalidOperationException("No current profile. ");
        }

        var store = this.App.CurrentProfile.Data?.ActivityCategories;

        if (store != null)
        {
            foreach (var item in store.Items)
            {
                Console.WriteLine(item.ToString());
            }
        }

        return Task.CompletedTask;
    }
}
