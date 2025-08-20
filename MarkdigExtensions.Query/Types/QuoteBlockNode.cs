namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a block quote node in the markdown document.
/// Block quotes are typically used for quoting text from other sources.
/// </summary>
public sealed class BlockQuoteNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "block_quote", value, attributes, children)
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this block quote node.
    /// </summary>
    public override string[] Selectors => ["block_quote", "blockquote"];
}
