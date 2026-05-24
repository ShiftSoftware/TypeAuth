namespace ShiftSoftware.TypeAuth.Core;

public static class AccessTreeGenerator
{
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
