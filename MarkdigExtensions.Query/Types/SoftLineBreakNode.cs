namespace MarkdigExtensions.Query.Types;

public class SoftBreakNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "softbreak", value, attributes, children)
{
    public override string[] Selectors => ["softbreak", "soft_line_break"];
}
