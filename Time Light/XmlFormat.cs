using System.Xml.Linq;

static class XmlFormat
{
    public const string Node = "node";
    public const string Leaf = "leaf";
    public const string Total = "total";
    public const string Billed = "billed";
    public const string Unbilled = "unbilled";
    public const string Default = "default";
    const string name = "name";

    /// <summary>Extract an element's name attribute</summary>
    public static string Name(XElement element)
    {
        return element.Attribute(name).Value;
    }
}