namespace MarkdigExtensions.Query;

/// <summary>
/// Represents a part of a CSS-like selector used for querying Markdown documents
/// </summary>
public readonly record struct SelectorPart
{
    public string Tag { get; init; }

    public string? AttributeKey { get; init; }

    public string? AttributeValue { get; init; }

    public CombinatorType Combinator { get; init; }

    public string? PseudoSelector { get; init; }

    public SelectorPart(
        string tag,
        string? attributeKey = null,
        string? attributeValue = null,
        CombinatorType combinator = CombinatorType.Descendant,
        string? pseudoSelector = null
    )
    {
        Tag = tag;
        AttributeKey = attributeKey;
        AttributeValue = attributeValue;
        Combinator = combinator;
        PseudoSelector = pseudoSelector;
    }

    public enum CombinatorType
    {
        Descendant,
        DirectChild,
    }
}
