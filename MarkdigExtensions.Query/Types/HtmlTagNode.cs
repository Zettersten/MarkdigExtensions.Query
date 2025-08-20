namespace MarkdigExtensions.Query.Types;

public class HtmlInlineNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "html_inline", value, attributes, children)
{
    public override string[] Selectors => ["html_inline", "html"];
}
