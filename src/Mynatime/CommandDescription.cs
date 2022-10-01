namespace Mynatime;

using MynatimeClient;

public sealed class CommandDescription
{
    private readonly List<SelectItem> commandPatterns;

    public CommandDescription()
    {
        this.commandPatterns = new List<SelectItem>();
    }

    public IReadOnlyList<SelectItem> CommandPatterns { get => this.commandPatterns; }

    public CommandDescription AddCommandPattern(string args, string description)
    {
        this.commandPatterns.Add(new SelectItem() { Id = args, DisplayName = description, });
        return this;
    }
}
