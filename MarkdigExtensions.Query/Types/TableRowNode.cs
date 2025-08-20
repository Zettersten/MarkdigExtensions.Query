namespace MarkdigExtensions.Query.Types;

public class TableRowNode(
    int[] position,
    bool isHeader = false,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "table_row",
        value,
        AttributeHelper.BuildAttributes(attributes, ("header", isHeader.ToLowerString())),
        children
    )
{
    public override string[] Selectors =>
        isHeader ? ["table_row", "tr", "thead", "table_header_row"] : ["table_row", "tr", "tbody"];

    public bool IsHeader => isHeader;
}
