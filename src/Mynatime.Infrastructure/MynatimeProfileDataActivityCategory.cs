namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

public class MynatimeProfileDataActivityCategory : JsonObject
{
    public MynatimeProfileDataActivityCategory(JObject element)
        : base(null, element)
    {
    }

    public MynatimeProfileDataActivityCategory(string id, string name)
        : base(null, new JObject())
    {
        this.Id = id;
        this.Name = name;
    }

    public string? Id
    {
        get => this.Element.Value<string>("Id");
        set => this.Element["Id"] = value;
    }

    public string? Name
    {
        get => this.Element.Value<string>("Name");
        set => this.Element["Name"] = value;
    }

    public DateTime? Created
    {
        get => this.Element.Value<DateTime?>("Created");
        set => this.Element["Created"] = value;
    }

    public DateTime? LastUpdated
    {
        get => this.Element.Value<DateTime?>("LastUpdated");
        set => this.Element["LastUpdated"] = value;
    }

    public DateTime? Deleted
    {
        get => this.Element.Value<DateTime?>("Deleted");
        set => this.Element["Deleted"] = value;
    }

    public override string ToString()
    {
        return "ActCategory " + this.Id + " <" + this.Name + ">";
    }
}
