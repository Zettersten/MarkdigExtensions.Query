namespace MarkdigExtensions.Query.Types;

public class CodeNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "code", value, attributes, children)
{
    public override string[] Selectors => ["code", "code_span"];
}
