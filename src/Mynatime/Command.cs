
namespace Mynatime.CLI;

/// <summary>
/// Base class for a CLI command. 
/// </summary>
public abstract class Command
{
    private readonly IConsoleApp? app;

    protected Command()
    {
    }

    protected Command(IConsoleApp consoleApp)
    {
        this.app = consoleApp ?? throw new ArgumentNullException(nameof(consoleApp));
    }

    protected IConsoleApp App => this.app ?? throw new InvalidOperationException("App is not set. ");

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
