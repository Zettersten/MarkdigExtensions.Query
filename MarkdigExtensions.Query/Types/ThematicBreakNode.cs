namespace MarkdigExtensions.Query.Types;

public class ThematicBreakNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "thematic_break", value, attributes, children)
{
    public override string[] Selectors => ["thematic_break", "hr", "horizontal_rule"];
}
