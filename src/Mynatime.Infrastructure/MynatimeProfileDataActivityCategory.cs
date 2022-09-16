namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

public class MynatimeProfileDataActivityCategory : JsonObject
{
    public MynatimeProfileDataActivityCategory(JObject element)
        : base(null, element)
    {
    }

    public MynatimeProfileDataActivityCategory(string name)
        : base(null, new JObject())
    {
        this.Name = name;
    }

    public string? Name
    {
        get => this.Element.Value<string>("Name");
        set => this.Element["Name"] = value;
    }
}
