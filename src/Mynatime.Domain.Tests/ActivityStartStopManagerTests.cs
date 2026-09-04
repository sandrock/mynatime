
namespace Mynatime.Domain.Tests;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ActivityStartStopManagerTests
{
    [Fact]
    public void ComputeActivityList_StartAndStop1()
    {
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(8.0), "Start", "project1");
        source.Add(date.AddHours(10.0), "Stop");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date, 8, 0, date, 10, 0, "project1"); });
    }

    [Fact]
    public void ComputeActivityList_StartAndStop2()
    {
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(8.0), "Start");
        source.Add(date.AddHours(10.0), "Stop", "project1");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date, 8, 0, date, 10, 0, "project1"); });
    }

    [Fact]
    public void ComputeActivityList_OneDayOkay()
    {
        var date = new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(0.0), "Start", null);
        source.Add(date.AddHours(1.5), "Start", "project2");
        source.Add(date.AddHours(4.0), "Stop", null);
        source.Add(date.AddHours(5.5), "Start", "project1");
        source.Add(date.AddHours(7.0), "Start", null);
        source.Add(date.AddHours(8.5), "Stop", "project3");

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, date, 09, 30, default(string?)); },
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project2"); },
            x => { VerifyEqual(x, date, 13, 30, date, 15, 00, "project1"); },
            x => { VerifyEqual(x, date, 15, 00, date, 16, 30, "project3"); });
        Assert.Collection(
            target.UsedEvents,
            x => { Assert.Same(source.EventsList[0], x); },
            x => { Assert.Same(source.EventsList[1], x); },
            x => { Assert.Same(source.EventsList[2], x); },
            x => { Assert.Same(source.EventsList[3], x); },
            x => { Assert.Same(source.EventsList[4], x); },
            x => { Assert.Same(source.EventsList[5], x); });
    }

    [Fact]
    public void ComputeActivityList_OneDayOkay_Comment()
    {
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(08.0), "Start", null, "c1");
        source.Add(date.AddHours(09.5), "Start", "project2", "c2");
        source.Add(date.AddHours(12.0), "Stop", null, "c3");
        source.Add(date.AddHours(13.5), "Start", "project1", "c4");
        source.Add(date.AddHours(15.0), "Start", null, "c5");
        source.Add(date.AddHours(16.5), "Stop", "project3", "c6");

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, date, 09, 30, null, "c1"); },
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project2", "c2\nc3"); },
            x => { VerifyEqual(x, date, 13, 30, date, 15, 00, "project1", "c4"); },
            x => { VerifyEqual(x, date, 15, 00, date, 16, 30, "project3", "c5\nc6"); });
        Assert.Collection(
            target.UsedEvents,
            x => { Assert.Same(source.EventsList[0], x); },
            x => { Assert.Same(source.EventsList[1], x); },
            x => { Assert.Same(source.EventsList[2], x); },
            x => { Assert.Same(source.EventsList[3], x); },
            x => { Assert.Same(source.EventsList[4], x); },
            x => { Assert.Same(source.EventsList[5], x); });
    }

    [Fact]
    public void ComputeActivityList_DuplicateStartStopComment()
    {
        // A start and an explicit stop carrying the same comment must not duplicate it.
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(12.5), "Start", "project1", "claude");
        source.Add(date.AddHours(13.25), "Stop", "project1", "claude");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date, 12, 30, date, 13, 15, "project1", "claude"); });
    }

    [Fact]
    public void ComputeActivityList_OneDayOkay_Unordered()
    {
        var date = new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        var events = new ActivityStartStopEvent[6];
        events[1] = source.Add(date.AddHours(1.5), "Start", "project2");
        events[2] = source.Add(date.AddHours(4.0), "Stop", null);
        events[4] = source.Add(date.AddHours(7.0), "Start", null);
        events[3] = source.Add(date.AddHours(5.5), "Start", "project1");
        events[5] = source.Add(date.AddHours(8.5), "Stop", "project3");
        events[0] = source.Add(date.AddHours(0.0), "Start", null);

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, date, 09, 30, default(string?)); },
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project2"); },
            x => { VerifyEqual(x, date, 13, 30, date, 15, 00, "project1"); },
            x => { VerifyEqual(x, date, 15, 00, date, 16, 30, "project3"); });
        ////source.Events
        Assert.Collection(
            target.UsedEvents,
            x => { Assert.Same(events[0], x); },
            x => { Assert.Same(events[1], x); },
            x => { Assert.Same(events[2], x); },
            x => { Assert.Same(events[3], x); },
            x => { Assert.Same(events[4], x); },
            x => { Assert.Same(events[5], x); });
    }

    [Fact]
    public void ComputeActivityList_StoppingWithoutStarting1()
    {
        var date = new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(0.0), "Stop", null); // stopping without starting

        target.GenerateItems();
        var items = target.Activities;
        Assert.Collection(
            target.Errors,
            x => { Assert.Equal("StopNotFollowingStart", x.Code); });
        Assert.Empty(items);
    }

    [Fact]
    public void ComputeActivityList_StoppingWithoutStarting2()
    {
        var date = new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(0.0), "Stop", null); // stopping without starting
        source.Add(date.AddHours(1.5), "Start", "project2");
        source.Add(date.AddHours(4.0), "Stop", null);

        target.GenerateItems();
        var items = target.Activities;
        Assert.Collection(
            target.Errors,
            x => { Assert.Equal("StopNotFollowingStart", x.Code); });
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project2"); });
    }

    [Fact]
    public void ComputeActivityList_StoppingWithoutStarting3()
    {
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(08.0), "Start");
        source.Add(date.AddHours(09.5), "Stop", "project2");
        source.Add(date.AddHours(12.0), "Stop", "project1"); // here we miss a start entry

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, date, 09, 30, "project2"); },
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project1"); });
    }

    [Fact]
    public void ComputeActivityList_StartingWithoutStopping()
    {
        var date = new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(0.0), "Start", null); // stopping without starting

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, null, 0, 00, null, isStartAndStop: false); });
    }

    [Fact]
    public void ComputeActivityList_ManyDaysOkay()
    {
        var date0 = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);

        // day 1
        source.Add(date0.AddHours(08.0), "Start", "project1");
        source.Add(date0.AddHours(09.5), "Start", "daily");
        source.Add(date0.AddHours(10.0), "Start", "project1");
        source.Add(date0.AddHours(13.5), "Start", "internal");
        source.Add(date0.AddHours(16.5), "Start", "project2");
        source.Add(date0.AddHours(17.5), "Stop");
        source.Add(date0.AddHours(12.0), "Stop", "project2"); // late stop input
        
        // day 2
        var date1 = date0.AddDays(1);
        source.Add(date1.AddHours(09.0), "Start");
        source.Add(date1.AddHours(09.5), "Start", "daily");
        source.Add(date1.AddHours(09.4), "Stop", "project1");
        source.Add(date1.AddHours(12.0), "Stop", "project2"); // stop without start that works
        source.Add(date1.AddHours(10.0), "Start", "project3"); // stop without start that works

        target.GenerateItems();
        var items = target.Activities;
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date0, 08, 00, date0, 09, 30, "project1"); },
            x => { VerifyEqual(x, date0, 09, 30, date0, 10, 00, "daily"); },
            x => { VerifyEqual(x, date0, 10, 00, date0, 12, 00, "project2"); },
            x => { VerifyEqual(x, date0, 13, 30, date0, 16, 30, "internal"); },
            x => { VerifyEqual(x, date0, 16, 30, date0, 17, 30, "project2"); },
            x => { VerifyEqual(x, date1, 09, 00, date1, 09, 24, "project1"); },
            x => { VerifyEqual(x, date1, 09, 30, date1, 10, 00, "daily"); },
            x => { VerifyEqual(x, date1, 10, 00, date1, 12, 00, "project2"); });
        Assert.Equal(source.EventsList.Count, target.UsedEvents.Count);
    }

    [Fact]
    public void ComputeActivityList_NightlyItem_ExplicitStop()
    {
        // A genuine night shift: an explicit Stop on the next day still forms a
        // multi-day span and keeps the nightly warning.
        var date0 = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var date1 = date0.AddDays(1);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date0.AddHours(22.0), "Start", "project1");
        source.Add(date1.AddHours(06.0), "Stop");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date0, 22, 00, date1, 06, 00, "project1"); });
        Assert.Equal(source.EventsList.Count, target.UsedEvents.Count);
        Assert.Collection(
            target.Warnings,
            x => { Assert.Equal("NightlyItem", x.Code); });
    }

    [Fact]
    public void ComputeActivityList_ManyDaysItem_ExplicitStop()
    {
        // A multi-day span from an explicit Stop two days later keeps the warning.
        var date0 = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var date2 = date0.AddDays(2);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date0.AddHours(22.0), "Start", "project1");
        source.Add(date2.AddHours(06.0), "Stop");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date0, 22, 00, date2, 06, 00, "project1"); });
        Assert.Equal(source.EventsList.Count, target.UsedEvents.Count);
        Assert.Collection(
            target.Warnings,
            x => { Assert.Equal("ManyDaysItem", x.Code); });
    }

    [Fact]
    public void ComputeActivityList_MissingStopAcrossMidnight_LeavesOpen()
    {
        // Forgot to stop yesterday: the next day's first start no longer fabricates an
        // overnight end — the previous activity is left open, to be closed explicitly.
        var date0 = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var date1 = date0.AddDays(1);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date0.AddHours(16.5), "Start", "project2");
        source.Add(date1.AddHours(09.0), "Start", "project1");
        source.Add(date1.AddHours(10.0), "Stop", "project1");

        target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Empty(target.Warnings);
        Assert.Collection(
            target.Activities,
            x => { VerifyEqual(x, date0, 16, 30, null, 0, 00, "project2", isStartAndStop: false); },
            x => { VerifyEqual(x, date1, 09, 00, date1, 10, 00, "project1"); });
    }

    [Fact]
    public void RemoveEventsForResolved_KeepsOpenTailSharingBoundary()
    {
        // Corrupted-state repro: a saved span (project2 09:30->12:00) whose end is the shared
        // start of an open, never-stopped entry (project1 12:00->). Resolving the saved span
        // must not drag the shared 12:00 event away, or the open entry vanishes.
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(08.0), "Start", "project0");
        source.Add(date.AddHours(09.5), "Start", "project2");
        var boundary = source.Add(date.AddHours(12.0), "Start", "project1"); // open tail starts here

        target.GenerateItems();
        Assert.Empty(target.Errors);

        // project0 and project2 are resolved (saved/converted); the open project1 is not.
        var resolved = new List<NewActivityItemPage>
        {
            target.Activities[0].Item,
            target.Activities[1].Item,
        };
        target.RemoveEventsForResolved(resolved);

        // Only the shared boundary event survives, so the open entry re-derives intact.
        Assert.Collection(
            source.EventsList,
            x => { Assert.Same(boundary, x); });

        var after = new ActivityStartStopManager(source);
        after.GenerateItems();
        Assert.Collection(
            after.Activities,
            x => { VerifyEqual(x, date, 12, 00, null, 0, 00, "project1", isStartAndStop: false); });
    }

    [Fact]
    public void RemoveEventsForResolved_EmptiesTrackerWhenAllResolved()
    {
        var date = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Local);
        var source = new ActivityStartStop();
        var target = new ActivityStartStopManager(source);
        source.Add(date.AddHours(09.0), "Start", "project1");
        source.Add(date.AddHours(10.0), "Stop", "project1");
        source.Add(date.AddHours(11.0), "Start", "project2");
        source.Add(date.AddHours(12.0), "Stop", "project2");

        target.GenerateItems();
        Assert.Empty(target.Errors);

        var resolved = target.Activities.Select(a => a.Item).ToList();
        target.RemoveEventsForResolved(resolved);

        Assert.Empty(source.EventsList);
    }

    private void VerifyEqual(ActivityItemWrapper wrapper, DateTime date, int hour, int minute, DateTime? endDate, int endHour, int endMinute, string? category, string? comment = null, bool? isStartAndStop = true)
    {
        var item = wrapper.Item;
        Assert.Null(item.Duration);
        Assert.Equal(item.DateStart, date.Date);

        if (endDate != null)
        {
            Assert.Equal(endDate.Value.Date, item.DateEnd);
            Assert.Equal(item.OutAt, new TimeSpan(endHour, endMinute, 0));
        }
        else
        {
            Assert.Null(endDate);
        }
        
        Assert.Equal(item.InAt, new TimeSpan(hour, minute, 0));

        if (category != null)
        {
            Assert.Equal(category, item.ActivityId);
        }
        else
        {
            Assert.Null(item.ActivityId);
        }

        if (comment != null)
        {
            Assert.Equal(comment, item.Comment);
        }
        else
        {
            Assert.Null(item.Comment);
        }

        if (isStartAndStop != null)
        {
            Assert.Equal(isStartAndStop.Value, wrapper.IsStartAndStop);
        }
    }
}
