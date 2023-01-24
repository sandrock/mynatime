
namespace Mynatime.Infrastructure.Tests;

using System;
using System.Globalization;
using Mynatime.Client;
using Xunit;

public class ClientConstantsTests
{
    [Fact]
    public void DateInputFormat_WorksWithDateTime()
    {
        var time = new DateTime(2023, 11, 16, 14, 15, 16, 123, DateTimeKind.Utc);
        var result = time.ToString(ClientConstants.DateInputFormat, CultureInfo.InvariantCulture);
        Assert.Equal("2023-11-16", result);
    }

    [Fact]
    public void HourMinuteTimeFormat_WorksWithDateTime()
    {
        var time = new DateTime(2023, 11, 16, 14, 15, 16, 123, DateTimeKind.Utc);
        var result = time.TimeOfDay.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture);
        Assert.Equal("14:15", result);
    }

    [Fact]
    public void HourMinuteTimeFormat_WorksWithTimeSpan()
    {
        var time = new TimeSpan(1, 14, 15, 16, 123);
        var result = time.ToString(ClientConstants.HourMinuteTimeFormat, CultureInfo.InvariantCulture);
        Assert.Equal("14:15", result);
    }
}
