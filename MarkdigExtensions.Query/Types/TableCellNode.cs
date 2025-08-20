using static MarkdigExtensions.Query.Types.TableCellNode;

namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a table cell node in the markdown document.
/// Can be either a header cell (th) or data cell (td) with optional alignment.
/// </summary>
public sealed class TableCellNode(
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
    /// <summary>
    /// Gets the CSS selectors that can be used to match this table cell node.
    /// Returns different selectors based on whether this is a header or data cell.
    /// </summary>
    public override string[] Selectors =>
        isHeader
            ? ["table_cell", "th", "table_header_cell"]
            : ["table_cell", "td", "table_data_cell"];

    /// <summary>
    /// Gets a value indicating whether this is a header cell.
    /// </summary>
    public bool IsHeader => isHeader;

    /// <summary>
    /// Gets the text alignment for this table cell.
    /// </summary>
    public TableCellAlignment Alignment => alignment;

    /// <summary>
    /// Represents the text alignment options for table cells.
    /// </summary>
    public enum TableCellAlignment
    {
        /// <summary>
        /// No specific alignment specified.
        /// </summary>
        None,

        /// <summary>
        /// Left-aligned text.
        /// </summary>
        Left,

        /// <summary>
        /// Center-aligned text.
        /// </summary>
        Center,

        /// <summary>
        /// Right-aligned text.
        /// </summary>
        Right,
    }
}
