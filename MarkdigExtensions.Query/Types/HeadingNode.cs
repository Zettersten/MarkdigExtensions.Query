namespace MarkdigExtensions.Query.Types;

public class HeadingNode(
    int[] position,
    int headingLevel,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "heading",
        value,
        AttributeHelper.BuildAttributes(attributes, ("heading_level", headingLevel.ToString())),
        children
    )
{
    public override string[] Selectors => ["heading", "h", $"h{headingLevel}"];

    public int Level => headingLevel;

    public int HeadingLevel => headingLevel;
}
