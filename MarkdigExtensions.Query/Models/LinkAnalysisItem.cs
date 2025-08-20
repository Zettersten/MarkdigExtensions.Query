namespace MarkdigExtensions.Query.Models;

/// <summary>
/// Represents an analyzed link in the document.
/// Provides information about link properties and context.
/// </summary>
public class LinkAnalysisItem
{
    /// <summary>Gets or sets the URL of the link.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the title attribute of the link.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the display text of the link.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this is an external link (http/https).</summary>
    public bool IsExternal { get; set; }

    /// <summary>Gets or sets whether this is a relative link.</summary>
    public bool IsRelative =>
        !this.IsExternal && !string.IsNullOrEmpty(this.Url) && !this.Url.StartsWith('/');

    /// <summary>Gets or sets whether this is an anchor link (starts with #).</summary>
    public bool IsAnchor => !string.IsNullOrEmpty(this.Url) && this.Url.StartsWith('#');

    /// <summary>Gets or sets the position of this link in the document.</summary>
    public int[] Position { get; set; } = [];
}
