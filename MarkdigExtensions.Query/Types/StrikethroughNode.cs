namespace MarkdigExtensions.Query.Types;

public class StrikethroughNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "strikethrough", value, attributes, children)
{
    public override string[] Selectors => ["strikethrough", "del", "s"];
}
