using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using static MarkdigExtensions.Query.SelectorPart;

namespace MarkdigExtensions.Query;

public static partial class MarkdownQueryExtensions
{
    // Make this internal so MarkdownNode can access it
    internal static readonly ConditionalWeakTable<
        MarkdownDocument,
        ConcurrentDictionary<MarkdownObject, MarkdownObject?>
    > documentParentMaps = new();

    internal static readonly object documentParentMapsLock = new object();

    internal static ConcurrentDictionary<MarkdownObject, MarkdownObject?> GetOrCreateParentMap(
        MarkdownDocument doc
    )
    {
        lock (documentParentMapsLock)
        {
            return documentParentMaps.GetOrCreateValue(doc);
        }
    }

    internal static bool TryGetParentMap(
        MarkdownDocument doc,
        out ConcurrentDictionary<MarkdownObject, MarkdownObject?> map
    )
    {
        lock (documentParentMapsLock)
        {
            return documentParentMaps.TryGetValue(doc, out map);
        }
    }

    public static IEnumerable<MarkdownNode> QueryBlocks(
        this MarkdownDocument document,
        string selector
    )
    {
        var selectorGroups = ParseSelectorGroups(selector);

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
        var selectorGroups = ParseSelectorGroups(selector);

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
        var selectorGroups = ParseSelectorGroups(selector);

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
        var selectorGroups = ParseSelectorGroups(selector);

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

    internal static MarkdownObject? GetParent(MarkdownObject node)
    {
        var doc = GetDocumentForNode(node);
        if (doc == null)
            return null;

        var docParentMap = documentParentMaps.GetOrCreateValue(doc);

        return docParentMap.TryGetValue(node, out var parent) ? parent : null;
    }

    private static void RegisterParent(
        MarkdownObject node,
        MarkdownObject? parent,
        MarkdownDocument doc
    )
    {
        var parentMap = documentParentMaps.GetOrCreateValue(doc);
        parentMap[node] = parent;
    }

    private static MarkdownDocument? GetDocumentForNode(MarkdownObject node)
    {
        if (node is MarkdownDocument doc)
            return doc;

        foreach (var docParentMapPair in documentParentMaps)
        {
            var parentMap = docParentMapPair.Value;

            if (parentMap.ContainsKey(node))
            {
                var current = node;
                while (current != null && !(current is MarkdownDocument))
                {
                    if (!parentMap.TryGetValue(current, out var parent))
                        break;
                    current = parent;
                }

                return current as MarkdownDocument;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the root document for a markdown node.
    /// </summary>
    /// <param name="node">The markdown node to find the document for</param>
    /// <returns>The root document containing the node, or null if not found</returns>
    public static MarkdownDocument? GetDocumentRoot(this MarkdownObject node)
    {
        // Direct case: node is already a document
        if (node is MarkdownDocument doc)
            return doc;

        // Try to find the document through the parent mapping
        var foundDoc = GetDocumentForNode(node);
        if (foundDoc != null)
            return foundDoc;

        // Fallback: walk up the parent chain
        var current = node;
        while (current != null)
        {
            if (current is MarkdownDocument document)
                return document;
            current = GetParent(current);
        }

        return null;
    }

    internal static IEnumerable<MarkdownObject> Flatten(MarkdownObject root)
    {
        var doc = root as MarkdownDocument;
        if (doc != null)
        {
            // Pre-build parent map for this document in a thread-safe way
            BuildParentMap(doc);
        }

        // Now yield nodes without modifying shared state
        return FlattenWithoutParentMapUpdates(root);
    }

    private static void BuildParentMap(MarkdownDocument doc)
    {
        // Get the parent map in a thread-safe way
        var parentMap = GetOrCreateParentMap(doc);

        Stack<(MarkdownObject node, MarkdownObject? parent)> stack = new();
        stack.Push((doc, null));

        while (stack.Count > 0)
        {
            var (node, parent) = stack.Pop();

            // Use thread-safe ConcurrentDictionary methods
            parentMap.AddOrUpdate(node, parent, (_, _) => parent);

            if (node is ContainerBlock cb)
            {
                foreach (var child in cb.Reverse())
                    stack.Push((child, node));
            }

            if (node is LeafBlock lb && lb.Inline != null)
            {
                BuildInlineParentMap(lb.Inline, node, doc);
            }
        }
    }

    private static void BuildInlineParentMap(
        ContainerInline container,
        MarkdownObject parent,
        MarkdownDocument doc
    )
    {
        var parentMap = GetOrCreateParentMap(doc);
        foreach (var child in container)
        {
            // Use thread-safe ConcurrentDictionary methods
            parentMap.AddOrUpdate(child, parent, (_, _) => parent);

            if (child is ContainerInline ci)
            {
                BuildInlineParentMap(ci, child, doc);
            }
        }
    }

    private static IEnumerable<MarkdownObject> FlattenWithoutParentMapUpdates(MarkdownObject root)
    {
        Stack<MarkdownObject> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;

            if (node is ContainerBlock cb)
            {
                foreach (var child in cb.Reverse())
                    stack.Push(child);
            }

            if (node is LeafBlock lb && lb.Inline != null)
            {
                foreach (var inline in FlattenInlineWithoutUpdates(lb.Inline))
                    yield return inline;
            }
        }
    }

    private static IEnumerable<MarkdownObject> FlattenInlineWithoutUpdates(
        ContainerInline container
    )
    {
        foreach (var child in container)
        {
            yield return child;

            if (child is ContainerInline ci)
            {
                foreach (var sub in FlattenInlineWithoutUpdates(ci))
                    yield return sub;
            }
        }
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
                        (
                            attrKey == "url"
                            && string.Equals(li.Url, attrVal, StringComparison.OrdinalIgnoreCase)
                        )
                        || (
                            attrKey == "href"
                            && string.Equals(li.Url, attrVal, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                ),

            "image" => obj is LinkInline img
                && img.IsImage
                && (
                    attrKey == null
                    || (
                        attrKey == "src"
                        && string.Equals(img.Url, attrVal, StringComparison.OrdinalIgnoreCase)
                    )
                ),

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

                var match = PsuedoSelectorPattern().Match(normalized);

                var tag = match.Groups["tag"].Value;
                var attrKey = match.Groups["attr"].Success ? match.Groups["attr"].Value : null;
                var attrVal = match.Groups["val"].Success ? match.Groups["val"].Value : null;
                var pseudo = match.Groups["pseudo"].Success ? match.Groups["pseudo"].Value : null;

                parts.Add(new SelectorPart(tag, attrKey, attrVal, currentCombinator, pseudo));
                currentCombinator = CombinatorType.Descendant;
            }
        }

        return parts;
    }

    private static List<List<SelectorPart>> ParseSelectorGroups(string selector)
    {
        return
        [
            .. selector
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(ParseSelectorChain),
        ];
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

    public static void ReleaseDocument(MarkdownDocument document)
    {
        documentParentMaps.Remove(document);
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
