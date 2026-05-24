namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Represents a node in the action tree hierarchy. Each node corresponds to either an action tree class or an individual action.
/// </summary>
public class ActionTreeNode
{
    /// <summary>
    /// The identifier for this node (type name for tree nodes, field name for action nodes).
    /// </summary>
    public string ID { get; set; } = default!;

    /// <summary>
    /// Dot-delimited path from the root to this node.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Friendly display name from the <see cref="ActionTree"/> attribute or action definition.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Display description from the <see cref="ActionTree"/> attribute or action definition.
    /// </summary>
    public string? DisplayDescription { get; set; }

    /// <summary>
    /// Extensibility property for attaching arbitrary data to a node.
    /// </summary>
    public dynamic? AdditionalData { get; set; }

    /// <summary>
    /// Access levels inherited from a parent node's wildcard grant.
    /// </summary>
    public List<Access> WildCardAccess { get; set; }

    /// <summary>
    /// The action definition at this node, or null if this is a grouping node.
    /// </summary>
    public Actions.ActionBase? Action { get; set; }

    /// <summary>
    /// Child nodes in the action tree hierarchy.
    /// </summary>
    public List<ActionTreeNode> ActionTreeItems { get; set; }

    /// <summary>
    /// True if this node was dynamically generated from a <see cref="Actions.DynamicAction"/>'s expanded items.
    /// </summary>
    public bool IsADynamicSubItem { get; set; }
    public ActionTreeNode(string? path)
    {
        this.ActionTreeItems = new List<ActionTreeNode>();
        this.WildCardAccess = new List<Access>();
        this.Path = path;
    }
}
