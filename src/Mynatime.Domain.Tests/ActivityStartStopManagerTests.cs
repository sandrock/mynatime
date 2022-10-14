
namespace Mynatime.Domain.Tests;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;
using System;
using Xunit;

public class ActivityStartStopManagerTests
{
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

        var items = target.GenerateItems();
        Assert.Empty(target.Errors);
        Assert.Collection(
            items,
            x => { VerifyEqual(x, date, 08, 00, date, 09, 30, default(string?)); },
            x => { VerifyEqual(x, date, 09, 30, date, 12, 00, "project2"); },
            x => { VerifyEqual(x, date, 13, 30, date, 15, 00, "project1"); },
            x => { VerifyEqual(x, date, 15, 00, date, 16, 30, "project3"); });

    }

    private void VerifyEqual(NewActivityItemPage item, DateTime date, int hour, int minute, DateTime endDate, int endHour, int endMinute, string? category)
    {
        Assert.Null(item.Duration);
        Assert.Null(item.Comment);
        Assert.Equal(item.DateStart, date.Date);
        Assert.Equal(item.DateEnd, endDate.Date);
        Assert.Equal(item.InAt, new TimeSpan(hour, minute, 0));
        Assert.Equal(item.OutAt, new TimeSpan(endHour, endMinute, 0));
    }
}
