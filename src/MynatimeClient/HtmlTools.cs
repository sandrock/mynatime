using System.Globalization;
using System.Text;
using System.Web;

namespace Mynatime.Client;

public static class HtmlTools
{
    /// <summary>
    /// Basic HTML node reader. Will return nodes (text or element) in order. 
    /// </summary>
    /// <param name="input">HTML string</param>
    /// <returns>HTML nodes</returns>
    public static IEnumerable<HtmlNode> EnumerateElementsAndValues(string input)
    {
        HtmlNode? node = new HtmlNode();
        HtmlNode? attribute = null;
        node.Value = string.Empty;
        bool readElementName = false, inAttributes = false;
        bool readAttributeName = false, readAttributeValue = false, attributeInQuotes = false;
        var attributeText = new StringBuilder();
        int startOfName = -1, startOfText = 0;
        for (int i = 0; i < input.Length; i++)
        {
            var c =  input[i];
            var next = input.Substring(i, Math.Min(3, input.Length - i));

            if (next.StartsWith("/>"))
            {
                node.Name = input.Substring(startOfName, i - startOfName);
                node.IsClosing = true;
                yield return node;
                
                node = new HtmlNode();
                readElementName = false;
                inAttributes = false;
                i++;
                startOfText = i + 1;
            }
            else if (next.StartsWith("</", StringComparison.Ordinal))
            {
                node.Value = HttpUtility.HtmlDecode(input.Substring(startOfText, i - startOfText));
                yield return node;
                
                node = new HtmlNode();
                node.IsClosing = true;
                readElementName = true;
                startOfName = i + 2;
                i++;
            }
            else if (c == '<')
            {
                attribute = null;
                node.Value = HttpUtility.HtmlDecode(input.Substring(startOfText, i - startOfText));
                yield return node;

                node = new HtmlNode();
                node.IsOpening = true;
                readElementName = true;
                startOfName = i + 1;
            }
            else if (c == ' ' && readElementName)
            {
                node.Name = input.Substring(startOfName, i - startOfName);
                readElementName = false;
                inAttributes = true;
            }
            else if (c == '>' && readElementName)
            {
                node.Name = input.Substring(startOfName, i - startOfName);
                readElementName = false;
                inAttributes = false;
                yield return node;
                node = new HtmlNode();
                startOfText = i + 1;
            }
            else if (c == '>')
            {
                inAttributes = false; attribute = null; attributeText.Clear();
                yield return node;
                
                node = new HtmlNode();
                startOfText = i + 1;
            }
            else if (inAttributes)
            {
                if (attribute == null)
                {
                    attribute = new HtmlNode();
                    attribute.IsAttribute = true;
                    node.Attributes ??= new List<HtmlNode>();
                    node.Attributes.Add(attribute);
                    readAttributeName = true;
                    attributeInQuotes = false;
                }

                if (c == '=' && readAttributeName)
                {
                    readAttributeName = false;
                    readAttributeValue = true;
                    attribute.Name = attributeText.ToString().Trim();
                    attributeText.Clear();
                }
                else if (c == '"' && readAttributeValue)
                {
                    attributeInQuotes = !attributeInQuotes;
                }
                else if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator && !attributeInQuotes)
                {
                    attribute.Value = HttpUtility.HtmlDecode(attributeText.ToString().Trim());
                    attributeText.Clear();
                    attribute = null;
                    readAttributeName = false;
                    readAttributeValue = false;
                }
                else
                {
                    attributeText.Append(c);
                    if (readAttributeValue)
                    {
                        attribute.Value = HttpUtility.HtmlDecode(attributeText.ToString());
                    }
                }
            }
            else
            {
            }
        }

        node.Value = HttpUtility.HtmlDecode(input.Substring(startOfText, input.Length - startOfText));
        yield return node;
    }

    /// <summary>
    /// From an HTML input containing option and optgroup elements, returns the options available.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static IEnumerable<HtmlNode> EnumerateOptions(string input)
    {
        HtmlNode item = new HtmlNode();
        HtmlNode? group = null;
        var name = new StringBuilder();
        foreach (var node in EnumerateElementsAndValues(input))
        {
            if ("option".Equals(node.Name, StringComparison.Ordinal) && node.IsOpening)
            {
                item = new HtmlNode();
                item.Attributes = new List<HtmlNode>();
                if (node.Attributes != null)
                {
                    foreach (var attribute in node.Attributes)
                    {
                        item.Attributes.Add(attribute);
                        if ("value".Equals(attribute.Name, StringComparison.Ordinal))
                        {
                            item.Value = attribute.Value;
                        }
                    }
                }
            }
            else if ("option".Equals(node.Name, StringComparison.Ordinal) && node.IsClosing)
            {
                if (group != null)
                {
                    item.Name = group.Name + " / " + name.ToString().Trim();
                }
                else
                {
                    item.Name = name.ToString().Trim();
                }

                yield return item;
                name.Clear();
            }
            else if ("optgroup".Equals(node.Name, StringComparison.Ordinal) && node.IsOpening)
            {
                group = new HtmlNode();
                group.Attributes = new List<HtmlNode>();
                if (node.Attributes != null)
                {
                    foreach (var attribute in node.Attributes)
                    {
                        group.Attributes.Add(attribute);
                        if ("label".Equals(attribute.Name, StringComparison.Ordinal))
                        {
                            group.Name = attribute.Value;
                        }
                    }
                }
            }
            else if ("optgroup".Equals(node.Name, StringComparison.Ordinal) && node.IsClosing)
            {
                group = null;
            }
            else if (node.Name == null)
            {
                name.Append(node.Value);
            }
        }
    }

    public sealed class HtmlNode
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public bool IsOpening { get; set; }
        public bool IsClosing { get; set; }
        public bool IsAttribute { get; set; }
        public List<HtmlNode>? Attributes { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (this.IsAttribute)
            {
                builder.Append(this.Name);
                builder.Append('=');
                builder.Append(this.Value);
            }
            else
            {
                if (this.Name != null)
                {
                    builder.Append(this.IsClosing ? "</" : "<");
                    builder.Append(this.Name);
                    builder.Append(this.IsOpening && this.IsClosing ? "/>" : ">");
                }
                else
                {
                    builder.Append("[");
                    builder.Append(this.Value);
                    builder.Append("]");
                }
            }

            return builder.ToString();
        }
    }
}
