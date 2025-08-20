namespace MarkdigExtensions.Query.Models;

/// <summary>
/// Represents an item in the document outline, typically a heading.
/// Used for generating table of contents and understanding document structure.
/// </summary>
public class DocumentOutlineItem
{
    /// <summary>Gets or sets the heading level (1-6).</summary>
    public int Level { get; set; }

    /// <summary>Gets or sets the title text of the heading.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the position of this item in the document.</summary>
    public int[] Position { get; set; } = [];

    /// <summary>Gets or sets child outline items (sub-headings).</summary>
    public List<DocumentOutlineItem> Children { get; set; } = [];
}
