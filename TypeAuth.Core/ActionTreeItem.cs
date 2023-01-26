namespace ShiftSoftware.TypeAuth.Core;

public class ActionTreeItem
{
    public string TypeName { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? DisplayDescription { get; set; }
    public dynamic? AdditionalData { get; set; }
    public List<Access> WildCardAccess { get; set; }
    public Actions.Action? Action { get; set; }
    public DynamicActionBase? DynamicAction { get; set; }
    public Type? Type { get; set; }
    public HashSet<ActionTreeItem> ActionTreeItems { get; set; }

    public ActionTreeItem()
    {
        this.ActionTreeItems = new HashSet<ActionTreeItem>();
        this.WildCardAccess = new List<Access>();
    }
}
