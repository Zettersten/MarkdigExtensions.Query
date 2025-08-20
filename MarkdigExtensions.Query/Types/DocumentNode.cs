namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents the root document node of a markdown document.
/// This is the top-level container for all other nodes in the document tree.
/// </summary>
public sealed class DocumentNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "document", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this document node.
    /// </summary>
    public override string[] Selectors => ["document"];
}
