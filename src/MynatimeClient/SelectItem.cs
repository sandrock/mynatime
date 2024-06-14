namespace Mynatime.Client;

using Mynatime.Infrastructure;

public sealed class SelectItem
{
    public SelectItem()
    {
    }

    public SelectItem(string id, string displayName)
    {
        this.Id = id;
        this.DisplayName = displayName;
    }

    public string DisplayName { get; set; }

    public string Id { get; set; }

    public int Index { get; set; } = -1;

    public void UpdateFrom(MynatimeProfileDataActivityCategory match, DateTime time)
    {
        match.LastUpdated = time;
        match.Name = this.DisplayName;
    }

    public override string ToString()
    {
        return nameof(SelectItem) + " " + this.Id;
    }
}
