
namespace Mynatime;

using Mynatime;
using Mynatime.Infrastructure;
using MynatimeClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

/// <summary>
/// The command line application. 
/// </summary>
public class ConsoleApp : IConsoleApp
{
    private readonly ILogger<ConsoleApp> log;
    private readonly IOptions<AppSettings> appSettings;
    private readonly IManatimeWebClient client;
    private readonly List<string> consoleErrors = new List<string>();
    private readonly List<MynatimeProfile> availableProfiles = new List<MynatimeProfile>();
    private readonly List<Command?> commands;

    public ConsoleApp(ILogger<ConsoleApp> log, IOptions<AppSettings> appSettings, IManatimeWebClient client)
    {
        this.log = log;
        this.appSettings = appSettings;
        this.client = client;
        this.commands = new List<Command?>();
        this.commands.Add(new ListProfilesCommand());
        this.commands.Add(new ActivityCommand(this));
        this.commands.Add(new ActivityCategoryCommand(this, this.client));
    }

    /// <summary>
    /// Gets or sets the configuration directory to use for profiles. 
    /// </summary>
    public string ConfigDirectory { get; set; } = MynatimeConfiguration.GetConfigDirectory().FullName;

    /// <summary>
    /// Sets the app exit code. 
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets or sets the name of the profile to use. 
    /// </summary>
    public string? OpenProfileName { get; set; }

    /// <summary>
    /// Gets or sets the active profile. 
    /// </summary>
    public MynatimeProfile? CurrentProfile { get; set; }

    /// <summary>
    /// Gets or sets the current command. 
    /// </summary>
    public Command? Command { get; set; }

    /// <summary>
    /// Gets or sets the discovered profiles. 
    /// </summary>
    public IEnumerable<MynatimeProfile> AvailableProfiles => this.availableProfiles;

    /// <summary>
    /// Parse CLI arguments and run the specified command. 
    /// </summary>
    /// <param name="args"></param>
    public async Task Run(string[] args)
    {
        Console.WriteLine(this.appSettings.Value.Title);
        this.log.LogInformation("App run. ");

        this.ParseArgs(args);

        if (this.ShowConsoleErrors())
        {
            this.ExitCode = 1;
            return;
        }

        await this.DiscoverProfiles();

        if (this.SelectCurrentProfile())
        {
        }

        this.ShowConsoleErrors();

        if (this.Command != null)
        {
            await this.ExecuteCommand(this.Command);
        }
        else
        {
            this.AddConsoleError("No command. ");
            this.ShowConsoleErrors();
            this.ExitCode = 2;
            return;
        }
    }

    internal static bool MatchArg(string arg, params string[] values)
    {
        foreach (var value in values)
        {
            if (value.Equals(arg, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool MatchShortArg(string arg, string name, out string? value)
    {
        value = null;
        if (arg.Length < name.Length + 1)
        {
            // "-" is too short to match "-d"
            return false;
        }

        if (!arg.StartsWith(name, StringComparison.Ordinal))
        {
            // "-c" does not match "-d"
            return false;
        }

        if (arg.Length > name.Length)
        {
            value = arg.Substring(name.Length);
        }

        return true;
    }

    internal static bool MatchShortArg(string arg, string name)
    {
        if (arg.Length < name.Length + 1)
        {
            // "-" is too short to match "-d"
            return false;
        }

        if (!arg.Equals(name, StringComparison.Ordinal))
        {
            // "-c" does not match "-d"
            return false;
        }

        return true;
    }

    private async Task ExecuteCommand(Command command)
    {
        await command.Run();
    }

    private bool ShowConsoleErrors()
    {
        if (this.consoleErrors.Count > 0)
        {
            Console.WriteLine("Some errors occured: ");
            Console.WriteLine();
            foreach (var error in this.consoleErrors)
            {
                Console.Write("ERROR: ");
                Console.WriteLine(error);
            }

            return true;
        }
        else
        {
            this.consoleErrors.Clear();
            return false;
        }
    }

    private void ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = (i + 1) < args.Length ? args[i + 1] : default(string);
            
            if (MatchArg(arg, "--config-directory"))
            {
                if (nextArg != null)
                {
                    this.ConfigDirectory = nextArg;
                    i++;
                }
                else
                {
                    this.AddConsoleError("Argument " + arg + " must be followed by a path. ");
                }
            }
            else if (MatchShortArg(arg, "-p", out string? value) || MatchArg(arg, "--profile"))
            {
                if (value != null)
                {
                    this.OpenProfileName = value;
                }
                else if (nextArg != null)
                {
                    this.OpenProfileName = nextArg;
                    i++;
                }
                else
                {
                    this.AddConsoleError("Argument " + arg + " must be followed by a profile identifier. ");
                }
            }
            else if (this.Command == null)
            {
                foreach (var command in this.GetCommandsByArg(arg))
                {
                    var remainingArgs = args.Skip(i).ToArray();
                    if (command.ParseArgs(this, remainingArgs, out int consumedArgs, out Command? command1))
                    {
                        this.Command = command1;
                    }

                    i += consumedArgs;
                }
            }
            else
            {
                this.AddConsoleError("Unknown argument " + arg + ". ");
            }
        }
    }

    private IEnumerable<Command> GetCommandsByArg(string arg)
    {
        foreach (var command in this.commands)
        {
            Debug.Assert(command != null, nameof(command) + " != null");
            if (command.MatchArg(arg))
            {
                yield return command;
            }
        }
    }

    private async Task DiscoverProfiles()
    {
        if (Directory.Exists(this.ConfigDirectory))
        {
            foreach (var path in Directory.EnumerateFiles(this.ConfigDirectory, "*.json"))
            {
                MynatimeProfile config;
                try
                {
                    config = await MynatimeProfile.LoadFromFile(path);
                    this.availableProfiles.Add(config);
                }
                catch (InvalidOperationException ex)
                {
                    this.log.LogWarning("Loading profile <{0}> failed: {1}", path, ex.Message);
                }
            }
        }
    }

    private bool SelectCurrentProfile()
    {
        if (this.availableProfiles.Count == 0)
        {
            return false;
        }

        if (this.OpenProfileName == null)
        {
            this.CurrentProfile = this.availableProfiles.First();
            return true;
        }
        else
        {
            var matches = new List<MynatimeProfile>();
            foreach (var profile in this.availableProfiles)
            {
                if (profile.FilePath != null && profile.FilePath.EndsWith(this.OpenProfileName, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(profile);
                }
                else if (profile.LoginUsername != null && this.OpenProfileName.Equals(profile.LoginUsername, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(profile);
                }
            }

            if (matches.Count == 1)
            {
                this.CurrentProfile = matches.Single();
                return true;
            }

            return false;
        }
    }

    private void AddConsoleError(string message)
    {
        this.consoleErrors.Add(message);
    }
}
