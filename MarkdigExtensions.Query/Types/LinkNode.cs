namespace MarkdigExtensions.Query.Types;

public class LinkNode(
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
    public override string[] Selectors => ["link", "a"];

    public string? Url => url;
    public string? Title => title;
}
