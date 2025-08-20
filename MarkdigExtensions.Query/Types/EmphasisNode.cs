namespace MarkdigExtensions.Query.Types;

public class EmphNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "emph", value, attributes, children)
{
    public override string[] Selectors => ["emph", "em", "emphasis", "italic"];
}
