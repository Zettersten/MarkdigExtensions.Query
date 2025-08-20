namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a thematic break (horizontal rule) node in the markdown document.
/// Typically rendered as a horizontal line separating content sections.
/// </summary>
public sealed class ThematicBreakNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "thematic_break", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this thematic break node.
    /// </summary>
    public override string[] Selectors => ["thematic_break", "hr", "horizontal_rule"];
}
