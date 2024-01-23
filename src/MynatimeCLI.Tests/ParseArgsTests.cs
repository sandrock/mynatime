
namespace Mynatime.CLI.Tests;

using System;
using Xunit;

public class ParseArgsTests
{
    [Fact]
    public void LoopOnZero()
    {
        var args = Array.Empty<string>();
        var target = new ParseArgs(args);

        Verify(target, args);
    }

    [Fact]
    public void LoopOnOne()
    {
        var args = new string[] { "a", };
        var target = new ParseArgs(args);

        Verify(target, args);
    }

    [Fact]
    public void LoopOnTwo()
    {
        var args = new string[] { "a", "b", };
        var target = new ParseArgs(args);

        Verify(target, args);
    }

    [Fact]
    public void RemainsOnZero()
    {
        var args = Array.Empty<string>();
        var target = new ParseArgs(args);
        var result = target.Remains();
        Assert.Empty(result);
        Assert.Equal(0, target.Index);
    }

    [Fact]
    public void RemainsOnOne()
    {
        var args = new string[] { "a", };
        var target = new ParseArgs(args);
        var result = target.Remains();
        Assert.Collection(
            result,
            x => { Assert.Equal(args[0], x); });
        Assert.Equal(1, target.Index);
    }

    [Fact]
    public void RemainsOnTwo()
    {
        var args = new string[] { "a", "b", };
        var target = new ParseArgs(args);
        var result = target.Remains();
        Assert.Collection(
            result,
            x => { Assert.Equal(args[0], x); },
            x => { Assert.Equal(args[1], x); });
        Assert.Equal(2, target.Index);
    }

    private static void Verify(ParseArgs target, string[] args)
    {
        var itemCount = 0;
        while (target.MoveNext())
        {
            Assert.False(target.Has(args.Length - itemCount));
            Assert.Equal(itemCount, target.Index);
            Assert.Equal(args[itemCount], target.Current);
            Assert.True(target.Is(args[itemCount]));
            Assert.True(target.Is(args[itemCount], "xxx"));

            var nexts = target.GetNexts();
            Assert.Equal(args.Length - itemCount, nexts.Length);

            itemCount++;
        }

        Assert.Equal(args.Length, itemCount);
    }
}
