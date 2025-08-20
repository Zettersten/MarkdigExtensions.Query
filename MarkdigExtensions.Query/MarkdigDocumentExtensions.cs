using QueryableMarkdownDocument = MarkdigExtensions.Query.Core.MarkdownDocument;

namespace MarkdigExtensions.Query;

/// <summary>
/// Extension methods specifically for Markdig.Syntax.MarkdownDocument.
/// These methods provide the bridge between Markdig's native document format
/// and the queryable document format with jQuery-style API.
/// </summary>
public static class MarkdigDocumentExtensions
{
    /// <summary>
    /// Converts a Markdig MarkdownDocument to a queryable MarkdownDocument.
    /// This is the primary entry point for adding jQuery-style querying to Markdig documents.
    /// </summary>
    /// <param name="markdigDocument">The source Markdig document</param>
    /// <returns>A queryable MarkdownDocument with jQuery-style API</returns>
    public static QueryableMarkdownDocument AsQueryable(
        this Markdig.Syntax.MarkdownDocument markdigDocument
    ) => markdigDocument.ToQueryableDocument();

    /// <summary>
    /// Converts a Markdig MarkdownDocument to a queryable MarkdownDocument.
    /// This is the primary entry point for adding jQuery-style querying to Markdig documents.
    /// </summary>
    /// <param name="markdigText">The source Markdig document</param>
    /// <returns>A queryable MarkdownDocument with jQuery-style API</returns>
    public static QueryableMarkdownDocument AsQueryable(this string markdigText) =>
        markdigText.ToQueryableDocument();
}
