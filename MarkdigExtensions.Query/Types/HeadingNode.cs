namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a heading node in the markdown document (H1-H6).
/// Provides access to heading level and standard heading selectors.
/// </summary>
public sealed class HeadingNode(
    int[] position,
    int headingLevel,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "heading",
        value,
        AttributeHelper.BuildAttributes(attributes, ("heading_level", headingLevel.ToString())),
        children
    )
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this heading node.
    /// Includes "heading", "h", and the specific heading level selector (e.g., "h1", "h2").
    /// </summary>
    public override string[] Selectors => ["heading", "h", $"h{headingLevel}"];

    /// <summary>
    /// Gets the heading level (1-6).
    /// </summary>
    public int Level => headingLevel;

    /// <summary>
    /// Gets the heading level (1-6). Alias for <see cref="Level"/>.
    /// </summary>
    public int HeadingLevel => headingLevel;
}
