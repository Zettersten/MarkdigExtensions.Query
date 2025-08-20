namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Helper class for building node attributes dictionaries.
/// </summary>
internal static class AttributeHelper
{
    /// <summary>
    /// Creates a new attributes dictionary or extends an existing one with additional key-value pairs.
    /// </summary>
    /// <param name="existingAttributes">Existing attributes dictionary (can be null)</param>
    /// <param name="additionalAttributes">Additional attributes to add</param>
    /// <returns>Combined attributes dictionary</returns>
    public static Dictionary<string, string?> BuildAttributes(
        Dictionary<string, string?>? existingAttributes,
        params (string Key, string? Value)[] additionalAttributes
    )
    {
        var result =
            existingAttributes != null
                ? new Dictionary<string, string?>(
                    existingAttributes,
                    StringComparer.OrdinalIgnoreCase
                )
                : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in additionalAttributes)
        {
            if (!string.IsNullOrEmpty(key))
            {
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts boolean values to lowercase strings for consistency.
    /// </summary>
    /// <param name="value">Boolean value to convert</param>
    /// <returns>Lowercase string representation</returns>
    public static string ToLowerString(this bool value) => value.ToString().ToLowerInvariant();
}
