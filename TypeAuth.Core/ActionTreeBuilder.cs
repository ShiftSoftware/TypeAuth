using System.Reflection;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class ActionTreeBuilder
    {
        internal ActionTreeNode GenerateActionTree(List<Type> actionTrees, List<string> accessTreeJSONStrings, ActionTreeNode? rootActionTree)
        {
            if (rootActionTree is null)
                rootActionTree = new ActionTreeNode(null) { ID = "Root" };

            foreach (var tree in actionTrees)
            {
                var path = string.IsNullOrWhiteSpace(rootActionTree.Path) ? tree.Name : $"{rootActionTree.Path}.{tree.Name}";

                var treeAttribute = tree.GetCustomAttribute((typeof(ActionTree))) as ActionTree;

                var actionTreeItem = new ActionTreeNode(path) { ID = tree.Name };

                if (treeAttribute != null)
                {
                    actionTreeItem.DisplayName = treeAttribute.Name;
                    actionTreeItem.DisplayDescription = treeAttribute.Description;
                }

                rootActionTree.ActionTreeItems.Add(actionTreeItem);

                var childTress = tree.GetNestedTypes().ToList().Where(x => x.GetCustomAttributes(typeof(ActionTree), false) != null).ToList();

                GenerateActionTree(childTress, accessTreeJSONStrings, actionTreeItem);

                // ActionTreeHelper owns the declaration convention (public static ActionBase
                // fields), so lookups made through it always agree with the built tree.
                foreach (var declared in ActionTreeHelper.GetDeclaredActions(tree))
                {
                    var action = declared.Value;

                    action.Path = $"{actionTreeItem.Path}.{declared.Key}";

                    var thisActionTreeItem = new ActionTreeNode(action.Path)
                    {
                        ID = declared.Key,
                        Action = action,
                        DisplayName = action.Name,
                        DisplayDescription = action.Description
                    };

                    actionTreeItem.ActionTreeItems.Add(thisActionTreeItem);
                }
            }

            return rootActionTree;
        }
    }
}
