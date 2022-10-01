
namespace Mynatime;

using System;
using Mynatime.Infrastructure;
using MynatimeClient;

/// <summary>
/// Lists pending changes. 
/// </summary>
public class StatusCommand : Command
{
    private readonly IManatimeWebClient client;

    public static string[] Args { get; } = new string[] { "status", };

    public StatusCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.client = client;
    }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.AddCommandPattern(Args[0], "lists pending changes");
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

        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;
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
        var profile = this.App.CurrentProfile;
        if (profile == null)
        {
            Console.WriteLine("No current profile. ");
            return Task.CompletedTask;
        }

        var operations = new List<MynatimeProfileTransactionItem>(0);
        if (profile.Transaction != null)
        {
            operations = profile.Transaction.Items.ToList();
        }

        if (profile.Transaction == null || operations.Count == 0)
        {
            Console.WriteLine("No pending operation. ");
            return Task.CompletedTask;
        }

        int i = -1;
        foreach (var operation in operations)
        {
            i++;
            Console.Write(i);
            Console.Write("\t");
            Console.WriteLine(operation);
        }

        return Task.CompletedTask;
    }
}
