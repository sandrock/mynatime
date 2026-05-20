
namespace Mynatime.CLI.Tests;

using Moq;
using Mynatime.Infrastructure;
using Mynatime.CLI.Tests.Resources;
using Mynatime.Client;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class ActivityTrackingCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);
    private IAnsiConsole? _console;
    private IAnsiConsole Console => _console ??= this.mocks.Create<IAnsiConsole>(MockBehavior.Loose).Object;

    public static IEnumerable<object?[]> ValidInitialArgument()
    {
        yield return new object?[] { "activities", null, };
        yield return new object?[] { "act", null, };
        yield return new object[] { "act", "status", };
        yield return new object[] { "act", "start", };
        yield return new object[] { "Activity", "start", };
        yield return new object[] { "activities", "start", };
        yield return new object[] { "act", "stop", };
        yield return new object[] { "Activity", "stop", };
        yield return new object[] { "activities", "stop", };
    }

    public static IEnumerable<object[]> InvalidInitialArgument()
    {
        yield return new object[] { "act", "cet", };
        yield return new object[] { "category", "activity", };
        yield return new object[] { "categories", };
        yield return new object[] { "act", "add", };
        yield return new object[] { "Activity", "add", };
        yield return new object[] { "activities", "add", };
    }

    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string? arg0, string? arg1)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.MatchArg(arg0!);
        Assert.True(result);
        result = target.ParseArgs(app.Object, arg0 == null ? new string[0] : arg1 == null ? new string[] { arg0, } : new string[] { arg0, arg1, }, out int consumedArgs,
            out Command? command);
        Assert.True(result);
    }

    [Theory, MemberData(nameof(InvalidInitialArgument))]
    public void MatchArg_No(params string[] args)
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public void Match_Start()
    {
        var args = new string[] { "act", "start", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Null(target.CategoryArg);
    }

    [Fact]
    public void Match_Start_Comment()
    {
        var args = new string[] { "act", "start", "-m", "working of this and that", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Null(target.CategoryArg);
        Assert.Equal(args[3], target.Comment);
    }

    [Fact]
    public void Match_Start_Category()
    {
        var args = new string[] { "act", "start", "proj1", };
        var app = GetAppMock(withProfile: true);
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Start_Category_Comment()
    {
        var args = new string[] { "act", "start", "proj1", "-m", "working of this and that", };
        var app = GetAppMock(withProfile: true);
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
        Assert.Equal(args[4], target.Comment);
    }

    [Fact]
    public void Match_Start_Comment_Category()
    {
        var args = new string[] { "act", "start", "-m", "working of this and that", "proj1", };
        var app = GetAppMock(withProfile: true);
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
        Assert.Equal(args[3], target.Comment);
    }

    [Fact]
    public void Match_Start_Category_Time()
    {
        var args = new string[] { "act", "start", "proj1", "0834", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Start_Time_Category()
    {
        var args = new string[] { "act", "start", "0834", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Start_Date_Time_Category()
    {
        var args = new string[] { "act", "start", "2022-08-01", "0834", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStart);
        Assert.False(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 8, 1, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Stop()
    {
        var args = new string[] { "act", "stop", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Null(target.CategoryArg);
    }

    [Fact]
    public void Match_Stop_Comment()
    {
        var args = new string[] { "act", "stop", "-m", "working of this and that", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Null(target.CategoryArg);
        Assert.Equal(args[3], target.Comment);
    }

    [Fact]
    public void Match_Stop_Category()
    {
        var args = new string[] { "act", "stop", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Stop_Category_Comment()
    {
        var args = new string[] { "act", "stop", "proj1", "-m", "working of this and that", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
        Assert.Equal(args[4], target.Comment);
    }

    [Fact]
    public void Match_Stop_Comment_Category()
    {
        var args = new string[] { "act", "stop", "-m", "working of this and that", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 13, 36, 42, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
        Assert.Equal(args[3], target.Comment);
    }

    [Fact]
    public void Match_Stop_Category_Time()
    {
        var args = new string[] { "act", "stop", "proj1", "0834", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Stop_Category_Date_Time()
    {
        var args = new string[] { "act", "stop", "proj1", "2022-08-01", "0834", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 8, 1, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Stop_Time_Category()
    {
        var args = new string[] { "act", "stop", "0834", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 9, 21, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Stop_Date_Time_Category()
    {
        var args = new string[] { "act", "stop", "2022-08-01", "0834", "proj1", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.False(target.IsStart);
        Assert.True(target.IsStop);
        Assert.False(target.IsStatus);
        Assert.Equal(new DateTime(2022, 8, 1, 8, 34, 0, DateTimeKind.Local), target.TimeLocal);
        Assert.Null(target.DurationHours);
        Assert.Equal(tz.Id, target.TimeZoneLocal.Id);
        Assert.Equal("proj1", target.CategoryArg);
    }

    [Fact]
    public void Match_Status()
    {
        var args = new string[] { "act", "status", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStatus);
        Assert.False(target.IsStart);
        Assert.False(target.IsStop);
    }

    [Fact]
    public void Match_Nothing()
    {
        var args = new string[] { "act", };
        var app = GetAppMock();
        var tz = app.Object.TimeZoneLocal;
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsStatus);
        Assert.False(target.IsStart);
        Assert.False(target.IsStop);
    }

    [Fact]
    public void Match_Events()
    {
        var args = new string[] { "act", "events", };
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(app.Object, client.Object, this.Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.True(target.IsEvents);
        Assert.False(target.IsStatus);
        Assert.False(target.IsStart);
        Assert.False(target.IsStop);
    }

    [Fact]
    public async Task Run_Events_Empty_ShowsNone()
    {
        var profile = MakeProfileWithTransaction();
        var output = await CaptureRun("events", profile);
        Assert.Contains("none", output);
    }

    [Fact]
    public async Task Run_Events_WithStartEvent_ShowsRow()
    {
        var profile = MakeProfileWithTransaction();
        var state = new Mynatime.Infrastructure.ProfileTransaction.ActivityStartStop();
        state.Add(new DateTime(2022, 9, 21, 9, 0, 0, DateTimeKind.Local), "Start", null);
        profile.Transaction!.Add(state.ToTransactionItem(null, DateTime.UtcNow));

        var output = await CaptureRun("events", profile);

        Assert.Contains("2022-09-21", output);
        Assert.Contains("09:00", output);
        Assert.Contains("Start", output);
    }

    [Fact]
    public async Task Run_Status_OpenEvent_ShowsMessage()
    {
        var profile = MakeProfileWithTransaction();
        var state = new Mynatime.Infrastructure.ProfileTransaction.ActivityStartStop();
        state.Add(new DateTime(2022, 9, 21, 9, 0, 0, DateTimeKind.Local), "Start", null);
        profile.Transaction!.Add(state.ToTransactionItem(null, DateTime.UtcNow));

        var output = await CaptureRun("status", profile);

        Assert.Contains("Open event", output);
        Assert.Contains("2022-09-21 09:00", output);
    }

    [Fact]
    public async Task Run_Status_WithWarning_ShowsWarning()
    {
        var profile = MakeProfileWithTransaction();
        var state = new Mynatime.Infrastructure.ProfileTransaction.ActivityStartStop();
        state.Add(new DateTime(2022, 9, 21, 22, 0, 0, DateTimeKind.Local), "Start", null);
        state.Add(new DateTime(2022, 9, 22, 2, 0, 0, DateTimeKind.Local), "Stop", null);
        profile.Transaction!.Add(state.ToTransactionItem(null, DateTime.UtcNow));

        var output = await CaptureRun("status", profile);

        Assert.Contains("Warning", output);
    }

    [Fact]
    public async Task Run_Clear_RemovesItemsAddedWithActAdd()
    {
        var profile = MakeProfileWithTransaction();
        var page = new NewActivityItemPage();
        page.DateStart = new DateTime(2022, 9, 18, 9, 0, 0, DateTimeKind.Local);
        page.Duration = "2.5";
        profile.Transaction!.Add(page.ToTransactionItem(null, DateTime.UtcNow));

        await RunClear(profile);

        Assert.Empty(profile.Transaction!.Items);
    }

    [Fact]
    public async Task Run_Clear_RemovesStartStopEventsAndAddedItems()
    {
        var profile = MakeProfileWithTransaction();
        var state = new Mynatime.Infrastructure.ProfileTransaction.ActivityStartStop();
        state.Add(new DateTime(2022, 9, 21, 9, 0, 0, DateTimeKind.Local), "Start", null);
        profile.Transaction!.Add(state.ToTransactionItem(null, DateTime.UtcNow));
        var page = new NewActivityItemPage();
        page.DateStart = new DateTime(2022, 9, 18, 9, 0, 0, DateTimeKind.Local);
        page.Duration = "2.5";
        profile.Transaction!.Add(page.ToTransactionItem(null, DateTime.UtcNow));

        await RunClear(profile);

        Assert.DoesNotContain(
            profile.Transaction!.Items,
            item => MynatimeProfileTransactionManager.Default.OfClass<NewActivityItemPage>(item));
    }

    private async Task RunClear(MynatimeProfile profile)
    {
        using var writer = new System.IO.StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(writer),
        });
        var appMock = GetAppMock();
        appMock.SetupGet(x => x.CurrentProfile).Returns(profile);
        appMock.SetupGet(x => x.TimeNowUtc).Returns(new DateTime(2022, 9, 21, 11, 36, 42, DateTimeKind.Utc));
        appMock.Setup(x => x.PersistProfile(It.IsAny<MynatimeProfile>())).Returns(Task.CompletedTask);
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(appMock.Object, client.Object, console);
        target.ParseArgs(appMock.Object, new[] { "act", "clear" }, out _, out _);
        await target.Run();
    }

    private static MynatimeProfile MakeProfileWithTransaction()
    {
        var profile = new MynatimeProfile();
        _ = profile.Transaction; // ensure transaction is initialized
        return profile;
    }

    private async Task<string> CaptureRun(string subcommand, MynatimeProfile profile)
    {
        using var writer = new System.IO.StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new AnsiConsoleOutput(writer),
        });
        var appMock = GetAppMock();
        appMock.SetupGet(x => x.CurrentProfile).Returns(profile);
        var client = GetClientMock();
        var target = new ActivityTrackingCommand(appMock.Object, client.Object, console);
        target.ParseArgs(appMock.Object, new[] { "act", subcommand }, out _, out _);
        await target.Run();
        return writer.ToString();
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
