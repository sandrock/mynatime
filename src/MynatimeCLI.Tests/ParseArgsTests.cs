
namespace Mynatime.CLI.Tests;

using System;
using Xunit;

// from <https://gist.github.com/sandrock/d1fb3040e1c9326d8dd16b3bad8930ac>

public class ParseArgsTests
{
    [Fact]
    public void Loop_With0()
    {
        var inputArgs = new string[] { };
        var parser = new ParseArgs(inputArgs);

        Assert.False(parser.MoveNext());
    }

    [Fact]
    public void Loop_With1()
    {
        var inputArgs = new string[] { "hello", };
        var parser = new ParseArgs(inputArgs);

        Assert.True(parser.MoveNext());
        Assert.Equal(inputArgs[0], parser.Current);
        Assert.Equal(0, parser.Index);
        Assert.True(parser.Is(parser.Current));
        Assert.False(parser.Is("other"));
        Assert.True(parser.Remains(0));
        Assert.False(parser.Remains(1));

        Assert.False(parser.MoveNext());
    }

    [Fact]
    public void Loop_With2()
    {
        var inputArgs = new string[] { "hello", "world", };
        var parser = new ParseArgs(inputArgs);

        Assert.True(parser.MoveNext());
        Assert.Equal(0, parser.Index);
        Assert.Equal(inputArgs[parser.Index], parser.Current);
        Assert.True(parser.Is(parser.Current));
        Assert.False(parser.Is("other"));
        Assert.True(parser.Remains(1));
        Assert.False(parser.Remains(2));

        Assert.True(parser.MoveNext());
        Assert.Equal(1, parser.Index);
        Assert.Equal(inputArgs[parser.Index], parser.Current);
        Assert.True(parser.Is(parser.Current));
        Assert.False(parser.Is("other"));
        Assert.True(parser.Remains(0));
        Assert.False(parser.Remains(1));

        Assert.False(parser.MoveNext());
    }

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
