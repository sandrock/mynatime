
namespace Mynatime.Infrastructure.ProfileTransaction;

using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;

public sealed class ActivityStartStopEvent
{
    public ActivityStartStopEvent(DateTime timeLocal, string mode, string? categoryId)
    {
        TimeLocal = timeLocal;
        Mode = mode;
        CategoryId = categoryId;
    }

    public DateTime TimeLocal { get; }

    public string Mode { get; }

    public string? CategoryId { get; }

    public static ActivityStartStopEvent Deserialize(JObject item)
    {
        return new ActivityStartStopEvent(
            item.Value<DateTime>("Time"),
            item.Value<string>("Mode")!,
            item.Value<string?>("CategoryId"));
    }

    public JObject Serialize()
    {
        var item = new JObject();
        item.Add("Time", this.TimeLocal.ToInvariantString());
        item.Add("Mode", this.Mode);
        item.Add("CategoryId", this.CategoryId);
        return item;
    }
}
