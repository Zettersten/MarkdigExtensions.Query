using System.Text;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdigExtensions.Query;

/// <summary>
/// Handles rendering of markdown content in various formats
/// </summary>
public static class MarkdownRenderer
{
    private static readonly Lazy<MarkdownPipeline> HtmlPipeline = new Lazy<MarkdownPipeline>(
        () => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build(),
        LazyThreadSafetyMode.ExecutionAndPublication
    );

    /// <summary>
    /// Extracts plain text from a markdown node
    /// </summary>
    public static string GetText(MarkdownObject node)
    {
        try
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
                    var text = GetText(child);
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.Append(text).Append('\n');
                }
                return sb.ToString().TrimEnd();
            }

            return node.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            // Provide a safe fallback
            return $"Error extracting text: {ex.Message}";
        }
    }

    /// <summary>
    /// Renders a markdown node as HTML
    /// </summary>
    public static string ToHtml(MarkdownObject node)
    {
        try
        {
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);

            var renderer = new HtmlRenderer(writer);
            HtmlPipeline.Value.Setup(renderer); // Use the shared pipeline
            renderer.Render(node);
            writer.Flush();

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"<!-- Error rendering HTML: {ex.Message} -->{node}";
        }
    }

    /// <summary>
    /// Renders a markdown node back to markdown format
    /// </summary>
    public static string ToMarkdown(MarkdownObject node)
    {
        try
        {
            // Best-effort text-only markdown fallback
            return node switch
            {
                LinkInline li => $"[{li.FirstOrDefault()}]({li.Url})",
                EmphasisInline em => em.DelimiterCount == 1
                    ? $"*{SerializeInline(em)}*"
                    : $"**{SerializeInline(em)}**",
                FencedCodeBlock fcb => $"```{fcb.Info}\n{fcb.Lines}\n```",
                HeadingBlock h => new string('#', h.Level) + " " + GetText(h),
                ParagraphBlock p => GetText(p),
                _ => GetText(node),
            };
        }
        catch (Exception ex)
        {
            // Provide a safe fallback
            return $"Error rendering markdown: {ex.Message}";
        }
    }

    /// <summary>
    /// Extracts attributes from a markdown node based on its type
    /// </summary>
    public static Dictionary<string, object?> GetAttributes(MarkdownObject node)
    {
        var dict = new Dictionary<string, object?>();

        try
        {
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
        }
        catch (Exception ex)
        {
            // Add error information to the dictionary
            dict["error"] = ex.Message;
        }

        return dict;
    }

    /// <summary>
    /// Helper method to serialize inline content
    /// </summary>
    private static string SerializeInline(ContainerInline? inline)
    {
        try
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
        catch (Exception ex)
        {
            // Provide a safe fallback
            return $"Error processing inline content: {ex.Message}";
        }
    }
}
