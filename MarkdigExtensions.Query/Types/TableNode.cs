namespace MarkdigExtensions.Query.Types;

public class TableNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "table", value, attributes, children)
{
    public override string[] Selectors => ["table"];
}
