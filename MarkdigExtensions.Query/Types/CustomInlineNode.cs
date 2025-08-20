namespace MarkdigExtensions.Query.Types;

public class CustomInlineNode(
    int[] position,
    string? customType = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "custom_inline",
        value,
        AttributeHelper.BuildAttributes(attributes, ("custom_type", customType)),
        children
    )
{
    public override string[] Selectors => ["custom_inline", "custom"];

    public string? CustomType => customType;
}
