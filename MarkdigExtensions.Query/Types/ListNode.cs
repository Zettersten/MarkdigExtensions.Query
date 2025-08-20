namespace MarkdigExtensions.Query.Types;

public enum ListType
{
    Bullet,
    Ordered,
}

public enum ListDelimiter
{
    Period,
    Paren,
}

public class ListNode(
    int[] position,
    ListType listType = ListType.Bullet,
    ListDelimiter listDelimiter = ListDelimiter.Period,
    int listStart = 1,
    bool listTight = true,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "list",
        value,
        AttributeHelper.BuildAttributes(
            attributes,
            ("list_type", listType.ToString().ToUpperInvariant()),
            ("list_delim", listDelimiter.ToString().ToUpperInvariant()),
            ("list_start", listStart.ToString()),
            ("list_tight", listTight.ToLowerString())
        ),
        children
    )
{
    public override string[] Selectors =>
        listType == ListType.Ordered
            ? ["list", "ol", "ordered_list"]
            : ["list", "ul", "unordered_list"];

    public ListType Type => listType;
    public ListDelimiter Delimiter => listDelimiter;
    public int Start => listStart;
    public bool IsTight => listTight;
}
