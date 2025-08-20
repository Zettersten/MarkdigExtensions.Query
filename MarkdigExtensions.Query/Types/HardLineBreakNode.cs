namespace MarkdigExtensions.Query.Types;

public class LineBreakNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "linebreak", value, attributes, children)
{
    public override string[] Selectors => ["linebreak", "br", "hard_line_break"];
}
