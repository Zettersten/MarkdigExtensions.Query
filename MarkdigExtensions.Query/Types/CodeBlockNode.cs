namespace MarkdigExtensions.Query.Types;

public class CodeBlockNode(
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
    public override string[] Selectors => ["code_block", "code", "pre"];

    public string? FenceInfo => fenceInfo;

    public string? Language => fenceInfo?.Split(' ').FirstOrDefault();
}
