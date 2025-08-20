namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents an image node in the markdown document.
/// Contains URL, alt text, title, and other image-related information.
/// </summary>
public sealed class ImageNode(
    int[] position,
    string? url = null,
    string? alt = null,
    string? title = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "image",
        value,
        AttributeHelper.BuildAttributes(
            attributes,
            ("url", url),
            ("src", url),
            ("alt", alt),
            ("title", title)
        ),
        children
    )
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this image node.
    /// </summary>
    public override string[] Selectors => ["image", "img"];

    /// <summary>
    /// Gets the URL/source of the image.
    /// </summary>
    public string? Url => url;

    /// <summary>
    /// Gets the alt text of the image.
    /// </summary>
    public string? Alt => alt;

    /// <summary>
    /// Gets the title attribute of the image, if present.
    /// </summary>
    public string? Title => title;
}
