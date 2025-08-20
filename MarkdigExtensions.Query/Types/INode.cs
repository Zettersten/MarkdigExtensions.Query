using Markdig.Syntax;

namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a node in the markdown document tree.
/// Provides a unified interface for accessing node properties and rendering capabilities.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the position of this node in the document tree as a hierarchical array of indices.
    /// </summary>
    int[] Position { get; }

    /// <summary>
    /// Gets a value indicating whether this node has child nodes.
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    /// Gets the read-only collection of child nodes.
    /// </summary>
    IReadOnlyList<INode> Children { get; }

    /// <summary>
    /// Gets the name/type identifier of this node (e.g., "heading", "paragraph", "text").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the text value or content of this node, if applicable.
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Gets the read-only dictionary of attributes associated with this node.
    /// </summary>
    IReadOnlyDictionary<string, string?> Attributes { get; }

    /// <summary>
    /// Gets the original Markdig object reference (Block or Inline) that this node was created from.
    /// </summary>
    IMarkdownObject? MarkdigRef { get; }

    /// <summary>
    /// Renders this node and all its children as plain text.
    /// </summary>
    /// <returns>A plain text representation of the node content.</returns>
    string GetPlainText();

    /// <summary>
    /// Renders this node and all its children as HTML.
    /// </summary>
    /// <returns>An HTML representation of the node content.</returns>
    string GetHtml();

    /// <summary>
    /// Renders this node and all its children as Markdown.
    /// </summary>
    /// <returns>A Markdown representation of the node content.</returns>
    string GetMarkdown();
}
