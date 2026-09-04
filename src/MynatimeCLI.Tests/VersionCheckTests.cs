
namespace Mynatime.CLI.Tests;

using Mynatime.Infrastructure;
using System;
using Xunit;

public class VersionCheckTests
{
    [Theory]
    [InlineData("7.4.0", "7.4.0")]        // equal
    [InlineData("7.3.9", "7.4.0")]        // older
    [InlineData(null, "7.4.0")]           // unknown seen
    [InlineData("7.4.0", null)]           // unknown baseline
    [InlineData("nonsense", "7.4.0")]     // unparseable
    public void CompareManatime_NoDrift(string? seen, string? expected)
    {
        Assert.Equal(VersionCheck.ManatimeDrift.None, VersionCheck.CompareManatime(seen, expected));
    }

    [Theory]
    [InlineData("7.4.1", "7.4.0")]        // patch
    [InlineData("7.5.0", "7.4.0")]        // minor
    [InlineData("7.5.0#1999", "7.4.0")]   // build suffix stripped
    public void CompareManatime_MinorPatch(string seen, string expected)
    {
        Assert.Equal(VersionCheck.ManatimeDrift.MinorPatch, VersionCheck.CompareManatime(seen, expected));
    }

    [Theory]
    [InlineData("8.0.0", "7.4.0")]        // major
    [InlineData("10.0.0#3", "7.4.0")]     // major with build suffix
    public void CompareManatime_Major(string seen, string expected)
    {
        Assert.Equal(VersionCheck.ManatimeDrift.Major, VersionCheck.CompareManatime(seen, expected));
    }

    [Fact]
    public void IsDue_NeverChecked_IsDue()
    {
        var profile = new MynatimeProfile();
        Assert.True(VersionCheck.IsDue(profile, DateTime.UtcNow));
    }

    [Fact]
    public void IsDue_FutureSchedule_NotDue()
    {
        var now = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
        var profile = new MynatimeProfile { NextVersionCheckUtc = now.AddHours(1) };
        Assert.False(VersionCheck.IsDue(profile, now));
    }

    [Fact]
    public void IsDue_PastSchedule_IsDue()
    {
        var now = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
        var profile = new MynatimeProfile { NextVersionCheckUtc = now.AddMinutes(-1) };
        Assert.True(VersionCheck.IsDue(profile, now));
    }

    [Fact]
    public void NextCheckTime_Reached_InOneDay()
    {
        var now = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(now.AddDays(1), VersionCheck.NextCheckTime(now, reached: true));
    }

    [Fact]
    public void NextCheckTime_NotReached_InOneHour()
    {
        var now = new DateTime(2026, 9, 4, 12, 0, 0, DateTimeKind.Utc);
        Assert.Equal(now.AddHours(1), VersionCheck.NextCheckTime(now, reached: false));
    }
}
