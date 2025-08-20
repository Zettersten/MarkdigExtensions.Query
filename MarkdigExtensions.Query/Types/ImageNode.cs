namespace MarkdigExtensions.Query.Types;

public class ImageNode(
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
    public override string[] Selectors => ["image", "img"];

    public string? Url => url;

    public string? Alt => alt;

    public string? Title => title;
}
