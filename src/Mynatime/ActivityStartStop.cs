
namespace Mynatime.CLI;

using Fastenshtein;
using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

public sealed class ActivityStartStop
{
    private readonly List<ActivityStartStopEvent> items;

    public ActivityStartStop(MynatimeProfileTransactionItem transactionItem)
    {
        this.items = new List<ActivityStartStopEvent>();
        if (transactionItem.Element.TryGetValue("Events", out JToken? events) && events is JArray array)
        {
            foreach (var jToken in array)
            {
                var item = (JObject)jToken;
                this.items.Add(ActivityStartStopEvent.Deserialize(item));
            }
        }
    }

    public ActivityStartStop()
    {
        this.items = new List<ActivityStartStopEvent>();
    }

    public MynatimeProfileTransactionItem ToTransactionItem(MynatimeProfileTransactionItem? root, DateTime utcNow)
    {
        if (root == null)
        {
            root = new MynatimeProfileTransactionItem();
            root.ObjectType = "MynatimeCLI.ActivityStartStop";
            root.TimeCreatedUtc = utcNow;
        }

        var arrayOfEvents = new JArray();
        root.Element["Events"] = arrayOfEvents;
        foreach (var eventItem in this.items)
        {
            arrayOfEvents.Add(eventItem.Serialize());
        }

        return root;
    }

    public void Add(DateTime timeLocal, string mode, string? categoryId)
    {
        this.items.Add(new ActivityStartStopEvent(timeLocal, mode, categoryId));
    }

    public string GetSummary()
    {
        var sb = new StringBuilder();
        foreach (var item in this.items)
        {
            sb.Append("  - ");
            sb.Append(item.TimeLocal.ToInvariantString());
            sb.Append("  ");
            sb.Append(item.Mode.PadRight(6, ' '));
            sb.Append("  ");
            sb.Append(item.CategoryId);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
}
