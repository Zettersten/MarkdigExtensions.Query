namespace MarkdigExtensions.Query.Types;

public class ListItemNode(
    int[] position,
    bool isChecked = false,
    bool hasCheckbox = false,
    string? value = null,
    Dictionary<string, string?>? attributes = null,
    List<INode>? children = null
)
    : Node(
        position,
        "item",
        value,
        AttributeHelper.BuildAttributes(
            attributes,
            ("checked", isChecked.ToLowerString()),
            ("has_checkbox", hasCheckbox.ToLowerString())
        ),
        children
    )
{
    public override string[] Selectors =>
        hasCheckbox ? ["item", "li", "task_list_item"] : ["item", "li"];

    public bool IsChecked => isChecked;

    public bool HasCheckbox => hasCheckbox;
}
