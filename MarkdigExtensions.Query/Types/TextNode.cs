namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a text node containing the actual text content in the markdown document.
/// Text nodes are typically leaf nodes in the document tree.
/// </summary>
public sealed class TextNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "text", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this text node.
    /// </summary>
    public override string[] Selectors => ["text", "#text"];
}
