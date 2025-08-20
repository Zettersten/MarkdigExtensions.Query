namespace MarkdigExtensions.Query.Types;

public class BlockQuoteNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "block_quote", value, attributes, children)
{
    public override string[] Selectors => ["block_quote", "blockquote"];
}
