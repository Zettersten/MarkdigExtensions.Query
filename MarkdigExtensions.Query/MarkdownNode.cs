using Markdig.Syntax;

namespace MarkdigExtensions.Query;

public sealed class MarkdownNode(MarkdownObject node)
{
    private string? _cachedText;
    private string? _cachedHtml;
    private Dictionary<string, object?>? _cachedAttr;

    // For thread-safe lazy initialization
    private readonly object _initLock = new object();

    public MarkdownObject Node => node;

    public string InnerText
    {
        get
        {
            if (_cachedText == null)
            {
                lock (_initLock)
                {
                    if (_cachedText == null)
                    {
                        _cachedText = MarkdownRenderer.GetText(node);
                    }
                }
            }
            return _cachedText;
        }
    }

    public string InnerHtml
    {
        get
        {
            if (_cachedHtml == null)
            {
                lock (_initLock)
                {
                    if (_cachedHtml == null)
                    {
                        _cachedHtml = MarkdownRenderer.ToHtml(node);
                    }
                }
            }
            return _cachedHtml;
        }
    }

    public Dictionary<string, object?> Attr()
    {
        if (_cachedAttr == null)
        {
            lock (_initLock)
            {
                if (_cachedAttr == null)
                {
                    _cachedAttr = MarkdownRenderer.GetAttributes(node);
                }
            }
        }
        return _cachedAttr;
    }

    public object? Attr(string key)
    {
        var attrs = this.Attr();

        return attrs.TryGetValue(key, out var value) ? value : null;
    }

    public string ToHtml() => MarkdownRenderer.ToHtml(node);

    public string ToMarkdown() => MarkdownRenderer.ToMarkdown(node);

    public override string ToString() => this.InnerText;

    /// <summary>
    /// Traversal direction for node iteration
    /// </summary>
    private enum TraversalDirection
    {
        Forward,
        Backward,
    }

    /// <summary>
    /// Common implementation for NextUntil and PrevUntil to avoid code duplication
    /// </summary>
    private IEnumerable<MarkdownNode> TraverseUntil(
        string selector,
        TraversalDirection direction,
        bool includeStart = false,
        bool includeEnd = false,
        bool siblingsOnly = false,
        bool blockOnly = true
    )
    {
        var doc = MarkdownQueryExtensions.GetDocumentRoot(this.Node);
        if (doc == null)
            yield break;

        // Create a thread-local copy of the flattened document
        List<MarkdownObject> flat;
        lock (doc) // Use document as sync object
        {
            flat = MarkdownQueryExtensions.Flatten(doc).ToList();
        }

        var startIndex = flat.IndexOf(this.Node);
        if (startIndex == -1)
            yield break;

        // Cache the query results to avoid repeated evaluation
        var matchingNodes = MarkdownQueryExtensions
            .QueryBlocks(doc, selector)
            .Select(n => n.Node)
            .ToHashSet();

        if (includeStart && (!blockOnly || this.Node is Block))
            yield return new MarkdownNode(this.Node);

        int step = direction == TraversalDirection.Forward ? 1 : -1;
        int endBound = direction == TraversalDirection.Forward ? flat.Count : -1;

        for (int i = startIndex + step; i != endBound; i += step)
        {
            var current = flat[i];

            if (
                siblingsOnly
                && MarkdownQueryExtensions.GetParent(current)
                    != MarkdownQueryExtensions.GetParent(this.Node)
            )
                continue;

            if (matchingNodes.Contains(current))
            {
                if (includeEnd && (!blockOnly || current is Block))
                    yield return new MarkdownNode(current);
                yield break;
            }

            if (!blockOnly || current is Block)
                yield return new MarkdownNode(current);
        }
    }

    public IEnumerable<MarkdownNode> NextUntil(
        string selector,
        bool includeStart = false,
        bool includeEnd = false,
        bool siblingsOnly = false,
        bool blockOnly = true
    )
    {
        return this.TraverseUntil(
            selector,
            TraversalDirection.Forward,
            includeStart,
            includeEnd,
            siblingsOnly,
            blockOnly
        );
    }

    public IEnumerable<MarkdownNode> PrevUntil(
        string selector,
        bool includeStart = false,
        bool includeEnd = false,
        bool siblingsOnly = false,
        bool blockOnly = true
    )
    {
        return this.TraverseUntil(
            selector,
            TraversalDirection.Backward,
            includeStart,
            includeEnd,
            siblingsOnly,
            blockOnly
        );
    }

    public static IEnumerable<MarkdownNode> Between(
        MarkdownNode start,
        MarkdownNode end,
        bool includeBounds = false
    )
    {
        var startDoc = MarkdownQueryExtensions.GetDocumentRoot(start.Node);
        var endDoc = MarkdownQueryExtensions.GetDocumentRoot(end.Node);

        if (startDoc == null || endDoc == null || startDoc != endDoc)
            yield break;

        // Add thread safety with lock on document
        List<MarkdownObject> flat;
        lock (startDoc) // Use document as sync object
        {
            flat = MarkdownQueryExtensions.Flatten(startDoc).ToList();
        }

        var startIndex = flat.IndexOf(start.Node);
        var endIndex = flat.IndexOf(end.Node);

        if (startIndex == -1 || endIndex == -1 || startIndex >= endIndex)
            yield break;

        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i == startIndex && !includeBounds)
                continue;
            if (i == endIndex && !includeBounds)
                continue;
            yield return new MarkdownNode(flat[i]);
        }
    }

    public static void ReleaseDocument(MarkdownDocument document)
    {
        // Use the methods from MarkdigQueryExtensions
        MarkdownQueryExtensions.ReleaseDocument(document);
    }
}
