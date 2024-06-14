
namespace Mynatime.CLI.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Mynatime.CLI.Tests.Resources;
using Mynatime.Client;
using Mynatime.Infrastructure;
using Xunit;

public class ActivitySearchCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "act", "search", };
        yield return new object[] { "Activity", "Search", };
        yield return new object[] { "activities", "search", };
    }

    public static IEnumerable<object[]> InvalidInitialArgument()
    {
        yield return new object[] { "act", "searsh", };
        yield return new object[] { "search", "activity", };
        yield return new object[] { "activities", };
        yield return new object[] { "search", };
    }
 
    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string arg0, string arg1)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivitySearchCommand(app.Object, client.Object);
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
        var target = new ActivitySearchCommand(app.Object, client.Object);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
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
            ActivityTesting.PopulateCategories0(profile.Data.ActivityCategories);
        }

        if (localTime != null && localTz != null)
        {
        }
        else
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                localTz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            }
            else
            {
                localTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Helsinki");
            }

            utcTime = new DateTime(2022, 9, 21, 11, 36, 42, DateTimeKind.Utc);
            ////localTime = new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local);
            ////utcTime = TimeZoneInfo.ConvertTimeToUtc(localTime.Value);
            localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.Value, localTz);
        }

        mock.SetupGet(x => x.TimeNowLocal).Returns(localTime.Value);
        mock.SetupGet(x => x.TimeNowUtc).Returns(utcTime.Value);
        mock.SetupGet(x => x.TimeZoneLocal).Returns(localTz);

        mock.Setup(x => x.PersistProfile(It.IsAny<MynatimeProfile>())).Returns(Task.CompletedTask).Verifiable();

        return mock;
    }
}
