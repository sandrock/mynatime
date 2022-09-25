
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

public class StatusCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "status", };
    }

    public static IEnumerable<object[]> InvalidInitialArgument()
    {
        yield return new object[] { "act", "cet", };
        yield return new object[] { "category", "activity", };
        yield return new object[] { "activities", };
        yield return new object[] { "categories", };
        yield return new object[] { "act", "add", };
        yield return new object[] { "Activity", "add", };
        yield return new object[] { "activities", "add", };
    }
 
    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string arg0)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new StatusCommand(app.Object, client.Object);
        var result = target.MatchArg(arg0);
        Assert.True(result);
        result = target.ParseArgs(app.Object, new string[] { arg0, }, out int consumedArgs, out Command? command);
        Assert.True(result);
    }

    [Theory, MemberData(nameof(InvalidInitialArgument))]
    public void MatchArg_No(params string[] args)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new StatusCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public async Task Status_Nothing()
    {
        var app = GetAppMock(withProfile: true);
        var client = GetClientMock();
        var target = new StatusCommand(app.Object, client.Object);
        await target.Run();
    }

    private Mock<IManatimeWebClient> GetClientMock()
    {
        var mock = this.mocks.Create<IManatimeWebClient>();
        return mock;
    }

    private Mock<IConsoleApp> GetAppMock(bool withProfile = false, DateTime? localTime = null, TimeZoneInfo? localTz = null, DateTime? utcTime = null)
    {
        var mock = this.mocks.Create<IConsoleApp>();

        var profile = new MynatimeProfile();
        if (withProfile)
        {
            mock.SetupGet(x => x.CurrentProfile).Returns(profile);
            
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

        return mock;
    }
}
