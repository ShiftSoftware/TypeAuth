namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Extension methods for serializing a <see cref="TypeAuthContext"/>'s action bank back into an access tree JSON string.
/// </summary>
public static class AccessTreeGenerator
{
    /// <summary>
    /// Generates an access tree JSON string by intersecting this context's permissions with a reducer, optionally preserving permissions from a third context.
    /// </summary>
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
}
