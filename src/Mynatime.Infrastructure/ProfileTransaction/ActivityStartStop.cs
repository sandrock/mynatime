﻿
namespace Mynatime.Infrastructure.ProfileTransaction;

using Mynatime.Client;
using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

public sealed class ActivityStartStop : ITransactionItem
{
    private readonly List<ActivityStartStopEvent> items;

    static ActivityStartStop()
    {
        MynatimeProfileTransactionManager.Default.RegisterTransactionItemType<ActivityStartStop>(
            "MynatimeCLI.ActivityStartStop",
            x => new ActivityStartStop(x));
    }

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

    public IEnumerable<ActivityStartStopEvent> Events { get => this.items; }

    internal List<ActivityStartStopEvent> EventsList { get => this.items; }

    public static void Hello()
    {
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

    public ActivityStartStopEvent Add(DateTime timeLocal, string mode)
    {
        var item = this.Add(timeLocal, mode, null);
        return item;
    }

    public ActivityStartStopEvent Add(DateTime timeLocal, string mode, string? categoryId, string? comment = null)
    {
        var item = new ActivityStartStopEvent(timeLocal, mode, categoryId);
        item.Comment = comment;
        this.items.Add(item);
        return item;
    }

    public string GetSummary()
    {
        var sb = new StringBuilder();
        foreach (var item in this.items.OrderBy(x => x.TimeLocal))
        {
            sb.Append("  - ");
            sb.Append(item.TimeLocal.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture));
            sb.Append("  ");
            sb.Append(item.TimeLocal.TimeOfDay.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture));
            sb.Append("  ");
            sb.Append(item.Mode.PadRight(6, ' '));
            sb.Append("  ");
            sb.Append(item.ActivityId);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    public bool Remove(ActivityStartStopEvent usedEvent)
    {
        return this.EventsList.Remove(usedEvent);
    }

    public void Clear()
    {
        this.items.Clear();
    }
}
