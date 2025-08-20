namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a paragraph node in the markdown document.
/// Paragraphs are block-level elements that typically contain inline content such as text, links, and emphasis.
/// </summary>
public sealed class ParagraphNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "paragraph", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this paragraph node.
    /// </summary>
    public override string[] Selectors => ["paragraph", "p"];
}
