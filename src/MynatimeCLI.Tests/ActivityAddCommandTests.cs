
namespace MynatimeCLI.Tests;

using Moq;
using Mynatime;
using Mynatime.Infrastructure;
using MynatimeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ActivityAddCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "act", "add", };
        yield return new object[] { "Activity", "add", };
        yield return new object[] { "activities", "add", };
    }

    public static IEnumerable<object[]> InvalidInitialArgument()
    {
        yield return new object[] { "act", "cet", };
        yield return new object[] { "category", "activity", };
        yield return new object[] { "activities", };
        yield return new object[] { "categories", };
    }
 
    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string arg0, string arg1)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.MatchArg(arg0);
        Assert.True(result);
        result = target.ParseArgs(app.Object, new string[] { arg0, arg1, }, out int consumedArgs, out Command? command);
        Assert.True(result);
    }

    [Theory, MemberData(nameof(InvalidInitialArgument))]
    public void MatchArg_No(params string[] args)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityCategoryCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public void Match_Date_Duration_Category()
    {
        var args = new string[] { "act", "add", "2022-09-18", "2.5", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal(new DateTime(2022, 9, 18, 0, 0, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Equal(2.5M, target.DurationHours);
        Assert.Null(target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Duration_Category()
    {
        var args = new string[] { "act", "add", "2.5", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Null(target.StartTimeLocal);
        Assert.Equal(2.5M, target.DurationHours);
        Assert.Null(target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }
    
    [Fact]
    public void Match_Date_TimeStart_TimeEnd_Category()
    {
        var args = new string[] { "act", "add", "2022-09-18", "925", "1140", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal(new DateTime(2022, 9, 18, 9, 25, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(new DateTime(2022, 9, 18, 11, 40, 0, DateTimeKind.Local), target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_StartDate_TimeStart_EndDate_TimeEnd_Category()
    {
        var args = new string[] { "act", "add", "2022-09-18", "925", "2022-09-19", "1140", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal(new DateTime(2022, 9, 18, 9, 25, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(new DateTime(2022, 9, 19, 11, 40, 0, DateTimeKind.Local), target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_TimeStart_TimeEnd_Category()
    {
        var args = new string[] { "act", "add", "925", "1140", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal(new DateTime(2022, 9, 21, 9, 25, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(new DateTime(2022, 9, 21, 11, 40, 0, DateTimeKind.Local), target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_TimeStart_Duration_Category()
    {
        var args = new string[] { "act", "add", "925", "2.5", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal(new DateTime(2022, 9, 21, 9, 25, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Equal(2.5M, target.DurationHours);
        Assert.Null(target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_TimeStart_Duration_Category_Extra()
    {
        var args = new string[] { "act", "add", "925", "2.5", "proj1", "qwerty", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
        Assert.Equal(new DateTime(2022, 9, 21, 9, 25, 0, DateTimeKind.Local), target.StartTimeLocal);
        Assert.Equal(2.5M, target.DurationHours);
        Assert.Null(target.EndTimeLocal);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public async Task Run_NoProfile()
    {
        var app = GetAppMock();
        var target = new ActivityAddCommand(app.Object, null);
        await target.Run();
    }

    private Mock<IManatimeWebClient> GetClientMock()
    {
        var mock = this.mocks.Create<IManatimeWebClient>();
        return mock;
    }

    private Mock<IConsoleApp> GetAppMock(bool withProfile = false, DateTime? localTime = null, TimeZoneInfo? localTz = null)
    {
        var mock = this.mocks.Create<IConsoleApp>();

        var profile = new MynatimeProfile();
        if (withProfile)
        {
            mock.SetupGet(x => x.CurrentProfile).Returns(profile);
        }

        if (localTime != null && localTz != null)
        {
        }
        else
        {
            localTime = new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local);
            localTz = TimeZoneInfo.GetSystemTimeZones().Last();
        }

        mock.SetupGet(x => x.TimeNowLocal).Returns(localTime.Value);
        mock.SetupGet(x => x.TimeZoneLocal).Returns(localTz);

        return mock;
    }
}
