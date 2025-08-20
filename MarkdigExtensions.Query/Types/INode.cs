using Markdig.Syntax;

namespace MarkdigExtensions.Query.Types;

public interface INode
{
    int[] Position { get; }

    bool HasChildren { get; }

    IReadOnlyList<INode> Children { get; }

    string Name { get; }

    string? Value { get; }

    IReadOnlyDictionary<string, string?> Attributes { get; }
    
    /// <summary>Gets the original Markdig object reference (Block or Inline).</summary>
    IMarkdownObject? MarkdigRef { get; }
    
    /// <summary>Renders this node and all its children as plain text.</summary>
    string GetPlainText();
    
    /// <summary>Renders this node and all its children as HTML.</summary>
    string GetHtml();
    
    /// <summary>Renders this node and all its children as Markdown.</summary>
    string GetMarkdown();
}
