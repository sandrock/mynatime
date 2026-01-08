using System.Linq;
using Xunit;

namespace Mynatime.Client.Tests;

public class HtmlToolsTests
{
    [Fact]
    public void EnumerateElementsAndValues_Test1()
    {
        var input = """a<b>c<d>e</b>f<g/>""";
        var items = HtmlTools.EnumerateElementsAndValues(input).ToList();
        Assert.Collection(
            items,
            x => AssertValue(x, "a"),
            x => AssertElement(x, "b", true, false),
            x => AssertValue(x, "c"),
            x => AssertElement(x, "d", true, false),
            x => AssertValue(x, "e"),
            x => AssertElement(x, "b", false, true),
            x => AssertValue(x, "f"),
            x => AssertElement(x, "g", true, true),
            x => AssertValue(x, string.Empty));
    }

    [Fact]
    public void EnumerateElementsAndValues_Test2()
    {
        var input = """<a id=y href="x">link</a>""";
        var items = HtmlTools.EnumerateElementsAndValues(input);
        Assert.Collection(
            items,
            x => AssertValue(x, string.Empty),
            x =>
            {
                AssertElement(x, "a", true, false);
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "id", "y"),
                    a => AssertNameAndValue(a, "href", "x"));
            },
            x => AssertValue(x, "link"),
            x => AssertElement(x, "a", false, true),
            x => AssertValue(x, string.Empty));
    }

    [Fact]
    public void EnumerateElementsAndValues_Test3()
    {
        var input = """<a id=y href="x">link</a><code class=c title="hello world">xyz</code>""";
        var items = HtmlTools.EnumerateElementsAndValues(input);
        Assert.Collection(
            items,
            x => AssertValue(x, string.Empty),
            x =>
            {
                AssertElement(x, "a", true, false);
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "id", "y"),
                    a => AssertNameAndValue(a, "href", "x"));
            },
            x => AssertValue(x, "link"),
            x => AssertElement(x, "a", false, true),
            x => AssertValue(x, string.Empty),
            x =>
            {
                AssertElement(x, "code", true, false);
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "class", "c"),
                    a => AssertNameAndValue(a, "title", "hello world"));
            },
            x => AssertValue(x, "xyz"),
            x => AssertElement(x, "code", false, true),
            x => AssertValue(x, string.Empty));
    }

    [Fact]
    public void ReadOptions()
    {
        var input = """
                    <option value="7000">Communication</option>
                    <optgroup label="Project-100"><option value="7001***1400">sub1</option><option value="7001***1401">sub2&amp;sub3</option></optgroup>
                    <option value="7002">Xommunication</option>
                    """;
        var items = HtmlTools.EnumerateOptions(input);
        Assert.Collection(
            items,
            x =>
            {
                AssertNameAndValue(x, "Communication", "7000");
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "value", "7000"));
            },
            x =>
            {
                AssertNameAndValue(x, "Project-100 / sub1", "7001***1400");
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "value", "7001***1400"));
            },
            x =>
            {
                AssertNameAndValue(x, "Project-100 / sub2&sub3", "7001***1401");
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "value", "7001***1401"));
            },
            x =>
            {
                AssertNameAndValue(x, "Xommunication", "7002");
                Assert.Collection(
                    x.Attributes,
                    a => AssertNameAndValue(a, "value", "7002"));
            });
    }

    private void AssertElement(HtmlTools.HtmlNode node, string name, bool isOpening, bool isClosing)
    {
        Assert.Equal(name, node.Name);
        Assert.Equal(isOpening, node.IsOpening);
        Assert.Equal(isClosing, node.IsClosing);
        Assert.Null(node.Value);
    }

    private void AssertNameAndValue(HtmlTools.HtmlNode node, string name, string value)
    {
        Assert.Equal(name, node.Name);
        Assert.Equal(value, node.Value);
    }

    private void AssertValue(HtmlTools.HtmlNode node, string value)
    {
        Assert.Null(node.Name);
        Assert.Equal(value, node.Value);
        Assert.False(node.IsOpening);
        Assert.False(node.IsClosing);
    }
}
