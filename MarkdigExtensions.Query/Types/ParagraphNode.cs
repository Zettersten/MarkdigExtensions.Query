namespace MarkdigExtensions.Query.Types;

public class ParagraphNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "paragraph", value, attributes, children)
{
    public override string[] Selectors => ["paragraph", "p"];
}
