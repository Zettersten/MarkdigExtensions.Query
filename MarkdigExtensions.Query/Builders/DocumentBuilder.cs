using Markdig.Syntax;
using MarkdigExtensions.Query.Types;
using CoreMarkdownDocument = MarkdigExtensions.Query.Core.MarkdownDocument;

namespace MarkdigExtensions.Query.Builders;

/// <summary>
/// Builder class for constructing MarkdownDocument instances with a fluent API.
/// Provides a convenient way to build complex document structures programmatically.
/// </summary>
internal class DocumentBuilder
{
    private DocumentNode? root;
    private readonly Stack<Node> nodeStack;
    private Node? lastAddedNode;

    public DocumentBuilder()
    {
        this.nodeStack = new Stack<Node>();
    }

    #region Document Structure

    /// <summary>Creates a new document and sets it as the root.</summary>
    public DocumentBuilder CreateDocument(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.root = new DocumentNode(position, value, attributes);
        this.nodeStack.Clear();
        this.nodeStack.Push(this.root);
        this.lastAddedNode = this.root;
        return this;
    }

    /// <summary>Builds and returns the completed MarkdownDocument with integrated querying capabilities.</summary>
    public CoreMarkdownDocument Build()
    {
        if (this.root == null)
        {
            throw new InvalidOperationException(
                "No document has been created. Call CreateDocument first."
            );
        }

        return new CoreMarkdownDocument(this.root);
    }

    /// <summary>Resets the builder to its initial state.</summary>
    public DocumentBuilder Reset()
    {
        this.root = null;
        this.nodeStack.Clear();
        this.lastAddedNode = null;
        return this;
    }

    /// <summary>Sets the MarkdigRef property on the last added node.</summary>
    public DocumentBuilder SetMarkdigRef(IMarkdownObject markdigObject)
    {
        if (this.lastAddedNode != null)
        {
            this.lastAddedNode.MarkdigRef = markdigObject;
        }
        return this;
    }

    #endregion Document Structure

    #region Context Management

    /// <summary>Pushes the last added node onto the stack to make it the current parent.</summary>
    public DocumentBuilder PushContext()
    {
        if (this.nodeStack.Count > 0)
        {
            var current = this.nodeStack.Peek();

            if (current.HasChildren)
            {
                var lastChild = current.Children[current.Children.Count - 1];

                if (lastChild is Node lastChildNode)
                {
                    this.nodeStack.Push(lastChildNode);
                }
            }
        }
        return this;
    }

    /// <summary>Pops the current context, returning to the previous parent node.</summary>
    public DocumentBuilder PopContext()
    {
        if (this.nodeStack.Count > 1)
        {
            this.nodeStack.Pop();
        }
        return this;
    }

    #endregion Context Management

    #region Block-level Elements

    /// <summary>Adds a paragraph node.</summary>
    public DocumentBuilder AddParagraph(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new ParagraphNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a heading node.</summary>
    public DocumentBuilder AddHeading(
        int[] position,
        int headingLevel,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new HeadingNode(position, headingLevel, value, attributes));
        return this;
    }

    /// <summary>Adds a block quote node.</summary>
    public DocumentBuilder AddBlockQuote(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new BlockQuoteNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a list node.</summary>
    public DocumentBuilder AddList(
        int[] position,
        ListType listType = ListType.Bullet,
        ListDelimiter listDelimiter = ListDelimiter.Period,
        int listStart = 1,
        bool listTight = true,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(
            new ListNode(position, listType, listDelimiter, listStart, listTight, value, attributes)
        );
        return this;
    }

    /// <summary>Adds a list item node.</summary>
    public DocumentBuilder AddListItem(
        int[] position,
        bool isChecked = false,
        bool hasCheckbox = false,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(
            new ListItemNode(position, isChecked, hasCheckbox, value, attributes)
        );
        return this;
    }

    /// <summary>Adds a code block node.</summary>
    public DocumentBuilder AddCodeBlock(
        int[] position,
        string? fenceInfo = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new CodeBlockNode(position, fenceInfo, value, attributes));
        return this;
    }

    /// <summary>Adds an HTML block node.</summary>
    public DocumentBuilder AddHtmlBlock(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new HtmlBlockNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a custom block node.</summary>
    public DocumentBuilder AddCustomBlock(
        int[] position,
        string? customType = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new CustomBlockNode(position, customType, value, attributes));
        return this;
    }

    /// <summary>Adds a thematic break node.</summary>
    public DocumentBuilder AddThematicBreak(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new ThematicBreakNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a footnote definition node.</summary>
    public DocumentBuilder AddFootnoteDefinition(
        int[] position,
        string? footnoteId = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(
            new FootnoteDefinitionNode(position, footnoteId, value, attributes)
        );
        return this;
    }

    /// <summary>Adds a table node.</summary>
    public DocumentBuilder AddTable(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new TableNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a table row node.</summary>
    public DocumentBuilder AddTableRow(
        int[] position,
        bool isHeader = false,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new TableRowNode(position, isHeader, value, attributes));
        return this;
    }

    /// <summary>Adds a table cell node.</summary>
    public DocumentBuilder AddTableCell(
        int[] position,
        bool isHeader = false,
        TableCellNode.TableCellAlignment alignment = TableCellNode.TableCellAlignment.None,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(
            new TableCellNode(position, isHeader, alignment, value, attributes)
        );
        return this;
    }

    #endregion Block-level Elements

    #region Inline Elements

    /// <summary>Adds a text node.</summary>
    public DocumentBuilder AddText(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new TextNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a soft break node.</summary>
    public DocumentBuilder AddSoftBreak(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new SoftBreakNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a line break node.</summary>
    public DocumentBuilder AddLineBreak(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new LineBreakNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a code (inline) node.</summary>
    public DocumentBuilder AddCode(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new CodeNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds an HTML inline node.</summary>
    public DocumentBuilder AddHtmlInline(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new HtmlInlineNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a custom inline node.</summary>
    public DocumentBuilder AddCustomInline(
        int[] position,
        string? customType = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new CustomInlineNode(position, customType, value, attributes));
        return this;
    }

    /// <summary>Adds an emphasis (emph) node.</summary>
    public DocumentBuilder AddEmph(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new EmphNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a strong node.</summary>
    public DocumentBuilder AddStrong(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new StrongNode(position, value, attributes));
        return this;
    }

    /// <summary>Adds a link node.</summary>
    public DocumentBuilder AddLink(
        int[] position,
        string? url = null,
        string? title = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new LinkNode(position, url, title, value, attributes));
        return this;
    }

    /// <summary>Adds an image node.</summary>
    public DocumentBuilder AddImage(
        int[] position,
        string? url = null,
        string? alt = null,
        string? title = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new ImageNode(position, url, alt, title, value, attributes));
        return this;
    }

    /// <summary>Adds a footnote reference node.</summary>
    public DocumentBuilder AddFootnoteReference(
        int[] position,
        string? footnoteId = null,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(
            new FootnoteReferenceNode(position, footnoteId, value, attributes)
        );
        return this;
    }

    /// <summary>Adds a strikethrough node.</summary>
    public DocumentBuilder AddStrikethrough(
        int[] position,
        string? value = null,
        Dictionary<string, string?>? attributes = null
    )
    {
        this.AddChildToCurrentNode(new StrikethroughNode(position, value, attributes));
        return this;
    }

    #endregion Inline Elements

    #region Helper Methods

    private void AddChildToCurrentNode(Node child)
    {
        if (this.nodeStack.Count == 0)
        {
            throw new InvalidOperationException(
                "No current node context. Create a document first."
            );
        }

        var current = this.nodeStack.Peek();
        current.AddChild(child);
        this.lastAddedNode = child;
    }

    #endregion Helper Methods
}
