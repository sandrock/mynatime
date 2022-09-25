
namespace MynatimeCLI.Tests;

using Moq;
using Mynatime;
using Mynatime.Infrastructure;
using MynatimeClient;
using Newtonsoft.Json;
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
        var target = new ActivityAddCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public async Task MatchAndRun_Date_Duration_Category()
    {
        var args = new string[] { "act", "add", "2022-09-18", "2.5", "proj1", };
        var app = GetAppMock(withProfile: true);
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

        await target.Run();
        var item = Assert.Single(app.Object.CurrentProfile?.Transaction.Items);
        Assert.Equal("{\"ObjectType\":\"MynatimeClient.NewActivityItemPage\",\"TimeCreatedUtc\":\"2022-09-21T11:36:42Z\",\"FormData\":\"create%5Btask%5D=1001&create%5BdateStart%5D=2022-09-18&create%5BdateEnd%5D=&create%5BinAt%5D=&create%5BoutAt%5D=&create%5Bduration%5D=2.5&create%5Bcomment%5D=&submitAdvanced=&create%5B_token%5D=\"}", item.Element.ToString(Formatting.None));
        app.Verify(x => x.PersistProfile(It.IsAny<MynatimeProfile>()), Times.Once);
    }

    [Fact]
    public void Match_Category_Date_Duration()
    {
        var args = new string[] { "act", "add", "proj1", "2022-09-18", "2.5", };
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
    public async Task MatchAndRun_Date_TimeStart_TimeEnd_Category()
    {
        var args = new string[] { "act", "add", "2022-09-18", "925", "1140", "proj1", };
        var app = GetAppMock(withProfile: true);
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

        await target.Run();
        var item = Assert.Single(app.Object.CurrentProfile?.Transaction.Items);
        Assert.Equal("{\"ObjectType\":\"MynatimeClient.NewActivityItemPage\",\"TimeCreatedUtc\":\"2022-09-21T11:36:42Z\",\"FormData\":\"create%5Btask%5D=1001&create%5BdateStart%5D=2022-09-18&create%5BdateEnd%5D=2022-09-18&create%5BinAt%5D=09%3A25&create%5BoutAt%5D=09%3A25&create%5Bduration%5D=&create%5Bcomment%5D=&submitAdvanced=&create%5B_token%5D=\"}", item.Element.ToString(Formatting.None));
        app.Verify(x => x.PersistProfile(It.IsAny<MynatimeProfile>()), Times.Once);
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
        app.Verify(x => x.PersistProfile(It.IsAny<MynatimeProfile>()), Times.Never);
    }

    private Mock<IManatimeWebClient> GetClientMock()
    {
        var mock = this.mocks.Create<IManatimeWebClient>();
        return mock;
    }

    private Mock<IConsoleApp> GetAppMock(bool withProfile = false, DateTime? localTime = null, TimeZoneInfo? localTz = null, DateTime? utcTime = null)
    {
        var mock = this.mocks.Create<IConsoleApp>();

        MynatimeProfile profile = null;
        mock.SetupGet(x => x.CurrentProfile).Returns(() => profile);
        if (withProfile)
        {
            profile = new MynatimeProfile();
            profile.Data.ActivityCategories.Add(new MynatimeProfileDataActivityCategory("1001", "proj1"));
        }

        if (localTime != null && localTz != null)
        {
        }
        else
        {
            localTz = TimeZoneInfo.GetSystemTimeZones().Last();
            localTime = new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local);
            utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime.Value);
        }

        mock.SetupGet(x => x.TimeNowLocal).Returns(localTime.Value);
        mock.SetupGet(x => x.TimeNowUtc).Returns(utcTime.Value);
        mock.SetupGet(x => x.TimeZoneLocal).Returns(localTz);

        mock.Setup(x => x.PersistProfile(It.IsAny<MynatimeProfile>())).Returns(Task.CompletedTask).Verifiable();

        return mock;
    }
}
