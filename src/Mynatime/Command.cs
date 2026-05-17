
namespace Mynatime.CLI;

using Spectre.Console;

/// <summary>
/// Base class for a CLI command.
/// </summary>
public abstract class Command
{
    private readonly IConsoleApp app;
    private readonly IAnsiConsole console;

    protected Command(IConsoleApp app, IAnsiConsole console)
    {
        this.app = app ?? throw new ArgumentNullException(nameof(app));
        this.console = console ?? throw new ArgumentNullException(nameof(console));
    }

    protected IConsoleApp App => this.app;

    protected IAnsiConsole Console => this.console;

    public bool AutoLoadProfile { get; protected set; } = true;

    public abstract bool MatchArg(string arg);

    public abstract bool ParseArgs(IConsoleApp app, string[] args, out int consumedArgs, out Command? command);

    public abstract Task Run();

    public virtual CommandDescription Describe()
    {
        var describe = new CommandDescription(this.GetType().Name);
        return describe;
    }
}
