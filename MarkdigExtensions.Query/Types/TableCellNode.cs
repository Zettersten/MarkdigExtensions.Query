using static MarkdigExtensions.Query.Types.TableCellNode;

namespace MarkdigExtensions.Query.Types;

public class TableCellNode(
    int[] position,
    bool isHeader = false,
    TableCellAlignment alignment = TableCellAlignment.None,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "table_cell",
        value,
        AttributeHelper.BuildAttributes(
            attributes,
            ("header", isHeader.ToLowerString()),
            ("alignment", alignment.ToString().ToLowerInvariant())
        ),
        children
    )
{
    public override string[] Selectors =>
        isHeader
            ? ["table_cell", "th", "table_header_cell"]
            : ["table_cell", "td", "table_data_cell"];

    public bool IsHeader => isHeader;

    public TableCellAlignment Alignment => alignment;

    public enum TableCellAlignment
    {
        None,
        Left,
        Center,
        Right,
    }
}
