namespace ShiftSoftware.TypeAuth.Core;

public class ActionTreeItem
{
    public string ID { get; set; } = default!;
    public string? Path { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayDescription { get; set; }
    public dynamic? AdditionalData { get; set; }
    public List<Access> WildCardAccess { get; set; }
    public Actions.ActionBase? Action { get; set; }
    //public DynamicActionBase? DynamicAction { get; set; }
    public HashSet<ActionTreeItem> ActionTreeItems { get; set; }

    public bool DynamicSubitem { get; set; }
    public ActionTreeItem(string? path)
    {
        this.ActionTreeItems = new HashSet<ActionTreeItem>();
        this.WildCardAccess = new List<Access>();
        this.Path = path;
    }
}
