namespace Mynatime.Infrastructure;

using Mynatime.Infrastructure.ProfileTransaction;
using Newtonsoft.Json.Linq;

/// <summary>
/// Contains pending actions to the service.
/// </summary>
public class MynatimeProfileTransaction : JsonObject
{
    private DateTime dateTime;

    public MynatimeProfileTransaction()
        : base("Transaction", new JObject())
    {
        dateTime = dateTime;
    }

    internal MynatimeProfileTransaction(JObject element)
        : base("Transaction", element)
    {
        dateTime = dateTime;
    }

    public IEnumerable<MynatimeProfileTransactionItem> Items
    {
        get
        {
            if (this.Element.TryGetValue("Items", out JToken? items) && items is JArray array)
            {
                return array.Select(x => new MynatimeProfileTransactionItem((JObject)x)); 
            }
            else
            {
                return Enumerable.Empty<MynatimeProfileTransactionItem>();
            }
        }
    }

    public void Add(MynatimeProfileTransactionItem item)
    {
        JArray array;
        if (this.Element.TryGetValue("Items", out JToken? items) && items != null)
        {
            array = (JArray)items;
        }
        else
        {
            this.Element["Items"] = array = new JArray();
        }

        array.Add(item.Element);
    }
}
