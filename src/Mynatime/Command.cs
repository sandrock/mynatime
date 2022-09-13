
namespace Mynatime;

/// <summary>
/// Base class for a CLI command. 
/// </summary>
public abstract class Command
{
    private readonly App? app;

    protected Command()
    {
    }

    protected Command(App app)
    {
        this.app = app ?? throw new ArgumentNullException(nameof(app));
    }

    protected App App => this.app ?? throw new InvalidOperationException("App is not set. ");

    public abstract bool MatchArg(string arg);

    public abstract bool ParseArgs(App app, string[] args, out int consumedArgs, out Command? command);

    public abstract Task Run();
}