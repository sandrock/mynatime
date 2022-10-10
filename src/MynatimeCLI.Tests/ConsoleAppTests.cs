
namespace Mynatime.CLI.Tests;

using System;
using Xunit;

public class ConsoleAppTests
{
    [Fact]
    public void MatchShortArg_Valid_Short1()
    {
        var arg = "-mthis is my message";
        string? value;
        var result = ConsoleApp.MatchShortArg(arg, "-m", out value);
        Assert.True(result);
        Assert.Equal("this is my message", value);
    }

    [Fact]
    public void MatchShortArg_Valid_Short2()
    {
        var arg = "-m";
        string? value;
        var result = ConsoleApp.MatchShortArg(arg, "-m", out value);
        Assert.True(result);
        Assert.Null(value);
    }

    [Fact]
    public void MatchShortArg_Invalid_Short1()
    {
        var arg = "-rthis is my message";
        string? value;
        var result = ConsoleApp.MatchShortArg(arg, "-m", out value);
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void MatchShortArg_Invalid_Short2()
    {
        var arg = "-r";
        string? value;
        var result = ConsoleApp.MatchShortArg(arg, "-m", out value);
        Assert.False(result);
        Assert.Null(value);
    }
}
