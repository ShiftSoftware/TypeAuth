namespace ShiftSoftware.TypeAuth.Core;

public class ActionTreeItem
{
    public string TypeName { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? DisplayDescription { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    public Actions.Action? Action { get; set; }
    public HashSet<ActionTreeItem> ActionTreeItems { get; set; }

    public ActionTreeItem()
    {
        this.ActionTreeItems = new HashSet<ActionTreeItem>();
    }
}
