namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a soft line break node in the markdown document.
/// Soft breaks are typically rendered as spaces in the final output.
/// </summary>
public sealed class SoftBreakNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "softbreak", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this soft break node.
    /// </summary>
    public override string[] Selectors => ["softbreak", "soft_line_break"];
}
