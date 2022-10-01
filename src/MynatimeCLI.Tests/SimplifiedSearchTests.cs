
namespace MynatimeCLI.Tests;

using SimplifiedSearch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// See https://github.com/tommysor/SimplifiedSearch/issues/54
/// </summary>
public class SimplifiedSearchTests
{
    [Fact]
    public async Task Search1()
    {
        var data = GetSampleData();
        var result = await data.SimplifiedSearchAsync("extra internal"); // add option to only get the "best" match?
        Assert.Collection(result, x => Assert.Equal("Extra Internals", x));
    }

    [Fact]
    public async Task Search2()
    {
        var data = GetSampleData();
        var result = await data.SimplifiedSearchAsync("internals"); // add option to only get the "best" match?
        Assert.Collection(result, x => Assert.Equal("Internals", x));
    }

    [Fact]
    public async Task Search3()
    {
        var data = GetSampleData();
        var result = await data.SimplifiedSearchAsync("extra");
        Assert.Collection(
            result,
            x => Assert.Equal("Extra Internals", x),
            x => Assert.Equal("Extra things", x));
    }

    private static List<string> GetSampleData()
    {
        var data = new List<string>();
        data.Add("Internals");
        data.Add("Super internal");
        data.Add("Extra Internals");
        data.Add("Extra things");
        return data;
    }
}
