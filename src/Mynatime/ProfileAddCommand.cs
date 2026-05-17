
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;

public class ProfileAddCommand : Command
{
    private readonly IManatimeWebClient client;

    public ProfileAddCommand(IConsoleApp app, IManatimeWebClient client, IAnsiConsole console)
        : base(app, console)
    {
        this.client = client;
        this.AutoLoadProfile = false;
    }

    public static string[] Args { get; } = new string[] { "add", "create", };

    public string? LoginUsername { get; set; }

    public override CommandDescription Describe()
    {
        var describe = base.Describe();
        describe.Title = "Profiles";
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
                    this.Console.WriteLine("Email argument requires a value. ");
                }
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

    public override async Task Run()
    {
        BaseResult loginPage = null!;
        await this.Console.Status().StartAsync("Connecting...", async _ =>
        {
            loginPage = await this.client.PrepareEmailPasswordAuthenticate();
        });

        if (!loginPage.Succeed)
        {
            this.Console.MarkupLine("[red]" + Markup.Escape(loginPage.ToString()!) + "[/]");
            return;
        }

        this.Console.WriteLine("Creating a new profile. Please authenticate. ");

        if (string.IsNullOrWhiteSpace(this.LoginUsername))
        {
            this.LoginUsername = this.Console.Ask<string>("Email address>");
        }

        var password = this.Console.Prompt(
            new TextPrompt<string>("Password>     ")
                .Secret());

        this.Console.WriteLine(string.Empty);

        LoginResult resultPage = null!;
        await this.Console.Status().StartAsync("Authenticating...", async _ =>
        {
            resultPage = await this.client.EmailPasswordAuthenticate(this.LoginUsername, password);
        });

        this.Console.WriteLine(string.Empty);
        if (resultPage.Succeed)
        {
            this.Console.MarkupLine("[green]OK.[/]");
        }
        else
        {
            this.Console.MarkupLine("[red]" + Markup.Escape(resultPage.ToString()!) + "[/]");
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
