using System.Text;
using Markdig;
using Markdig.Syntax;

namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Abstract base class for all markdown nodes.
/// Provides common functionality for node traversal, attributes, and rendering capabilities.
/// </summary>
public abstract class Node(
    int[] position,
    string name,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : INode
{
    private readonly List<INode> children = children ?? [];

    /// <summary>
    /// Gets the CSS selectors that can be used to match this node type.
    /// </summary>
    public abstract string[] Selectors { get; }

    /// <summary>
    /// Gets a value indicating whether this node has child nodes.
    /// </summary>
    public bool HasChildren => this.children.Count > 0;

    /// <summary>
    /// Gets the read-only collection of child nodes.
    /// </summary>
    public IReadOnlyList<INode> Children => this.children.AsReadOnly();

    /// <summary>
    /// Gets the name/type identifier of this node.
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// Gets the text value or content of this node, if applicable.
    /// </summary>
    public string? Value { get; } = value;

    /// <summary>
    /// Gets the read-only dictionary of attributes associated with this node.
    /// </summary>
    public IReadOnlyDictionary<string, string?> Attributes { get; } =
        attributes ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the position of this node in the document tree as a hierarchical array of indices.
    /// </summary>
    public int[] Position => position;

    /// <summary>
    /// Gets or sets the original Markdig object reference (Block or Inline).
    /// </summary>
    public IMarkdownObject? MarkdigRef { get; set; }

    /// <summary>
    /// Adds a child node to this node's children collection.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="child"/> is null.</exception>
    public void AddChild(INode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        this.children.Add(child);
    }

    /// <summary>
    /// Renders this node and all its children as plain text.
    /// </summary>
    /// <returns>A plain text representation of the node content.</returns>
    public virtual string GetPlainText()
    {
        // Build markdown first, then convert to plain text using Markdig
        var markdown = this.GetMarkdown();
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        try
        {
            return Markdown.ToPlainText(markdown);
        }
        catch
        {
            // Fallback to simple text extraction if Markdig conversion fails
            return this.GetSimplePlainText();
        }
    }

    /// <summary>
    /// Renders this node and all its children as HTML.
    /// </summary>
    /// <returns>An HTML representation of the node content.</returns>
    public virtual string GetHtml()
    {
        // Build markdown first, then convert to HTML using Markdig
        var markdown = this.GetMarkdown();
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        try
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            return Markdown.ToHtml(markdown, pipeline);
        }
        catch
        {
            // Fallback to simple HTML encoding if Markdig conversion fails
            return System.Net.WebUtility.HtmlEncode(this.GetSimplePlainText());
        }
    }

    /// <summary>
    /// Renders this node and all its children as Markdown.
    /// </summary>
    /// <returns>A Markdown representation of the node content.</returns>
    public virtual string GetMarkdown()
    {
        // Default implementation builds markdown from node structure
        return this.BuildMarkdownFromStructure();
    }

    /// <summary>
    /// Builds markdown representation from the node structure.
    /// </summary>
    /// <returns>The markdown representation of this node and its children.</returns>
    protected virtual string BuildMarkdownFromStructure()
    {
        var markdown = new StringBuilder();

        switch (this.Name.ToLowerInvariant())
        {
            case "heading":
                if (this is HeadingNode heading)
                {
                    var level = Math.Max(1, Math.Min(6, heading.HeadingLevel));
                    markdown.Append(new string('#', level));
                    markdown.Append(' ');
                    markdown.Append(this.GetChildrenMarkdown());
                    markdown.AppendLine();
                }
                break;

            case "paragraph":
                markdown.Append(this.GetChildrenMarkdown());
                markdown.AppendLine();
                markdown.AppendLine();
                break;

            case "text":
                markdown.Append(this.Value ?? string.Empty);
                break;

            case "strong":
                markdown.Append("**");
                markdown.Append(this.GetChildrenMarkdown());
                markdown.Append("**");
                break;

            case "emph":
                markdown.Append('*');
                markdown.Append(this.GetChildrenMarkdown());
                markdown.Append('*');
                break;

            case "code":
                markdown.Append('`');
                markdown.Append(this.Value ?? string.Empty);
                markdown.Append('`');
                break;

            case "link":
                if (this is LinkNode link)
                {
                    markdown.Append('[');
                    markdown.Append(this.GetChildrenMarkdown());
                    markdown.Append("](");
                    markdown.Append(link.Url ?? string.Empty);
                    if (!string.IsNullOrEmpty(link.Title))
                    {
                        markdown.Append(" \"");
                        markdown.Append(link.Title);
                        markdown.Append('"');
                    }
                    markdown.Append(')');
                }
                break;

            case "image":
                if (this is ImageNode image)
                {
                    markdown.Append("![");
                    markdown.Append(this.GetChildrenMarkdown());
                    markdown.Append("](");
                    markdown.Append(image.Url ?? string.Empty);
                    if (!string.IsNullOrEmpty(image.Title))
                    {
                        markdown.Append(" \"");
                        markdown.Append(image.Title);
                        markdown.Append('"');
                    }
                    markdown.Append(')');
                }
                break;

            case "code_block":
                if (this is CodeBlockNode codeBlock)
                {
                    markdown.Append("```");
                    if (!string.IsNullOrEmpty(codeBlock.Language))
                        markdown.Append(codeBlock.Language);
                    markdown.AppendLine();
                    markdown.Append(this.Value ?? string.Empty);
                    if (!string.IsNullOrEmpty(this.Value) && !this.Value.EndsWith('\n'))
                        markdown.AppendLine();
                    markdown.AppendLine("```");
                }
                break;

            case "list":
                if (this is ListNode list)
                {
                    var isOrdered = list.Type == ListType.Ordered;
                    var startNumber = list.Start;

                    for (int i = 0; i < this.Children.Count; i++)
                    {
                        var item = this.Children[i];
                        if (isOrdered)
                        {
                            markdown.Append($"{startNumber + i}. ");
                        }
                        else
                        {
                            markdown.Append("- ");
                        }

                        var itemMarkdown = item.GetMarkdown().TrimEnd();
                        // Handle multi-line list items by indenting continuation lines
                        var lines = itemMarkdown.Split('\n');
                        markdown.Append(lines[0]);
                        for (int j = 1; j < lines.Length; j++)
                        {
                            markdown.AppendLine();
                            markdown.Append("  ");
                            markdown.Append(lines[j]);
                        }
                        markdown.AppendLine();
                    }
                    markdown.AppendLine();
                }
                break;

            case "item":
                // List items are handled by their parent list
                markdown.Append(this.GetChildrenMarkdown());
                break;

            case "block_quote":
                var blockquoteContent = this.GetChildrenMarkdown();
                var blockquoteLines = blockquoteContent.Split(
                    '\n',
                    StringSplitOptions.RemoveEmptyEntries
                );
                foreach (var line in blockquoteLines)
                {
                    markdown.Append("> ");
                    markdown.AppendLine(line);
                }
                markdown.AppendLine();
                break;

            case "thematic_break":
                markdown.AppendLine("---");
                markdown.AppendLine();
                break;

            case "table":
                // Tables require special handling - let the table implementation handle this
                if (this is TableNode)
                {
                    markdown.Append(this.BuildTableMarkdown());
                }
                break;

            case "html_block":
                markdown.Append(this.Value ?? string.Empty);
                if (!string.IsNullOrEmpty(this.Value) && !this.Value.EndsWith('\n'))
                    markdown.AppendLine();
                break;

            case "line_break":
                markdown.Append("  ");
                markdown.AppendLine();
                break;

            case "soft_break":
                markdown.Append(' ');
                break;

            default:
                // For unknown node types, just render children
                markdown.Append(this.GetChildrenMarkdown());
                break;
        }

        return markdown.ToString();
    }

    /// <summary>
    /// Gets markdown content from all children.
    /// </summary>
    /// <returns>The combined markdown content of all child nodes.</returns>
    protected string GetChildrenMarkdown()
    {
        var markdown = new StringBuilder();
        foreach (var child in this.Children)
        {
            markdown.Append(child.GetMarkdown());
        }
        return markdown.ToString();
    }

    /// <summary>
    /// Builds table markdown representation.
    /// </summary>
    /// <returns>The markdown representation of a table.</returns>
    protected virtual string BuildTableMarkdown()
    {
        var markdown = new StringBuilder();
        var rows = this.Children.Where(c => c.Name == "table_row").ToList();

        if (rows.Count == 0)
            return string.Empty;

        // Header row
        var headerRow = rows[0];
        var cells = headerRow.Children.Where(c => c.Name == "table_cell").ToList();

        if (cells.Count > 0)
        {
            markdown.Append("| ");
            foreach (var cell in cells)
            {
                markdown.Append(cell.GetMarkdown().Trim().Replace('\n', ' '));
                markdown.Append(" | ");
            }
            markdown.AppendLine();

            // Separator row
            markdown.Append("| ");
            foreach (var cell in cells)
            {
                if (cell is TableCellNode tableCell)
                {
                    markdown.Append(
                        tableCell.Alignment switch
                        {
                            TableCellNode.TableCellAlignment.Left => ":---",
                            TableCellNode.TableCellAlignment.Center => ":---:",
                            TableCellNode.TableCellAlignment.Right => "---:",
                            _ => "---",
                        }
                    );
                }
                else
                {
                    markdown.Append("---");
                }
                markdown.Append(" | ");
            }
            markdown.AppendLine();

            // Data rows
            for (int i = 1; i < rows.Count; i++)
            {
                var dataRow = rows[i];
                var dataCells = dataRow.Children.Where(c => c.Name == "table_cell").ToList();

                markdown.Append("| ");
                for (int j = 0; j < Math.Max(cells.Count, dataCells.Count); j++)
                {
                    if (j < dataCells.Count)
                    {
                        markdown.Append(dataCells[j].GetMarkdown().Trim().Replace('\n', ' '));
                    }
                    markdown.Append(" | ");
                }
                markdown.AppendLine();
            }

            markdown.AppendLine();
        }

        return markdown.ToString();
    }

    /// <summary>
    /// Simple fallback method for extracting plain text when Markdig conversion fails.
    /// </summary>
    /// <returns>The plain text content extracted from this node and its children.</returns>
    protected virtual string GetSimplePlainText()
    {
        var text = new StringBuilder();

        if (!string.IsNullOrEmpty(this.Value))
        {
            text.Append(this.Value);
        }

        foreach (var child in this.Children)
        {
            if (child is Node childNode)
            {
                text.Append(childNode.GetSimplePlainText());
            }
        }

        return text.ToString();
    }
}
