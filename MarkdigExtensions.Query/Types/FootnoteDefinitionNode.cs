namespace MarkdigExtensions.Query.Types;

public class FootnoteDefinitionNode(
    int[] position,
    string? footnoteId = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "footnote_definition",
        value,
        AttributeHelper.BuildAttributes(attributes, ("footnote_id", footnoteId)),
        children
    )
{
    public override string[] Selectors => ["footnote_definition", "footnote_def"];

    public string? FootnoteId => footnoteId;
}
