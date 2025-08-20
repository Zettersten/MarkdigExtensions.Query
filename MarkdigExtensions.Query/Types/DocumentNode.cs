namespace MarkdigExtensions.Query.Types;

public class DocumentNode(
    int[] position,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
) : Node(position, "document", value, attributes, children)
{
    public override string[] Selectors => ["document"];
}
