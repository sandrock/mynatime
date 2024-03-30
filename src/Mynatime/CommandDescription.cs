namespace Mynatime.CLI;

using Mynatime.Client;

public sealed class CommandDescription
{
    private readonly List<SelectItem> commandPatterns;

    public CommandDescription(string title)
    {
        this.Title = title;
        this.commandPatterns = new List<SelectItem>();
    }

    public string Title { get; set; }

    public IReadOnlyList<SelectItem> CommandPatterns { get => this.commandPatterns; }

    public CommandDescription AddCommandPattern(string args, string description)
    {
        this.commandPatterns.Add(new SelectItem() { Id = args, DisplayName = description, });
        return this;
    }
}
