using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// An access tree generated for a save, and how it differs from the submitted tree.
/// See <see cref="AccessTreeGenerator.GenerateAccessTreeWithReport"/>.
/// </summary>
public sealed class AccessTreeReport
{
    internal AccessTreeReport(string accessTree, List<AccessTreeChange> dropped, List<AccessTreeChange> kept, bool isAsSubmitted)
    {
        this.AccessTree = accessTree;
        this.Dropped = dropped;
        this.Kept = kept;
        this.IsAsSubmitted = isAsSubmitted;
    }

    /// <summary>
    /// The tree to store, as <see cref="AccessTreeGenerator.GenerateAccessTree"/> writes it.
    /// </summary>
    public string AccessTree { get; }

    /// <summary>
    /// What the submitted tree grants that the tree doesn't: what the saver can't grant, worded as
    /// <see cref="AccessTreeComparer.FindInAccessibleActionsOn"/> words it, and what they could grant but can't be saved
    /// next to what is kept ("(not applied: ...)").
    /// </summary>
    public IReadOnlyList<AccessTreeChange> Dropped { get; }

    /// <summary>
    /// What the tree grants that the submitted tree doesn't: stored grants the saver couldn't give back ("Write, Delete",
    /// "You can't lower 90", "You can't remove /"), and those kept with them ("(kept with ...)").
    /// </summary>
    public IReadOnlyList<AccessTreeChange> Kept { get; }

    /// <summary>
    /// Whether the tree grants exactly what was submitted: nothing dropped and nothing kept, counting every access an
    /// action can be given, every row and value, and every group wildcard. Maximum on a read-write-delete action, which
    /// grants nothing, doesn't count, as in <see cref="AccessTreeComparer.FindInAccessibleActionsOn"/>.
    /// </summary>
    public bool IsAsSubmitted { get; }
}

/// <summary>
/// One line of an <see cref="AccessTreeReport"/>: an action, or a group's wildcard, and what of it differs.
/// </summary>
public sealed class AccessTreeChange
{
    internal AccessTreeChange(string name, string path, ActionBase? action, string description)
    {
        this.Name = name;
        this.Path = path;
        this.Action = action;
        this.Description = description;
    }

    /// <summary>
    /// The action's name, or the group's.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The action's path, or the group's.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The action, or null for a group's wildcard.
    /// </summary>
    public ActionBase? Action { get; }

    /// <summary>
    /// What differs: "Write, Delete", "B: Read", "You can't lower 90", "Read (on the whole group)".
    /// </summary>
    public string Description { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{this.Name}: {this.Description}";
}
