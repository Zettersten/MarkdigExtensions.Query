namespace MarkdigExtensions.Query.Types;

public class FootnoteReferenceNode(
    int[] position,
    string? footnoteId = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "footnote_reference",
        value,
        AttributeHelper.BuildAttributes(attributes, ("footnote_id", footnoteId)),
        children
    )
{
    public override string[] Selectors => ["footnote_reference", "footnote_ref"];

    public string? FootnoteId => footnoteId;
}
