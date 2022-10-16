
namespace Mynatime.Domain.Tests;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;
using System;
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
        Assert.Empty(items);
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

    private void VerifyEqual(NewActivityItemPage item, DateTime date, int hour, int minute, DateTime endDate, int endHour, int endMinute, string? category)
    {
        Assert.Null(item.Duration);
        Assert.Null(item.Comment);
        Assert.Equal(item.DateStart, date.Date);
        Assert.Equal(item.DateEnd, endDate.Date);
        Assert.Equal(item.InAt, new TimeSpan(hour, minute, 0));
        Assert.Equal(item.OutAt, new TimeSpan(endHour, endMinute, 0));

        if (category != null)
        {
            Assert.Equal(category, item.ActivityId);
        }
        else
        {
            Assert.Null(item.ActivityId);
        }
    }
}
