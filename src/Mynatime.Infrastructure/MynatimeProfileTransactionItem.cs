namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

/// <summary>
/// Represents one pending action to the service.
/// </summary>
public sealed class MynatimeProfileTransactionItem : JsonObject
{
    public MynatimeProfileTransactionItem()
        : base("Item", new JObject())
    {
    }

    public MynatimeProfileTransactionItem(JObject element)
        : base("Item", element)
    {
    }

    public DateTime TimeCreatedUtc
    {
        get => this.Element.Value<DateTime>("TimeCreatedUtc");
        set => this.Element["TimeCreatedUtc"] = value.ToInvariantString();
    }

    public DateTime? TimeCommittedUtc
    {
        get => this.Element.Value<DateTime?>("TimeCommittedUtc");
        set => this.Element["TimeCommittedUtc"] = value;
    }

    public string? ObjectType
    {
        get => this.Element.Value<string>("ObjectType");
        set => this.Element["ObjectType"] = value;
    }

    public long? CommitId
    {
        get => this.Element.Value<long?>("CommitId");
        set => this.Element["CommitId"] = value;
    }

    public long? CommitItemId
    {
        get => this.Element.Value<long?>("CommitItemId");
        set => this.Element["CommitItemId"] = value;
    }

    public override string ToString()
    {
        return nameof(MynatimeProfileTransactionItem) + " " + (this.ObjectType ?? "???");
    }
}
