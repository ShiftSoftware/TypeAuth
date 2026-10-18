namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Extension methods for serializing a <see cref="TypeAuthContext"/>'s action bank back into an access tree JSON string.
/// </summary>
public static class AccessTreeGenerator
{
    /// <summary>
    /// Generates an access tree JSON string by intersecting this context's permissions with a reducer, optionally preserving permissions from a third context.
    /// </summary>
    /// <remarks>
    /// The output does not depend on the order the access trees were added in, as long as each text action's
    /// <see cref="Actions.TextCombine"/> doesn't (see <see cref="Actions.TextCombineAssert"/>). Text values are reduced, and
    /// preserved, per value and per row with that rule.
    /// Passing the context as its own reducer, with no preserver, writes out what the context grants: where the trees
    /// grant more beneath a wildcard than the wildcard does, what is beneath it is written instead. Rows that go beyond
    /// a dynamic action's wildcard cannot be written next to it and are left out.
    /// </remarks>
    /// <param name="typeAuthContext">The source context whose permissions are being serialized.</param>
    /// <param name="reducer">Context that constrains the output — only permissions present in both the source and reducer are included.</param>
    /// <param name="preserver">Optional context whose permissions are preserved even if the reducer would remove them.</param>
    public static string GenerateAccessTree(this TypeAuthContext typeAuthContext, TypeAuthContext reducer, TypeAuthContext? preserver = null)
    {
        var reducedActionTreeItems = new List<ActionTreeNode>();

        AccessTreeTraverser.FlattenActionTree(reducedActionTreeItems, reducer.ActionTree);

        List<ActionTreeNode>? preservedActionTreeItems = null;

        if (preserver != null)
        {
            preservedActionTreeItems = new List<ActionTreeNode>();
            AccessTreeTraverser.FlattenActionTree(preservedActionTreeItems, preserver.ActionTree);
        }

        var accessTree = AccessTreeTraverser.TraverseActionTree(typeAuthContext, typeAuthContext.ActionTree, new Dictionary<string, object>(), reducer, reducedActionTreeItems, false, preserver, preservedActionTreeItems);

        if (accessTree == null)
            accessTree = new Dictionary<string, object>();

        return Newtonsoft.Json.JsonConvert.SerializeObject(accessTree);
    }

    /// <summary>
    /// Generates the access tree as <see cref="GenerateAccessTree"/> does, and reports how it differs from what this context
    /// grants: what it leaves out because the reducer can't grant it, and what it keeps from the preserver because the
    /// reducer couldn't give it back.
    /// </summary>
    /// <remarks>
    /// A host that saves a tree only as it was submitted refuses the save unless <see cref="AccessTreeReport.IsAsSubmitted"/>.
    /// For that, the submitted tree must hold what the saver leaves untouched, beyond their reach too: a client that reduces
    /// the tree against the saver before submitting it leaves those out, and they are reported as kept.
    /// </remarks>
    /// <param name="typeAuthContext">The submitted tree.</param>
    /// <param name="reducer">The saver.</param>
    /// <param name="preserver">The stored tree, on an update.</param>
    public static AccessTreeReport GenerateAccessTreeWithReport(this TypeAuthContext typeAuthContext, TypeAuthContext reducer, TypeAuthContext? preserver = null)
    {
        const string NotApplied = " (not applied: it can't be saved next to what you can't remove)";
        const string KeptWith = " (kept with what you can't remove)";
        const string WholeGroup = " (on the whole group)";

        var accessTree = typeAuthContext.GenerateAccessTree(reducer, preserver);
        var generated = new TypeAuthContext(accessTree, typeAuthContext.ActionTrees);

        //Submitted and not written: what the reducer can't grant, and what it could grant but can't be written next to
        //what is kept.
        var grantable = new TypeAuthContext(typeAuthContext.GenerateAccessTree(reducer), typeAuthContext.ActionTrees);
        var dropped = AccessTreeComparer.Changes(reducer, typeAuthContext, generated, grantable, AccessTreeComparer.Wording.Grant);
        dropped.AddRange(AccessTreeComparer.Changes(reducer, typeAuthContext, generated, null, AccessTreeComparer.Wording.Plain, NotApplied, grantable));

        //Written and not submitted: what the reducer couldn't give back, and what it could but is kept with it.
        var kept = new List<AccessTreeChange>();

        if (preserver != null)
        {
            var givableBack = new TypeAuthContext(generated.GenerateAccessTree(reducer), typeAuthContext.ActionTrees);
            kept.AddRange(AccessTreeComparer.Changes(reducer, generated, typeAuthContext, givableBack, AccessTreeComparer.Wording.Remove));
            kept.AddRange(AccessTreeComparer.Changes(reducer, generated, typeAuthContext, null, AccessTreeComparer.Wording.Plain, KeptWith, givableBack));
        }

        //A group's wildcard that no action beneath it shows, such as Maximum on a group of read-write-delete actions.
        dropped.AddRange(AccessTreeComparer.GroupChanges(reducer, typeAuthContext, generated, dropped, WholeGroup, NotApplied));
        kept.AddRange(AccessTreeComparer.GroupChanges(reducer, generated, typeAuthContext, kept, WholeGroup, KeptWith));

        //Exact when nothing is dropped or kept: every access an action can be given, every row and value, and every group
        //wildcard. Maximum on a read-write-delete action, which grants nothing, doesn't count, as in the assignment check.
        return new AccessTreeReport(accessTree, dropped, kept, dropped.Count == 0 && kept.Count == 0);
    }
}
