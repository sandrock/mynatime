
namespace MynatimeCLI.Tests;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mynatime;
using Xunit;
using Xunit.Abstractions;

public class ActivityCommandTests
{
    private MockRepository mocks = new MockRepository(MockBehavior.Strict);

    public static IEnumerable<object[]> ValidInitialArgument()
    {
        yield return new object[] { "act", };
        yield return new object[] { "Activity", };
        yield return new object[] { "activities", };
    }

    [Theory, MemberData(nameof(ValidInitialArgument))]
    public void MatchArg_Yes(string arg)
    {
        var app = GetAppMock();
        var target = new ActivityCommand(app.Object);
        var result = target.MatchArg(arg);
        Assert.True(result);
        result = target.ParseArgs(app.Object, new string[] { arg, }, out int consumedArgs, out Command? command);
        Assert.True(result);
    }

    private Mock<IConsoleApp> GetAppMock()
    {
        var mock = this.mocks.Create<IConsoleApp>();
        return mock;
    }
}
