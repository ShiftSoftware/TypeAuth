namespace ShiftSoftware.TypeAuth.Core;

public class ActionTreeNode
{
    public string ID { get; set; } = default!;
    public string? Path { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayDescription { get; set; }
    public dynamic? AdditionalData { get; set; }
    public List<Access> WildCardAccess { get; set; }
    public Actions.ActionBase? Action { get; set; }
    //public DynamicActionBase? DynamicAction { get; set; }
    public HashSet<ActionTreeNode> ActionTreeItems { get; set; }

    public bool DynamicSubitem { get; set; }
    public ActionTreeNode(string? path)
    {
        this.ActionTreeItems = new HashSet<ActionTreeNode>();
        this.WildCardAccess = new List<Access>();
        this.Path = path;
    }
}
