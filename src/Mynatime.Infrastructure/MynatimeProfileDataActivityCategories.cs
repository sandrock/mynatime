namespace Mynatime.Infrastructure;

using Newtonsoft.Json.Linq;

public class MynatimeProfileDataActivityCategories : JsonObject
{
    public MynatimeProfileDataActivityCategories(JObject element)
        : base("ActivityCategories", element)
    {
    }

    public IEnumerable<MynatimeProfileDataActivityCategory> Items
    {
        get
        {
            var array = this.GetItemsArray(false);
            if (array != null)
            {
                foreach (var token in array)
                {
                    var entry = (JObject)token;
                    yield return new MynatimeProfileDataActivityCategory(entry);
                }
            }
        }
    }

    public DateTime? LastUpdated
    {
        get => this.Element.Value<DateTime?>("LastUpdated");
        set => this.Element["LastUpdated"] = value;
    }

    public void Add(MynatimeProfileDataActivityCategory category)
    {
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category));
        }

        var array = this.GetItemsArray(true)!;
        array.Add(category.Element);
    }

    private JArray? GetItemsArray(bool allowCreate)
    {
        JArray array = null;
        if (this.Element.TryGetValue("Items", out JToken? token))
        {
            if (token is JArray)
            {
                array = (JArray)token;
            }
        }

        if (array == null && allowCreate)
        {
            array = new JArray();
            this.Element["Items"] = array;
        }

        return array;
    }
}
