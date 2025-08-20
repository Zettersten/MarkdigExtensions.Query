namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a table node in the markdown document.
/// Contains table rows, which in turn contain table cells.
/// </summary>
public sealed class TableNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "table", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this table node.
    /// </summary>
    public override string[] Selectors => ["table"];
}
