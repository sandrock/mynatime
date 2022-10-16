
namespace Mynatime.CLI.Tests;

using Moq;
using Mynatime.Infrastructure;
using Mynatime.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Spectre.Console;
using Xunit;

public class ProfileAddCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);
    private IAnsiConsole? console;

    private IAnsiConsole Console
    {
        get => this.console ?? (this.console = this.mocks.Create<IAnsiConsole>(MockBehavior.Loose).Object);
    }

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "pro", "add", };
        yield return new object[] { "Profile", "Add", };
        yield return new object[] { "profiles", "add", };
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
        var target = new ProfileAddCommand(app.Object, client.Object, Console);
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
        var target = new ProfileAddCommand(app.Object, client.Object, Console);
        var result = target.ParseArgs(app.Object, args, out int consumedArgs, out Command? command);
        Assert.False(result);
    }

    [Fact]
    public void ParseArgs()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ProfileAddCommand(app.Object, client.Object, Console);
        var result = target.ParseArgs(app.Object, new []{ "profile", "add", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Null(target.LoginUsername);
    }

    [Fact]
    public void ParseArgs_Email()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ProfileAddCommand(app.Object, client.Object, Console);
        var result = target.ParseArgs(app.Object, new []{ "profile", "add", "--email", "test@test.com", }, out int consumedArgs, out Command? command);
        Assert.True(result);
        Assert.Equal("test@test.com", target.LoginUsername);
    }

    [Fact(Skip = "cannot mock the Run method (yet)")]
    public async Task Run_NoEmail()
    {
        var app = GetAppMock();
        var client = GetClientMock();
        var target = new ProfileAddCommand(app.Object, client.Object, Console);
        await target.Run();
    }

    private Mock<IManatimeWebClient> GetClientMock()
    {
        var mock = this.mocks.Create<IManatimeWebClient>();
        return mock;
    }

    private Mock<IConsoleApp> GetAppMock(bool withProfile = false)
    {
        var mock = this.mocks.Create<IConsoleApp>();

        var profile = new MynatimeProfile();
        if (withProfile)
        {
            mock.SetupGet(x => x.CurrentProfile).Returns(profile);
        }

        return mock;
    }
}
