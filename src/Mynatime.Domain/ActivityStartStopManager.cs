namespace Mynatime.Domain;

using Mynatime.Client;
using Mynatime.Infrastructure;
using Mynatime.Infrastructure.ProfileTransaction;
using System.Globalization;

/// <summary>
/// Given a list of start/stop event, computes a list of <see cref="NewActivityItemPage"/>.
/// </summary>
public sealed class ActivityStartStopManager
{
    private readonly ActivityStartStop source;
    private readonly List<BaseError> errors = new ();
    private readonly List<BaseError> warnings = new ();
    private readonly List<ActivityStartStopEvent> usedEvents = new ();
    private readonly List<NewActivityItemPage> activities = new();

    public ActivityStartStopManager(ActivityStartStop source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public IReadOnlyList<BaseError> Errors { get => this.errors; }

    public IReadOnlyList<BaseError> Warnings { get => this.warnings; }

    public IReadOnlyList<ActivityStartStopEvent> UsedEvents { get => this.usedEvents; }

    public IReadOnlyList<NewActivityItemPage> Activities { get => this.activities; }

    public void GenerateItems()
    {
        this.errors.Clear();
        this.warnings.Clear();
        this.usedEvents.Clear();
        this.activities.Clear();

        NewActivityItemPage? currentActivity = null, previousActivity = null;
        // ReSharper disable once NotAccessedVariable
        ActivityStartStopEvent? startEvent = null, stopEvent = null;
        foreach (var currentEvent in source.Events.OrderBy(x => x.TimeLocal))
        {
            bool itemStarts = currentEvent.Mode == "Start";
            bool itemStops = currentEvent.Mode == "Stop";

            if (itemStarts)
            {
                // starting an activity
                if (currentActivity != null)
                {
                    // and stopping another
                    usedEvents.AddIfAbsent(startEvent!);
                    usedEvents.AddIfAbsent(stopEvent = currentEvent);
                    MakeStop(currentActivity, currentEvent.TimeLocal);
                    previousActivity = currentActivity;
                    currentActivity = null;
                }

                startEvent = currentEvent;
                previousActivity = currentActivity;
                currentActivity = new NewActivityItemPage();
                currentActivity.ActivityId = currentEvent.ActivityId;
                MakeStart(currentActivity, currentEvent.TimeLocal);
                AppendComment(currentActivity, currentEvent);
            }
            else if (itemStops)
            {
                // stopping an activity
                if (currentActivity != null)
                {
                    usedEvents.AddIfAbsent(startEvent!);
                    usedEvents.AddIfAbsent(stopEvent = currentEvent);
                    currentActivity.ActivityId = currentEvent.ActivityId ?? currentActivity.ActivityId;
                    MakeStop(currentActivity, currentEvent.TimeLocal);
                    AppendComment(currentActivity, currentEvent);
                    previousActivity = currentActivity;
                    currentActivity = null;
                }
                else if (previousActivity != null)
                {
                    // stopping after a stop
                    usedEvents.AddIfAbsent(stopEvent = currentEvent);
                    currentActivity = new NewActivityItemPage();
                    currentActivity.ActivityId = currentEvent.ActivityId;
                    MakeStart(currentActivity, previousActivity.GetEndTime()!.Value);
                    MakeStop(currentActivity, currentEvent.TimeLocal);
                    AppendComment(currentActivity, currentEvent);
                    previousActivity = currentActivity;
                    currentActivity = null;
                }
                else
                {
                    this.errors.Add(new BaseError("StopNotFollowingStart", "Stop event " + currentEvent + " is not following a start event. "));
                }
            }
            else
            {
                this.errors.Add(new BaseError("UnknownEventType", "Event " + currentEvent + " is of unknown type. "));
            }
        }

        if (currentActivity?.OutAt != null)
        {
            this.activities.Add(currentActivity);
        }

        foreach (var activity in this.activities)
        {
            int days = CountDaysAcross(activity.DateStart!.Value, activity.DateEnd!.Value);
            if (days == 0)
            {
                // impossible
            }
            else if (days == 1)
            {
                // ok
            }
            else if (days == 2)
            {
                this.warnings.Add(new BaseError("NightlyItem", "Nightly activity between " + activity.DateStart.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture) + " and " + activity.DateEnd.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture) + ". "));
            }
            else
            {
                this.warnings.Add(new BaseError("ManyDaysItem", "Multiple days activity between " + activity.DateStart.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture) + " and " + activity.DateEnd.Value.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture) + ". "));
            }
        }
    }

    private int CountDaysAcross(DateTime start, DateTime end)
    {
        int value = 0;
        var current = start;
        for (; current <= end ;)
        {
            current = start.AddDays(value + 1);
            value++;
        }

        return value;
    }

    private void MakeStart(NewActivityItemPage current, DateTime item)
    {
        current.DateStart = item.Date;
        current.InAt = item.TimeOfDay;
    }

    private void MakeStop(NewActivityItemPage current, DateTime item)
    {
        current.DateEnd = item.Date;
        current.OutAt = item.TimeOfDay;
        this.activities.Add(current);
    }

    private void AppendComment(NewActivityItemPage currentActivity, ActivityStartStopEvent currentEvent)
    {
        if (string.IsNullOrEmpty(currentEvent.Comment))
        {
            return;
        }

        if (!string.IsNullOrEmpty(currentActivity.Comment))
        {
            currentActivity.Comment += "\n" + currentEvent.Comment;
        }
        else
        {
            currentActivity.Comment = currentEvent.Comment;
        }
    }
}
