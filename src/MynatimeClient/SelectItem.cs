namespace MynatimeClient;

using Mynatime.Infrastructure;

public sealed class SelectItem
{
    public string DisplayName { get; set; }

    public string Id { get; set; }

    public int Index { get; set; } = -1;

    public void Update(MynatimeProfileDataActivityCategory match, DateTime time)
    {
        match.LastUpdated = time;
        match.Name = this.DisplayName;
    }

    public override string ToString()
    {
        return nameof(SelectItem) + " " + this.Id;
    }
}
