namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

/// <summary>
/// Represents one pending action to the service.
/// </summary>
public class MynatimeProfileTransactionItem : JsonObject
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
        set => this.Element["TimeCreatedUtc"] = value;
    }

    public string? ObjectType
    {
        get => this.Element.Value<string>("ObjectType");
        set => this.Element["ObjectType"] = value;
    }
}
