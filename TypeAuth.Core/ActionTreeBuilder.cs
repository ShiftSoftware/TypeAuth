using ShiftSoftware.TypeAuth.Core.Actions;

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

                tree.GetFields(BindingFlags.Public | BindingFlags.Static).ToList().ForEach(y =>
                {
                    var value = y.GetValue(y);

                    if (value != null && (value as ActionBase) != null)
                    {
                        var action = (ActionBase)value;

                        action.Path = $"{actionTreeItem.Path}.{y.Name}";

                        var thisActionTreeItem = new ActionTreeNode(action.Path)
                        {
                            ID = y.Name,
                            Action = action,
                            DisplayName = action.Name,
                            DisplayDescription = action.Description
                        };

                        actionTreeItem.ActionTreeItems.Add(thisActionTreeItem);
                    }
                });
            }

            return rootActionTree;
        }
    }
}
