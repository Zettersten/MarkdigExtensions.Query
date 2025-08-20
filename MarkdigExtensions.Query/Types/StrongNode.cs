namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a strong emphasis (bold) node in the markdown document.
/// Typically rendered with bold text formatting.
/// </summary>
public sealed class StrongNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "strong", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this strong node.
    /// </summary>
    public override string[] Selectors => ["strong", "b", "bold"];
}
