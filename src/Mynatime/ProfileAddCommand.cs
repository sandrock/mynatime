
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using System;

public class ProfileAddCommand : Command
{
    private readonly IManatimeWebClient client;

    public ProfileAddCommand(IConsoleApp app, IManatimeWebClient client)
        : base(app)
    {
        this.client = client;
        this.AutoLoadProfile = false;
    }

    public static string[] Args { get; } = new string[] { "add", "create", };

    public string? LoginUsername { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.AddCommandPattern(ProfileCommand.Args[0] + " " + Args[0], "adds user profiles");
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

        for (++i; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);

            string? value = null;
            if (ConsoleApp.MatchArg(arg, "--email", "email") || ConsoleApp.MatchShortArg(arg, "-e", out value))
            {
                if (value != null)
                {
                    this.LoginUsername = value;
                }
                else if (nextArg != null)
                {
                    this.LoginUsername = nextArg;
                    i++;
                }
                else
                {
                    Console.WriteLine("Email argument requires a value. ");
                }
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

    public override async Task Run()
    {
        var loginPage = await this.client.PrepareEmailPasswordAuthenticate();
        if (!loginPage.Succeed)
        {
            Console.WriteLine(loginPage);
            return;
        }

        Console.WriteLine("Creating a new profile. Please authenticate. ");
        while (string.IsNullOrWhiteSpace(this.LoginUsername))
        {
            Console.Write("Email address> ");
            this.LoginUsername = Console.ReadLine();
        }

        var password = default(string);
        while (string.IsNullOrEmpty(password))
        {
            password = ConsoleApp.AskForPassword("Password>      ");
        }

        Console.WriteLine("Processing... ");
        var resultPage = await this.client.EmailPasswordAuthenticate(this.LoginUsername, password);
        Console.WriteLine();
        if (resultPage.Succeed)
        {
            Console.WriteLine("OK. ");
        }
        else
        {
            Console.WriteLine(resultPage);
            return;
        }

        var profile = new MynatimeProfile();
        profile.LoginUsername = this.LoginUsername;
        profile.LoginPassword = password;
        profile.UserId = resultPage.UserId;
        profile.GroupId = resultPage.GroupId;

        if (resultPage.Identity != null)
        {
            profile.Identity = (JObject)resultPage.Identity.DeepClone();
        }

        if (resultPage.Group != null)
        {
            profile.Group = (JObject)resultPage.Group.DeepClone();
        }

        profile.Cookies = this.client.GetCookies();

        await this.App.PersistProfile(profile);
    }
}
