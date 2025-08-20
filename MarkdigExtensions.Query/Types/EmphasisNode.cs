namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents an emphasis (italic) node in the markdown document.
/// Typically rendered with italic text formatting.
/// </summary>
public sealed class EmphNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "emph", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this emphasis node.
    /// </summary>
    public override string[] Selectors => ["emph", "em", "emphasis", "italic"];
}
