namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents the type of list (bullet or ordered).
/// </summary>
public enum ListType
{
    /// <summary>
    /// Bullet list (unordered list with bullet points).
    /// </summary>
    Bullet,
    
    /// <summary>
    /// Ordered list (numbered list).
    /// </summary>
    Ordered,
}

/// <summary>
/// Represents the delimiter used in ordered lists.
/// </summary>
public enum ListDelimiter
{
    /// <summary>
    /// Period delimiter (1. 2. 3.).
    /// </summary>
    Period,
    
    /// <summary>
    /// Parenthesis delimiter (1) 2) 3)).
    /// </summary>
    Paren,
}

/// <summary>
/// Represents a list node in the markdown document (ordered or unordered).
/// Contains list items and metadata about list formatting.
/// </summary>
public sealed class ListNode(
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
    /// <summary>
    /// Gets the CSS selectors that can be used to match this list node.
    /// Returns different selectors based on whether this is an ordered or unordered list.
    /// </summary>
    public override string[] Selectors =>
        listType == ListType.Ordered
            ? ["list", "ol", "ordered_list"]
            : ["list", "ul", "unordered_list"];

    /// <summary>
    /// Gets the type of this list (bullet or ordered).
    /// </summary>
    public ListType Type => listType;
    
    /// <summary>
    /// Gets the delimiter used in this ordered list.
    /// </summary>
    public ListDelimiter Delimiter => listDelimiter;
    
    /// <summary>
    /// Gets the starting number for ordered lists.
    /// </summary>
    public int Start => listStart;
    
    /// <summary>
    /// Gets a value indicating whether this is a tight list (no blank lines between items).
    /// </summary>
    public bool IsTight => listTight;
}
