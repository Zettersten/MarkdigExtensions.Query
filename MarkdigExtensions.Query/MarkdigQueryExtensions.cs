using System.Text.RegularExpressions;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using static MarkdigExtensions.Query.SelectorPart;

namespace MarkdigExtensions.Query;

public static partial class MarkdownQueryExtensions
{
    public static IEnumerable<MarkdownNode> QueryBlocks(
        this MarkdownDocument document,
        string selector
    )
    {
        var selectorGroups = selector
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(ParseSelectorChain)
            .ToList();

        foreach (var node in Flatten(document))
        {
            foreach (var group in selectorGroups)
            {
                if (MatchesSelectorChain(node, group))
                {
                    yield return new MarkdownNode(node);
                }
            }
        }
    }

    public static IEnumerable<MarkdownNode> QueryBlocks(this MarkdownNode node, string selector)
    {
        var selectorGroups = selector
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(ParseSelectorChain)
            .ToList();

        var decedents = node.Node.Descendants().OfType<MarkdownObject>().ToList();

        foreach (var descendant in decedents)
        {
            foreach (var group in selectorGroups)
            {
                if (MatchesSelectorChain(descendant, group))
                {
                    yield return new MarkdownNode(descendant);
                }
            }
        }
    }

    public static MarkdownNode? QueryBlock(this MarkdownNode node, string selector)
    {
        var selectorGroups = selector
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(ParseSelectorChain)
            .ToList();

        var decedents = node.Node.Descendants().OfType<MarkdownObject>().ToList();

        foreach (var descendant in decedents)
        {
            foreach (var group in selectorGroups)
            {
                if (MatchesSelectorChain(descendant, group))
                {
                    return new MarkdownNode(descendant);
                }
            }
        }

        return null;
    }

    public static MarkdownNode? QueryBlock(this MarkdownDocument document, string selector)
    {
        var selectorGroups = selector
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(ParseSelectorChain)
            .ToList();

        foreach (var node in Flatten(document))
        {
            foreach (var group in selectorGroups)
            {
                if (MatchesSelectorChain(node, group))
                {
                    return new MarkdownNode(node);
                }
            }
        }

        return null;
    }

    private static bool MatchesSelectorChain(MarkdownObject node, List<SelectorPart> chain)
    {
        if (chain.Count == 0)
        {
            return false;
        }

        return MatchFromLeaf(node, chain.Count - 1);

        bool MatchFromLeaf(MarkdownObject current, int depth)
        {
            if (depth < 0)
                return true;

            var selectorPart = chain[depth];

            if (
                !IsMatch(
                    current,
                    selectorPart.Tag,
                    selectorPart.AttributeKey,
                    selectorPart.AttributeValue
                )
            )
            {
                return false;
            }

            if (!MatchesPseudo(current, selectorPart.PseudoSelector))
            {
                return false;
            }

            if (depth == 0)
            {
                return true;
            }

            return selectorPart.Combinator switch
            {
                CombinatorType.DirectChild => GetParent(current) is { } parent
                    && MatchFromLeaf(parent, depth - 1),

                CombinatorType.Descendant => WalkAncestors(current)
                    .Any(ancestor => MatchFromLeaf(ancestor, depth - 1)),

                _ => false,
            };
        }

        static IEnumerable<MarkdownObject> WalkAncestors(MarkdownObject start)
        {
            var current = GetParent(start);
            while (current != null)
            {
                yield return current;
                current = GetParent(current);
            }
        }
    }

    private static List<SelectorPart> ParseSelectorChain(string selectorChain)
    {
        var parts = new List<SelectorPart>();
        var tokens = SelectorChainPattern().Split(selectorChain);

        var currentCombinator = CombinatorType.Descendant;

        foreach (var token in tokens)
        {
            var t = token.Trim();
            if (string.IsNullOrEmpty(t))
                continue;

            if (t == ">")
            {
                currentCombinator = CombinatorType.DirectChild;
            }
            else
            {
                var normalized = NormalizeSelector(t);

                // Regex to extract tag, optional [attr=val], optional :pseudo
                var match = PsuedoSelectorPattern().Match(normalized);

                var tag = match.Groups["tag"].Value;
                var attrKey = match.Groups["attr"].Success ? match.Groups["attr"].Value : null;
                var attrVal = match.Groups["val"].Success ? match.Groups["val"].Value : null;
                var pseudo = match.Groups["pseudo"].Success ? match.Groups["pseudo"].Value : null;

                parts.Add(new SelectorPart(tag, attrKey, attrVal, currentCombinator, pseudo));
                currentCombinator = CombinatorType.Descendant; // reset after use
            }
        }

        return parts;
    }

    private static bool MatchesPseudo(MarkdownObject obj, string? pseudo)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
            return true;

        var parent = GetParent(obj);

        if (parent is not ContainerBlock)
        {
            return false;
        }

        var siblings = parent switch
        {
            ContainerBlock block => [.. block.Cast<MarkdownObject>()],
            ContainerInline inline => inline.Cast<MarkdownObject>().ToList(),
            _ => null,
        };

        if (siblings == null)
            return false;

        var index = siblings.IndexOf(obj);

        return pseudo switch
        {
            "first-child" => index == 0,
            "last-child" => index == siblings.Count - 1,
            "even" => index % 2 == 0,
            "odd" => index % 2 == 1,
            var p when p.StartsWith("nth-child(") && p.EndsWith(')') => int.TryParse(
                p["nth-child(".Length..^1],
                out var n
            )
                && index == (n - 1),
            _ => false,
        };
    }

    private static string NormalizeSelector(string selector)
    {
        return selector switch
        {
            "html" => "html",
            "a" => "link",
            "link" => "link",
            "b" => "strong",
            "i" => "emphasis",
            "em" => "emphasis",
            "img" => "image",
            "p" => "paragraph",
            "ol" => "ol",
            "ul" => "ul",
            "blockquote" => "blockquote",
            "hr" => "thematicbreak",
            "code" => "codeblock",
            "table" => "table",
            var s when HeadingLevelPattern().IsMatch(s) => $"heading[level={s[1]}]",
            _ => selector,
        };
    }

    private static readonly Dictionary<MarkdownObject, MarkdownObject?> parentMap = [];

    internal static IEnumerable<MarkdownObject> Flatten(MarkdownObject root)
    {
        Stack<(MarkdownObject node, MarkdownObject? parent)> stack = new();
        stack.Push((root, null));

        while (stack.Count > 0)
        {
            var (node, parent) = stack.Pop();
            parentMap[node] = parent;
            yield return node;

            if (node is ContainerBlock cb)
            {
                foreach (var child in cb.Reverse())
                    stack.Push((child, node));
            }

            if (node is LeafBlock lb && lb.Inline != null)
            {
                foreach (var inline in FlattenInline(lb.Inline, node))
                    yield return inline;
            }
        }
    }

    private static IEnumerable<MarkdownObject> FlattenInline(
        ContainerInline container,
        MarkdownObject parent
    )
    {
        foreach (var child in container)
        {
            parentMap[child] = parent;
            yield return child;

            if (child is ContainerInline ci)
            {
                foreach (var sub in FlattenInline(ci, child))
                    yield return sub;
            }
        }
    }

    internal static MarkdownObject? GetParent(MarkdownObject node)
    {
        return parentMap.TryGetValue(node, out var parent) ? parent : null;
    }

    private static IEnumerable<MarkdownObject> FlattenInline(ContainerInline container)
    {
        foreach (var child in container)
        {
            yield return child;

            if (child is ContainerInline ci)
            {
                foreach (var sub in FlattenInline(ci))
                    yield return sub;
            }
        }
    }

    private static bool IsMatch(MarkdownObject obj, string tag, string? attrKey, string? attrVal)
    {
        tag = tag.ToLowerInvariant();

        if (tag == "*")
        {
            return true;
        }

        return tag switch
        {
            "heading" => obj is HeadingBlock h
                && (attrKey == null || (attrKey == "level" && attrVal == h.Level.ToString())),

            "paragraph" => obj is ParagraphBlock,

            "link" => obj is LinkInline li
                && !li.IsImage
                && (
                    attrKey == null
                    || (
                        (attrKey == "url" && li.Url == attrVal)
                        || (attrKey == "href" && li.Url == attrVal)
                    )
                ),

            "image" => obj is LinkInline img
                && img.IsImage
                && (attrKey == null || (attrKey == "src" && img.Url == attrVal)),

            "emphasis" => obj is EmphasisInline em && em.DelimiterCount == 1,

            "strong" => obj is EmphasisInline st && st.DelimiterCount >= 2,

            "codeblock" => obj is FencedCodeBlock cb
                && (
                    attrKey == null
                    || (
                        attrKey == "language"
                        && string.Equals(cb.Info, attrVal, StringComparison.OrdinalIgnoreCase)
                    )
                ),

            "table" => obj is Table,

            "ol" => obj is ListBlock list && list.IsOrdered,

            "ul" => obj is ListBlock list && !list.IsOrdered,

            "li" => obj is ListItemBlock li
                && (
                    attrKey == null
                    || (
                        (attrKey == "index" && li.Order.ToString() == attrVal)
                        || attrKey == "order" && li.Order.ToString() == attrVal
                    )
                ),

            "blockquote" => obj is QuoteBlock,

            "html" => obj is HtmlBlock,

            "thematicbreak" => obj is ThematicBreakBlock,

            _ => false,
        };
    }

    [GeneratedRegex(@"h[1-6]")]
    private static partial Regex HeadingLevelPattern();

    [GeneratedRegex(@"(?<=[^\s>])\s*(>)\s*|\s+")]
    private static partial Regex SelectorChainPattern();

    [GeneratedRegex(
        @"^(?<tag>[^\[:]+|\*)(\[(?<attr>[^\]=]+)=?(?<val>[^\]]*)\])?(:(?<pseudo>.+))?$"
    )]
    private static partial Regex PsuedoSelectorPattern();
}
