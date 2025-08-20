namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a hyperlink node in the markdown document.
/// Contains URL, title, and link text information.
/// </summary>
public sealed class LinkNode(
    int[] position,
    string? url = null,
    string? title = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "link",
        value,
        AttributeHelper.BuildAttributes(attributes, ("url", url), ("href", url), ("title", title)),
        children
    )
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this link node.
    /// </summary>
    public override string[] Selectors => ["link", "a"];

    /// <summary>
    /// Gets the URL that this link points to.
    /// </summary>
    public string? Url => url;

    /// <summary>
    /// Gets the title attribute of the link, if present.
    /// </summary>
    public string? Title => title;
}
