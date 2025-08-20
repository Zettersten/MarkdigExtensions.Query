namespace MarkdigExtensions.Query.Types;

public class StrongNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "strong", value, attributes, children)
{
    public override string[] Selectors => ["strong", "b", "bold"];
}
