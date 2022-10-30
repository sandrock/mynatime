
namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
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
        this.commands.Add(new HelpCommand(this));
        this.commands.Add(new ProfileListCommand(this));
        this.commands.Add(new ProfileAddCommand(this, this.client));
        this.commands.Add(new ActivityCommand(this));
        this.commands.Add(new ActivityCategoryCommand(this, this.client));
        this.commands.Add(new ActivityAddCommand(this, this.client));
        this.commands.Add(new ActivityTrackingCommand(this, this.client));
        this.commands.Add(new StatusCommand(this, this.client));
        this.commands.Add(new CommitCommand(this, this.client));

        // call static constructors. this is wrong.
        new ActivityStartStop();
        new NewActivityItemPage();
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

    public DateTime TimeNowLocal { get => DateTime.Now; }

    public DateTime TimeNowUtc { get => DateTime.UtcNow; }

    public TimeZoneInfo TimeZoneLocal { get => TimeZoneInfo.Local; }

    public IEnumerable<Command> Commands { get => this.commands.AsReadOnly(); }

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

        if (await this.SelectCurrentProfile())
        {
        }

        if (this.ShowConsoleErrors())
        {
            this.ExitCode = 3;
            return;
        }

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

    public static string AskForPassword(string label)
    {
        var consoleOut = Console.Out;
        var consoleIn = Console.In;
        
        var pass = new List<char>();
        ConsoleKeyInfo key;
        consoleOut.Write(label);

        do
        {
            try
            {
                key = Console.ReadKey(true);
            }
            catch (InvalidOperationException ex)
            {
                // System.InvalidOperationException: Cannot read keys when either application does not have a console or when console input has been redirected from a file.
                Trace.WriteLine("Failed to Console.ReadKey: " + ex.Message);
                consoleOut.WriteLine("WARNING: your terminal emulator does not support typing hidden passwords.");
                consoleOut.WriteLine("WARNING: you can type a password but IT WILL BE VISIBLE ON THE SCREEN!");
                consoleOut.Write(label);
                return consoleIn.ReadLine();
            }

            if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }

            // Backspace Should Not Work
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass.Add(key.KeyChar);
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Count > 0)
                {
                    pass.RemoveAt(pass.Count - 1);
                    Console.Write("\b \b");
                }
            }
        }

        // Stops Receiving Keys Once Enter is Pressed
        while (key.Key != ConsoleKey.Enter);

        return new string(pass.ToArray());
    }

    public async Task PersistProfile(MynatimeProfile profile)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        string filePath;
        if (profile.FilePath != null)
        {
            filePath = profile.FilePath;
        }
        else
        {
            DirectoryInfo directory;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (this.ConfigDirectory != null)
            {
                directory = new DirectoryInfo(this.ConfigDirectory);
                if (!directory.Exists)
                {
                    directory.Create();
                }
            }
            else
            {
                directory = MynatimeConfiguration.EnsureConfigDirectory();
            }

            var name = MynatimeConfiguration.GetNewProfileFileName();
            filePath = Path.Combine(directory.FullName, name);
        }

        await profile.SaveToFile(filePath);

        Console.WriteLine("Profile saved to: " + filePath);
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
        if (arg.Length < name.Length)
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns>return true if there are errors; otherwise, false</returns>
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
        foreach (var file in MynatimeProfiles.DiscoverProfileFiles(this.ConfigDirectory))
        {
            var config = await this.OpenProfileByFilePath(file);
        }
    }

    private async Task<bool> SelectCurrentProfile()
    {
        if (this.availableProfiles.Count == 0)
        {
            return false;
        }

        // try open by file path
        if (this.OpenProfileName != null)
        {
            var fileInfo = new FileInfo(this.OpenProfileName);
            if (fileInfo.Exists)
            {
                var config = await this.OpenProfileByFilePath(fileInfo);
                if (config != null)
                {
                    this.CurrentProfile = config;
                    return true;
                }
            }

            // maybe path is relative to config dir?
            fileInfo = new FileInfo(Path.Combine(this.ConfigDirectory, this.OpenProfileName));
            if (fileInfo.Exists)
            {
                var config = await this.OpenProfileByFilePath(fileInfo);
                if (config != null)
                {
                    this.CurrentProfile = config;
                    return true;
                }
            }

            ////this.AddConsoleError("Found no profile matching argument \"" + this.OpenProfileName+"\". ");
            ////return false;
        }

        if (this.availableProfiles.Count == 0)
        {
            return false;
        }
        else if (this.availableProfiles.Count == 1)
        {
            this.CurrentProfile = this.availableProfiles.Single();
            Console.WriteLine("Using profile: " + this.CurrentProfile.FilePath);
            return true;
        }
        else
        {
            if (this.OpenProfileName == null)
            {
                this.CurrentProfile = this.availableProfiles.FirstOrDefault(x => x.IsDefault == true)
                 ?? this.availableProfiles.First();
                Console.WriteLine("Using profile: " + this.CurrentProfile.FilePath);
                return true;
            }
            else
            {
                var matches = new List<MynatimeProfile>();
                foreach (var profile in this.availableProfiles)
                {
                    var profileFile = profile.FilePath != null ? new FileInfo(profile.FilePath) : null;
                    var fileNameWithoutExtension = profileFile != null ? (profileFile.Name.Substring(0, profileFile.Name.Length - profileFile.Extension.Length)) : null;
                    if (profileFile != null && profileFile.Name.Equals(this.OpenProfileName, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(profile);
                    }
                    else if (fileNameWithoutExtension != null && fileNameWithoutExtension.Equals(this.OpenProfileName, StringComparison.OrdinalIgnoreCase))
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
                    Console.WriteLine("Using profile: " + this.CurrentProfile.FilePath);
                    return true;
                }

                return false;
            }
        }
    }

    private async Task<MynatimeProfile?> OpenProfileByFilePath(FileInfo file)
    {
        try
        {
            var config = await MynatimeProfile.LoadFromFile(file.FullName);
            this.availableProfiles.Add(config);
            return config;
        }
        catch (InvalidOperationException ex)
        {
            this.log.LogWarning("Loading profile <{0}> failed: {1}", file.FullName, ex.Message);
            return null;
        }
    }

    private void AddConsoleError(string message)
    {
        this.consoleErrors.Add(message);
    }
}
