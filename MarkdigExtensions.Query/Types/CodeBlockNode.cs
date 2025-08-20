namespace MarkdigExtensions.Query.Types;

/// <summary>
/// Represents a code block node in the markdown document.
/// Code blocks can be fenced (```code```) or indented, and may include language information.
/// </summary>
public sealed class CodeBlockNode(
    int[] position,
    string? fenceInfo = null,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "code_block",
        value,
        AttributeHelper.BuildAttributes(attributes, ("fence_info", fenceInfo)),
        children
    )
{
    /// <summary>
    /// Gets the CSS selectors that can be used to match this code block node.
    /// </summary>
    public override string[] Selectors => ["code_block", "code", "pre"];

    /// <summary>
    /// Gets the fence info string (the text after the opening fence in fenced code blocks).
    /// This typically contains the language identifier and optional additional parameters.
    /// </summary>
    public string? FenceInfo => fenceInfo;

    /// <summary>
    /// Gets the programming language identifier extracted from the fence info.
    /// </summary>
    public string? Language => fenceInfo?.Split(' ').FirstOrDefault();
}
