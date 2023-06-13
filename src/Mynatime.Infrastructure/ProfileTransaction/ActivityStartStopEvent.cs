
namespace Mynatime.Infrastructure.ProfileTransaction;

using Mynatime.Infrastructure;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;
using static Mynatime.Infrastructure.MynatimeConstants;

public sealed class ActivityStartStopEvent
{
    public ActivityStartStopEvent(DateTime timeLocal, string mode, string? activityId)
        : this(timeLocal, mode, activityId, null)
    {
    }

    public ActivityStartStopEvent(DateTime timeLocal, string mode, string? activityId, string? comment)
    {
        this.TimeLocal = timeLocal;
        this.Mode = mode;
        this.ActivityId = activityId;
        this.Comment = comment;
    }

    public DateTime TimeLocal { get; }

    public string Mode { get; }

    public string? ActivityId { get; }

    public string? Comment { get; set; }

    public static ActivityStartStopEvent Deserialize(JObject item)
    {
        return new ActivityStartStopEvent(
            item.Value<DateTime>("Time"),
            item.Value<string>("Mode")!,
            item.Value<string?>("CategoryId"),
            item.Value<string?>("Comment"));
    }

    public JObject Serialize()
    {
        var item = new JObject();
        item.Add("Time", this.TimeLocal.ToInvariantString());
        item.Add("Mode", this.Mode);
        item.Add("CategoryId", this.ActivityId);
        item.Add("Comment", this.Comment);
        return item;
    }

    public override string ToString()
    {
        return nameof(ActivityStartStopEvent) + " " + this.TimeLocal.ToInvariantString() + " " + this.Mode + " " + this.ActivityId;
    }

    public string ToDisplayString(MynatimeProfileData data)
    {
        var sb = new StringBuilder();

        sb.Append(this.TimeLocal.ToString(DateFormat, CultureInfo.InvariantCulture));
        sb.Append(' ');
        sb.Append(this.TimeLocal.ToString(TimeFormat, CultureInfo.InvariantCulture));

        if (this.ActivityId != null)
        {
            var activity = data.GetActivityById(this.ActivityId);
            sb.Append(' ');
            sb.Append(activity.Name);
        }

        return sb.ToString();
    }
}
