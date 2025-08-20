namespace MarkdigExtensions.Query.Types;

public class TextNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "text", value, attributes, children)
{
    public override string[] Selectors => ["text", "#text"];
}
