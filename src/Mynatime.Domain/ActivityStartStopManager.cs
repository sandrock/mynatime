namespace Mynatime.Domain;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;

public sealed class ActivityStartStopManager
{
    private readonly ActivityStartStop source;
    private readonly List<string> errors = new ();

    public ActivityStartStopManager(ActivityStartStop source)
    {
        this.source = source;
    }

    public IReadOnlyList<string> Errors { get => this.errors; }

    public IEnumerable<NewActivityItemPage> GenerateItems()
    {
        this.errors.Clear();

        NewActivityItemPage current = null;
        foreach (var item in source.Events)
        {
            bool itemStarts = item.Mode == "Start";
            bool itemStops = item.Mode == "Stop";

            if (itemStarts)
            {
                if (current != null)
                {
                    MakeStop(current, item.TimeLocal);
                    yield return current;
                    current = null;
                }

                current = new NewActivityItemPage();
                MakeStart(current, item.TimeLocal);
            }
            else if (itemStops)
            {
                if (current != null)
                {
                    MakeStop(current, item.TimeLocal);
                    yield return current;
                    current = null;
                }
                else
                {
                    this.errors.Add("Stop event " + item + " is not following a start event. ");
                }
            }
            else
            {
                this.errors.Add("Event " + item + " is of unknown type. ");
            }
        }

        if (current?.OutAt != null)
        {
            yield return current;
        }
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
    }
}
