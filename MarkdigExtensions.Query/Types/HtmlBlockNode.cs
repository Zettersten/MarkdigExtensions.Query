namespace MarkdigExtensions.Query.Types;

public class HtmlBlockNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "html_block", value, attributes, children)
{
    public override string[] Selectors => ["html_block", "html"];
}
