using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdigExtensions.Query.Builders;
using MarkdigExtensions.Query.Models;
using MarkdigExtensions.Query.Types;
using MarkdownDocument = MarkdigExtensions.Query.Core.MarkdownDocument;

namespace MarkdigExtensions.Query;

/// <summary>
/// Extension methods that add jQuery-style querying capabilities to Markdig's MarkdownDocument.
/// This is the primary entry point for developers to convert Markdig documents to queryable documents
/// and perform document analysis operations.
/// </summary>
public static partial class MarkdownQueryExtensions
{
    #region Primary Conversion Methods

    /// <summary>
    /// Converts a Markdig MarkdownDocument to a queryable MarkdownDocument with jQuery-style API.
    /// This is the primary extension method that enables all querying capabilities.
    /// </summary>
    /// <param name="markdigDocument">The Markdig document to convert</param>
    /// <returns>A queryable MarkdownDocument with jQuery-style API</returns>
    internal static MarkdownDocument ToQueryableDocument(
        this Markdig.Syntax.MarkdownDocument markdigDocument
    )
    {
        var builder = new DocumentBuilder();
        builder.CreateDocument([0]);

        var positionCounter = 1;

        // Convert the Markdig document tree to our unified structure
        foreach (var block in markdigDocument)
        {
            ConvertBlock(builder, block, ref positionCounter);
        }

        return builder.Build();
    }

    /// <summary>
    /// Converts a markdown string to a queryable MarkdownDocument using Markdig parser.
    /// Supports all GFM (GitHub Flavored Markdown) features including tables, task lists, footnotes, and more.
    /// Creates proper hierarchical structure with parent-child relationships and jQuery-style querying.
    /// </summary>
    /// <param name="markdown">The markdown string to parse and convert</param>
    /// <returns>A queryable MarkdownDocument with jQuery-style API</returns>
    internal static MarkdownDocument ToQueryableDocument(this string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var markdigDocument = Markdown.Parse(markdown, pipeline);
        return markdigDocument.ToQueryableDocument();
    }

    #endregion

    #region Document Analysis Extensions

    /// <summary>
    /// Gets comprehensive statistics about the markdown document.
    /// Provides counts of different node types and structural information.
    /// </summary>
    /// <param name="document">The queryable markdown document</param>
    /// <returns>A dictionary containing various document statistics</returns>
    public static Dictionary<string, object> GetDocumentStatistics(
        this MarkdownDocument document
    ) =>
        new()
        {
            ["TotalNodes"] = document.Count,
            ["SelectedNodes"] = document.Length,
            ["HeadingCount"] = document.GetHeadings().Get().Count,
            ["ParagraphCount"] = document.GetParagraphs().Get().Count,
            ["LinkCount"] = document.GetLinks().Get().Count,
            ["ImageCount"] = document.GetImages().Get().Count,
            ["CodeBlockCount"] = document.GetCodeBlocks().Get().Count,
            ["ListCount"] = document.GetLists().Get().Count,
            ["TableCount"] = document.GetTables().Get().Count,
            ["MaxDepth"] = document.AllNodes.Select(document.GetDepth).DefaultIfEmpty(0).Max(),
        };

    /// <summary>
    /// Analyzes the document structure and returns a hierarchical outline.
    /// Useful for generating table of contents or understanding document organization.
    /// </summary>
    /// <param name="document">The queryable markdown document</param>
    /// <returns>A structured outline of the document</returns>
    public static List<DocumentOutlineItem> GetDocumentOutline(this MarkdownDocument document)
    {
        var outline = new List<DocumentOutlineItem>();
        var headings = document.GetHeadings().Get();

        foreach (var heading in headings.Cast<HeadingNode>())
        {
            outline.Add(
                new DocumentOutlineItem
                {
                    Level = heading.Level,
                    Title = heading.Value ?? string.Empty,
                    Position = heading.Position,
                }
            );
        }

        return outline;
    }

    /// <summary>
    /// Extracts all links from the document for analysis.
    /// Returns both internal and external links with their context.
    /// </summary>
    /// <param name="document">The queryable markdown document</param>
    /// <returns>A list of link analysis objects</returns>
    public static List<LinkAnalysisItem> AnalyzeLinks(this MarkdownDocument document)
    {
        var links = new List<LinkAnalysisItem>();
        var linkNodes = document.GetLinks().Get();

        foreach (var link in linkNodes.Cast<LinkNode>())
        {
            var isExternal =
                !string.IsNullOrEmpty(link.Url)
                && (link.Url.StartsWith("http://") || link.Url.StartsWith("https://"));

            links.Add(
                new LinkAnalysisItem
                {
                    Url = link.Url ?? string.Empty,
                    Title = link.Title,
                    Text = link.Value ?? string.Empty,
                    IsExternal = isExternal,
                    Position = link.Position,
                }
            );
        }

        return links;
    }

    #endregion

    #region Internal Conversion Implementation

    private static void ConvertBlock(DocumentBuilder builder, Block block, ref int positionCounter)
    {
        var position = new[] { positionCounter++ };

        switch (block)
        {
            case HeadingBlock heading:

                _ = builder.AddHeading(position, heading.Level, ExtractText(heading.Inline));

                SetMarkdigRef(builder, heading);
                if (heading.Inline != null)
                {
                    builder.PushContext();
                    ConvertInlines(builder, heading.Inline, ref positionCounter);
                    builder.PopContext();
                }
                break;

            case ParagraphBlock paragraph:
                builder.AddParagraph(position);
                SetMarkdigRef(builder, paragraph);
                if (paragraph.Inline != null)
                {
                    builder.PushContext();
                    ConvertInlines(builder, paragraph.Inline, ref positionCounter);
                    builder.PopContext();
                }
                break;

            case QuoteBlock quote:
                builder.AddBlockQuote(position);
                SetMarkdigRef(builder, quote);
                builder.PushContext();
                foreach (var childBlock in quote)
                {
                    ConvertBlock(builder, childBlock, ref positionCounter);
                }
                builder.PopContext();
                break;

            case CodeBlock codeBlock:
                var fenceInfo = codeBlock is FencedCodeBlock fenced ? fenced.Info : null;
                builder.AddCodeBlock(position, fenceInfo, codeBlock.Lines.ToString());
                SetMarkdigRef(builder, codeBlock);
                break;

            case ListBlock list:
                var listType = list.IsOrdered ? ListType.Ordered : ListType.Bullet;
                var delimiter =
                    list.IsOrdered && list.OrderedDelimiter == '.'
                        ? ListDelimiter.Period
                        : ListDelimiter.Paren;
                var startNumber = list.IsOrdered
                    ? (int.TryParse(list.OrderedStart, out var start) ? start : 1)
                    : 1;
                var isTight = true; // Default to tight, Markdig doesn't expose this directly

                builder.AddList(position, listType, delimiter, startNumber, isTight);
                SetMarkdigRef(builder, list);
                builder.PushContext();

                foreach (ListItemBlock listItem in list.Cast<ListItemBlock>())
                {
                    var itemPosition = new[] { positionCounter++ };

                    // Check for task list item
                    bool isChecked = false;
                    bool hasCheckbox = false;

                    if (listItem.Count > 0 && listItem[0] is ParagraphBlock firstPara)
                    {
                        if (firstPara.Inline?.FirstChild is TaskList taskList)
                        {
                            hasCheckbox = true;
                            isChecked = taskList.Checked;
                        }
                    }

                    builder.AddListItem(itemPosition, isChecked, hasCheckbox);
                    SetMarkdigRef(builder, listItem);
                    builder.PushContext();

                    foreach (var itemBlock in listItem)
                    {
                        ConvertBlock(builder, itemBlock, ref positionCounter);
                    }

                    builder.PopContext();
                }

                builder.PopContext();
                break;

            case ThematicBreakBlock:
                builder.AddThematicBreak(position);
                SetMarkdigRef(builder, block);
                break;

            case HtmlBlock htmlBlock:
                var htmlContent = htmlBlock.Lines.ToString();
                builder.AddHtmlBlock(position, htmlContent);
                SetMarkdigRef(builder, htmlBlock);
                break;

            case Table table:
                builder.AddTable(position);
                SetMarkdigRef(builder, table);
                builder.PushContext();

                // Add header row if exists
                if (table.Count > 0)
                {
                    if (table[0] is TableRow headerRow)
                    {
                        var headerRowPosition = new[] { positionCounter++ };
                        builder.AddTableRow(headerRowPosition, true);
                        SetMarkdigRef(builder, headerRow);
                        builder.PushContext();

                        for (int i = 0; i < headerRow.Count; i++)
                        {
                            if (headerRow[i] is TableCell cell)
                            {
                                var cellPosition = new[] { positionCounter++ };
                                var alignment = ConvertTableAlignment(
                                    table.ColumnDefinitions[i].Alignment
                                );
                                builder.AddTableCell(cellPosition, true, alignment);
                                SetMarkdigRef(builder, cell);

                                builder.PushContext();
                                // Convert cell content - TableCell contains blocks, not inlines
                                foreach (var cellBlock in cell)
                                {
                                    ConvertBlock(builder, cellBlock, ref positionCounter);
                                }
                                builder.PopContext();
                            }
                        }

                        builder.PopContext();
                    }

                    // Add data rows
                    for (int rowIndex = 1; rowIndex < table.Count; rowIndex++)
                    {
                        if (table[rowIndex] is TableRow dataRow)
                        {
                            var dataRowPosition = new[] { positionCounter++ };
                            builder.AddTableRow(dataRowPosition, false);
                            SetMarkdigRef(builder, dataRow);
                            builder.PushContext();

                            for (int i = 0; i < dataRow.Count; i++)
                            {
                                if (dataRow[i] is TableCell cell)
                                {
                                    var cellPosition = new[] { positionCounter++ };
                                    var alignment =
                                        i < table.ColumnDefinitions.Count
                                            ? ConvertTableAlignment(
                                                table.ColumnDefinitions[i].Alignment
                                            )
                                            : TableCellNode.TableCellAlignment.None;
                                    builder.AddTableCell(cellPosition, false, alignment);
                                    SetMarkdigRef(builder, cell);

                                    builder.PushContext();
                                    // Convert cell content - TableCell contains blocks, not inlines
                                    foreach (var cellBlock in cell)
                                    {
                                        ConvertBlock(builder, cellBlock, ref positionCounter);
                                    }
                                    builder.PopContext();
                                }
                            }

                            builder.PopContext();
                        }
                    }
                }

                builder.PopContext();
                break;

            case FootnoteGroup footnoteGroup:
                foreach (var footnote in footnoteGroup)
                {
                    if (footnote is Footnote footnoteBlock)
                    {
                        var footnotePosition = new[] { positionCounter++ };
                        builder.AddFootnoteDefinition(footnotePosition, footnoteBlock.Label);
                        SetMarkdigRef(builder, footnoteBlock);
                        builder.PushContext();

                        foreach (var footnoteChildBlock in footnoteBlock)
                        {
                            ConvertBlock(builder, footnoteChildBlock, ref positionCounter);
                        }

                        builder.PopContext();
                    }
                }
                break;

            // Handle container blocks that might contain other blocks
            case ContainerBlock container:
                foreach (var childBlock in container)
                {
                    ConvertBlock(builder, childBlock, ref positionCounter);
                }
                break;
        }
    }

    private static void ConvertInlines(
        DocumentBuilder builder,
        ContainerInline? container,
        ref int positionCounter
    )
    {
        if (container == null)
            return;

        foreach (var inline in container)
        {
            ConvertInline(builder, inline, ref positionCounter);
        }
    }

    private static void ConvertInline(
        DocumentBuilder builder,
        Inline inline,
        ref int positionCounter
    )
    {
        var position = new[] { positionCounter++ };

        switch (inline)
        {
            case LiteralInline literal:
                builder.AddText(position, literal.Content.ToString());
                SetMarkdigRef(builder, literal);
                break;

            case EmphasisInline emphasis:
                if (emphasis.DelimiterChar == '*' || emphasis.DelimiterChar == '_')
                {
                    if (emphasis.DelimiterCount == 1)
                    {
                        builder.AddEmph(position);
                        SetMarkdigRef(builder, emphasis);
                        builder.PushContext();
                        ConvertInlines(builder, emphasis, ref positionCounter);
                        builder.PopContext();
                    }
                    else if (emphasis.DelimiterCount == 2)
                    {
                        builder.AddStrong(position);
                        SetMarkdigRef(builder, emphasis);
                        builder.PushContext();
                        ConvertInlines(builder, emphasis, ref positionCounter);
                        builder.PopContext();
                    }
                }
                break;

            case LinkInline link:
                if (link.IsImage)
                {
                    // Handle as image
                    builder.AddImage(position, link.Url, null, link.Title);
                    SetMarkdigRef(builder, link);
                    builder.PushContext();
                    ConvertInlines(builder, link, ref positionCounter);
                    builder.PopContext();
                }
                else
                {
                    // Handle as regular link
                    builder.AddLink(position, link.Url, link.Title);
                    SetMarkdigRef(builder, link);
                    builder.PushContext();
                    ConvertInlines(builder, link, ref positionCounter);
                    builder.PopContext();
                }
                break;

            case AutolinkInline autolink:
                builder.AddLink(position, autolink.Url, null, autolink.Url);
                SetMarkdigRef(builder, autolink);
                break;

            case CodeInline code:
                builder.AddCode(position, code.Content);
                SetMarkdigRef(builder, code);
                break;

            case HtmlInline html:
                builder.AddHtmlInline(position, html.Tag);
                SetMarkdigRef(builder, html);
                break;

            case LineBreakInline lineBreak:
                if (lineBreak.IsHard)
                {
                    builder.AddLineBreak(position);
                    SetMarkdigRef(builder, lineBreak);
                }
                else
                {
                    builder.AddSoftBreak(position);
                    SetMarkdigRef(builder, lineBreak);
                }
                break;

            case FootnoteLink footnoteLink:
                builder.AddFootnoteReference(position, footnoteLink.Index.ToString());
                SetMarkdigRef(builder, footnoteLink);
                break;

            case ContainerInline containerInline:
                ConvertInlines(builder, containerInline, ref positionCounter);
                break;

            // Handle special cases
            case TaskList:
                // Task list markers are handled at the list item level
                break;

            default:
                // For any unhandled inline types, try to extract text content
                var text = ExtractText(inline);
                if (!string.IsNullOrEmpty(text))
                {
                    builder.AddText(position, text);
                    SetMarkdigRef(builder, inline);
                }
                break;
        }
    }

    /// <summary>
    /// Sets the MarkdigRef property on the most recently added node in the builder.
    /// </summary>
    private static void SetMarkdigRef(DocumentBuilder builder, IMarkdownObject markdigObject)
    {
        builder.SetMarkdigRef(markdigObject);
    }

    private static string ExtractText(Inline? inline)
    {
        if (inline == null)
            return string.Empty;

        return inline switch
        {
            LiteralInline literal => literal.Content.ToString(),
            ContainerInline container => string.Concat(container.Select(ExtractText)),
            CodeInline code => code.Content,
            AutolinkInline autolink => autolink.Url,
            _ => string.Empty,
        };
    }

    private static TableCellNode.TableCellAlignment ConvertTableAlignment(
        TableColumnAlign? alignment
    )
    {
        return alignment switch
        {
            TableColumnAlign.Left => TableCellNode.TableCellAlignment.Left,
            TableColumnAlign.Center => TableCellNode.TableCellAlignment.Center,
            TableColumnAlign.Right => TableCellNode.TableCellAlignment.Right,
            _ => TableCellNode.TableCellAlignment.None,
        };
    }

    #endregion
}
