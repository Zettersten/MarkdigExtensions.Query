using System.Text;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Query;

public sealed class MarkdownNode(MarkdownObject node)
{
    private string? _cachedText;
    private string? _cachedHtml;
    private Dictionary<string, object?>? _cachedAttr;

    public MarkdownObject Node => node;

    public string InnerText
    {
        get
        {
            if (_cachedText is null)
            {
                _cachedText = ComputeText(node);
            }

            return _cachedText;
        }
    }
    public string InnerHtml => _cachedHtml ??= ComputeHtml(node);

    public Dictionary<string, object?> Attr()
    {
        return _cachedAttr ??= ComputeAttrs(node);
    }

    public object? Attr(string key)
    {
        var attrs = _cachedAttr ??= ComputeAttrs(node);

        return attrs.TryGetValue(key, out var value) ? value : null;
    }

    public string ToHtml() => ComputeHtml(node);

    public string ToMarkdown() => ComputeMarkdown(node);

    public override string ToString() => InnerText;

    public IEnumerable<MarkdownNode> NextUntil(
        string selector,
        bool includeStart = false,
        bool includeEnd = false,
        bool siblingsOnly = false,
        bool blockOnly = true
    )
    {
        var doc = GetDocumentRoot();
        if (doc == null)
            yield break;

        var flat = MarkdownQueryExtensions.Flatten(doc).ToList();
        var startIndex = flat.IndexOf(this.Node);
        if (startIndex == -1)
            yield break;

        if (includeStart && (!blockOnly || this.Node is Block))
            yield return new MarkdownNode(this.Node);

        for (int i = startIndex + 1; i < flat.Count; i++)
        {
            var current = flat[i];

            if (
                siblingsOnly
                && MarkdownQueryExtensions.GetParent(current)
                    != MarkdownQueryExtensions.GetParent(this.Node)
            )
                continue;

            if (MarkdownQueryExtensions.QueryBlocks(doc, selector).Any(n => n.Node == current))
            {
                if (includeEnd && (!blockOnly || current is Block))
                    yield return new MarkdownNode(current);
                yield break;
            }

            if (!blockOnly || current is Block)
                yield return new MarkdownNode(current);
        }
    }

    public IEnumerable<MarkdownNode> PrevUntil(
        string selector,
        bool includeStart = false,
        bool includeEnd = false,
        bool siblingsOnly = false,
        bool blockOnly = true
    )
    {
        var doc = GetDocumentRoot();
        if (doc == null)
            yield break;

        var flat = MarkdownQueryExtensions.Flatten(doc).ToList();
        var startIndex = flat.IndexOf(this.Node);
        if (startIndex == -1)
            yield break;

        if (includeStart && (!blockOnly || this.Node is Block))
            yield return new MarkdownNode(this.Node);

        for (int i = startIndex - 1; i >= 0; i--)
        {
            var current = flat[i];

            if (
                siblingsOnly
                && MarkdownQueryExtensions.GetParent(current)
                    != MarkdownQueryExtensions.GetParent(this.Node)
            )
                continue;

            if (MarkdownQueryExtensions.QueryBlocks(doc, selector).Any(n => n.Node == current))
            {
                if (includeEnd && (!blockOnly || current is Block))
                    yield return new MarkdownNode(current);
                yield break;
            }

            if (!blockOnly || current is Block)
                yield return new MarkdownNode(current);
        }
    }

    public static IEnumerable<MarkdownNode> Between(
        MarkdownNode start,
        MarkdownNode end,
        bool includeBounds = false
    )
    {
        var doc = start.GetDocumentRoot();
        if (doc == null || end.GetDocumentRoot() != doc)
            yield break;

        var flat = MarkdownQueryExtensions.Flatten(doc).ToList();
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

    private MarkdownDocument? GetDocumentRoot()
    {
        if (this.Node is MarkdownDocument doc)
            return doc;

        var current = this.Node;
        while (current != null)
        {
            if (current is MarkdownDocument d)
                return d;
            current = MarkdownQueryExtensions.GetParent(current);
        }
        return null;
    }

    #region -- Rendering Helpers --

    private static string ComputeText(MarkdownObject node)
    {
        if (node is LeafBlock lb && lb.Inline != null)
        {
            return SerializeInline(lb.Inline);
        }

        if (node is ContainerInline ci)
        {
            return SerializeInline(ci);
        }

        if (node is ThematicBreakBlock)
        {
            return string.Empty;
        }

        if (node is ContainerBlock cb)
        {
            // Concatenate the inner text of child LeafBlocks
            var sb = new StringBuilder();
            foreach (var child in cb)
            {
                var text = ComputeText(child);
                if (!string.IsNullOrWhiteSpace(text))
                    sb.Append(text).Append('\n');
            }
            return sb.ToString().TrimEnd();
        }

        return node.ToString() ?? string.Empty;
    }

    private static string ComputeHtml(MarkdownObject node)
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);

        var renderer = new HtmlRenderer(writer);
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

        pipeline.Setup(renderer);
        renderer.Render(node);
        writer.Flush();

        return sb.ToString();
    }

    private static string ComputeMarkdown(MarkdownObject node)
    {
        // Best-effort text-only markdown fallback
        return node switch
        {
            LinkInline li => $"[{li.FirstOrDefault()}]({li.Url})",
            EmphasisInline em => em.DelimiterCount == 1
                ? $"*{SerializeInline(em)}*"
                : $"**{SerializeInline(em)}**",
            FencedCodeBlock fcb => $"```{fcb.Info}\n{fcb.Lines}\n```",
            HeadingBlock h => new string('#', h.Level) + " " + ComputeText(h),
            ParagraphBlock p => ComputeText(p),
            _ => ComputeText(node),
        };
    }

    private static Dictionary<string, object?> ComputeAttrs(MarkdownObject node)
    {
        var dict = new Dictionary<string, object?>();

        switch (node)
        {
            case LinkInline li:
                dict["isImage"] = li.IsImage;

                if (li.IsImage)
                {
                    dict["src"] = li.Url;
                    dict["alt"] = li.Title;
                }
                else
                {
                    dict["href"] = li.Url;
                    dict["title"] = li.Title;
                }

                dict["text"] = SerializeInline(li);
                break;

            case HeadingBlock h:
                dict["level"] = h.Level;
                dict["text"] = SerializeInline(h.Inline);
                break;

            case EmphasisInline em:
                dict["type"] = em.DelimiterCount == 1 ? "italic" : "bold";
                dict["text"] = SerializeInline(em);
                break;

            case FencedCodeBlock cb:
                dict["language"] = cb.Info;
                dict["text"] = cb.Lines.ToString();
                break;

            case Table t:
                dict["type"] = "table";
                break;

            case ListBlock l:
                dict["ordered"] = l.IsOrdered;
                dict["count"] = l.Count;
                break;

            case ParagraphBlock p:
                dict["text"] = SerializeInline(p.Inline);
                break;
        }

        return dict;
    }

    private static string SerializeInline(ContainerInline? inline)
    {
        var sb = new StringBuilder();

        if (inline is null)
        {
            return string.Empty;
        }

        foreach (var child in inline)
        {
            switch (child)
            {
                case LiteralInline lit:
                    sb.Append(lit.Content.ToString());
                    break;

                case LineBreakInline:
                    sb.Append('\n');
                    break;

                case CodeInline ci:
                    sb.Append('`').Append(ci.Content).Append('`');
                    break;

                case EmphasisInline em:
                    var delim = em.DelimiterCount == 2 ? "**" : "*";
                    sb.Append(delim).Append(SerializeInline(em)).Append(delim);
                    break;

                case LinkInline li:
                    if (li.IsImage)
                    {
                        sb.Append("![")
                            .Append(SerializeInline(li))
                            .Append("](")
                            .Append(li.Url)
                            .Append(')');
                    }
                    else
                    {
                        sb.Append("[")
                            .Append(SerializeInline(li))
                            .Append("](")
                            .Append(li.Url)
                            .Append(')');
                    }
                    break;

                case HtmlInline html:
                    sb.Append(html.Tag);
                    break;

                case ContainerInline ci:
                    sb.Append(SerializeInline(ci));
                    break;

                default:
                    sb.Append(child.ToString());
                    break;
            }
        }

        return sb.ToString();
    }

    #endregion -- Rendering Helpers --
}
