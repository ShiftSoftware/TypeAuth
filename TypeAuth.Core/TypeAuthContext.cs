﻿using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// This is what you use to check Action Trees against an Access Tree
    /// </summary>
    public class TypeAuthContext
    {
        private TypeAuthContextHelper TypeAuthContextHelper { get; set; }

        internal const string SelfRererenceKey = "_shift_software_type_auth_core_self_reference";

        internal List<string> AccessTreeJsonStrings { get; set; } = default!;

        internal Type[] ActionTrees { get; set; } = default!;

        public ActionTreeItem ActionTree { get; set; } = default!;

        public TypeAuthContext(string accessTreeJSONString = "{}", params Type[] actionTrees)
        {
            this.TypeAuthContextHelper = new TypeAuthContextHelper();
            this.Init(new List<string> { accessTreeJSONString }, actionTrees);
        }

        /// <summary>
        /// Constructs a Context by Providing a list of Action Trees and an Access Tree provided as a serialized JSON string.
        /// </summary>
        /// <param name="actionTrees">A list of Action Trees to Check your Access Tree against.</param>
        /// <param name="accessTreeJSONString">The Access Tree provided as a JSON string. Access Tree contains the Actions that a Subject can perform.</param>
        public TypeAuthContext(List<string> accessTreeJSONStrings, params Type[] actionTrees)
        {
            this.TypeAuthContextHelper = new TypeAuthContextHelper();
            this.Init(accessTreeJSONStrings, actionTrees);
        }

        internal void Init(List<string> accessTreeJSONStrings, params Type[] actionTrees)
        {
            this.AccessTreeJsonStrings = accessTreeJSONStrings;
            this.ActionTrees = actionTrees;

            this.ActionTree = this.TypeAuthContextHelper.GenerateActionTree(actionTrees.ToList(), accessTreeJSONStrings);

            //Console.WriteLine("Action Trees Are:");
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this.ActionTree, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            //{
            //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //}));

            //Console.WriteLine();

            foreach (var accessTreeJSONString in accessTreeJSONStrings)
            {
                var accessTree = Newtonsoft.Json.JsonConvert.DeserializeObject(accessTreeJSONString);

                //Console.WriteLine(accessTree.ToString());
                this.TypeAuthContextHelper.PopulateActionBank(this.ActionTree, accessTree);
            }

            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this.TypeAuthContextHelper.ActionBank, Newtonsoft.Json.Formatting.Indented));

            this.TypeAuthContextHelper.ExpandDynamicActions(this.ActionTree);
        }

        public bool Can(ActionBase action, Access access)
        {
            return this.TypeAuthContextHelper.Can(action, access);
        }

        public bool Can(ActionBase action, Access access, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, access, Id, selfId);
        }

        public bool Can(Type actionTreeType, string actionName, Access access)
        {
            var instance = (Activator.CreateInstance(actionTreeType))!;

            var action = (Actions.Action) actionTreeType
                .GetFields()
                .FirstOrDefault(x => x.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                ?.GetValue(instance)!;

            if (action == null)
                throw new Exception($"No Such Action ({actionTreeType.FullName}.{actionName})");

            return this.TypeAuthContextHelper.Can(action, access);
        }

        public bool CanRead(ReadAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }
        public bool CanRead(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadWriteAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadWriteDeleteAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }

        public bool CanWrite(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        public bool CanWrite(DynamicReadWriteAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id, selfId);
        }

        public bool CanWrite(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        public bool CanWrite(DynamicReadWriteDeleteAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id, selfId);
        }

        public bool CanDelete(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete);
        }

        public bool CanDelete(DynamicReadWriteDeleteAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete, Id, selfId);
        }

        public bool CanAccess(BooleanAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Maximum);
        }

        public bool CanAccess(DynamicBooleanAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Maximum, Id, selfId);
        }

        public string? AccessValue(TextAction action)
        {
            return GetTextAccessValue(action, action.Comparer, action.Merger, action.MinimumAccess, action.MaximumAccess);
        }

        public decimal? AccessValue(DecimalAction action)
        {
            var textValue = AccessValue(action as TextAction);

            if (textValue != null)
                return decimal.Parse(textValue);

            return null;
        }

        public string? AccessValue(DynamicTextAction action, string? Id, string? selfId = null)
        {
            return GetTextAccessValue(action, action.Comparer, action.Merger, action.MinimumAccess, action.MaximumAccess, Id, selfId);
        }

        public decimal? AccessValue(DynamicDecimalAction action, string? Id, string? selfId = null)
        {
            var textValue = AccessValue(action as DynamicTextAction, Id, selfId);

            if (textValue != null)
                return decimal.Parse(textValue);

            return null;
        }

        private string? GetTextAccessValue(ActionBase action, Func<string?, string?, string?>? comparer, Func<string?, string?, string?>? merger, string? minAccess, string? maxAccess, string? Id = null, string? selfId = null)
        {
            var access = minAccess;

            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(action, Id, selfId);

            for (int i = 0; i < actionMatches.Count; i++)
            {
                string? thisAccess = actionMatches[i].AccessValue;

                if (i > 0)
                {
                    if (comparer != null)
                        thisAccess = comparer(access, thisAccess);

                    if (merger != null)
                        thisAccess = merger(access, thisAccess);
                }

                if (thisAccess != null)
                    access = thisAccess;
            }

            return access;
        }

        public void SetAccessValue(TextAction theAction, string? value, string? maximumValue)
        {
            SetTextAccessValue(theAction, theAction.MinimumAccess, theAction.MaximumAccess, value, maximumValue, theAction.Comparer);
        }

        public void SetAccessValue(DynamicTextAction theAction, string? Id, string? value, string? maximumValue)
        {
            SetTextAccessValue(theAction, theAction.MinimumAccess, theAction.MaximumAccess, value, maximumValue, theAction.Comparer, Id);
        }

        private void SetTextAccessValue(ActionBase theAction, string? actionMin, string? actionMax, string? valueToSet, string? maxAllowed, Func<string?, string?, string?>? comparer, string? Id = null)
        {

            var actionMatches = this.FindOrAddInActionBank(theAction, Id);

            foreach (var action in actionMatches)
            {
                valueToSet = ReduceValue(comparer, actionMin, actionMax, valueToSet, maxAllowed);

                action.AccessValue = valueToSet;
            }
        }

        private string? ReduceValue(Func<string?, string?, string?>? comparer, string? actionMinimum, string? actionMaximum, string? value, string? maximumValue)
        {
            if (comparer != null)
            {
                //Only used to parse the value.
                //For example if the comparer deals with numbers. And a number like 000100 is provided. Comparing the value against it self will return 100
                value = comparer(value, value);

                var assignableMaximumWinner = comparer(maximumValue, actionMaximum);

                if (assignableMaximumWinner == actionMaximum)
                    assignableMaximumWinner = maximumValue;

                var actionMaximumWinner = comparer(value, assignableMaximumWinner);

                if (actionMaximumWinner == value)
                    value = assignableMaximumWinner;

                var minimumWinner = comparer(value, actionMinimum);

                if (minimumWinner == actionMinimum)
                    value = actionMinimum;
            }

            return value;
        }

        public void ToggleAccess(ActionBase theAction, Access access, string? Id = null)
        {
            var actionMatches = this.FindOrAddInActionBank(theAction, Id);

            foreach (var action in actionMatches)
            {
                if (action.AccessList.Contains(access))
                    action.AccessList.Remove(access);
                else
                    action.AccessList.Add(access);
            }
        }

        private List<ActionBankItem> FindOrAddInActionBank(ActionBase theAction, string? Id)
        {
            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(theAction, Id);

            if (actionMatches.Count == 0 && theAction is Actions.Action)
            {
                var actionBankItem = new ActionBankItem(theAction, new List<Access>());

                this.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

                actionMatches.Add(actionBankItem);
            }

            if (theAction is DynamicAction)
            {
                if (actionMatches.Count == 0)
                {
                    var actionBankItem = new ActionBankItem(theAction, new List<Access>());

                    actionBankItem.SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = Id }, new List<Access> { }));

                    this.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

                    actionMatches = this.TypeAuthContextHelper.LocateActionInBank(theAction, Id);
                }


                if (!actionMatches.Any(x => (x.Action as DynamicAction)!.Id == Id))
                {
                    actionMatches.First().SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = Id, Type = theAction.Type }, new List<Access>()));

                    actionMatches = this.TypeAuthContextHelper.LocateActionInBank(theAction, Id);
                }
            }

            return actionMatches;
        }

        public string GenerateAccessTree(TypeAuthContext reducer, TypeAuthContext? preserver = null)
        {
            var reducedActionTreeItems = new List<ActionTreeItem>();

            this.FlattenActionTree(reducedActionTreeItems, reducer.ActionTree);

            List<ActionTreeItem>? preservedActionTreeItems = null;

            if (preserver != null)
            {
                preservedActionTreeItems = new List<ActionTreeItem>();
                this.FlattenActionTree(preservedActionTreeItems, preserver.ActionTree);
            }

            var accessTree = this.TraverseActionTree(this.ActionTree, new Dictionary<string, object>(), reducer, reducedActionTreeItems, false, preserver, preservedActionTreeItems);

            if (accessTree == null)
                accessTree = new Dictionary<string, object>(); //To return an empty json {}

            return Newtonsoft.Json.JsonConvert.SerializeObject(accessTree);
        }

        private void FlattenActionTree(List<ActionTreeItem> flattenedActionTreeItems, ActionTreeItem root)
        {
            foreach (var item in root.ActionTreeItems)
            {
                FlattenActionTree(flattenedActionTreeItems, item);
            }

            if (!root.DynamicSubitem)
                flattenedActionTreeItems.Add(root);
        }

        private object? TraverseActionTree(ActionTreeItem actionTreeItem, Dictionary<string, object> accessTree, TypeAuthContext reducer, List<ActionTreeItem> reducedActionTreeItems, bool stopTraversing = false, TypeAuthContext? preserver = null, List<ActionTreeItem>? preservedActionTreeItems = null)
        {
            ActionTreeItem? preserverActionTreeItem = null;

            if (preservedActionTreeItems != null)
            {
                preserverActionTreeItem = preservedActionTreeItems.FirstOrDefault(
                    x => x.Type == actionTreeItem.Type
                );
            }

            if (actionTreeItem.WildCardAccess.Count > 0 || preserverActionTreeItem?.WildCardAccess?.Count > 0)
            {
                var reducerActionTreeItem = reducedActionTreeItems.FirstOrDefault(
                    x => x.Type == actionTreeItem.Type
                );

                var access = actionTreeItem.WildCardAccess.Where(x => reducerActionTreeItem.WildCardAccess.Contains(x)).ToList();

                if (preserverActionTreeItem != null)
                {
                    foreach (var item in preserverActionTreeItem.WildCardAccess)
                    {
                        if (!reducerActionTreeItem.WildCardAccess.Contains(item) && !access.Contains(item))
                        {
                            access.Add(item);
                        }
                    }
                }

                if (access.Count() > 0)
                    //return null;

                    return access;
            }

            foreach (var subActionTreeItem in actionTreeItem.ActionTreeItems)
            {
                var value = this.TraverseActionTree(subActionTreeItem, new Dictionary<string, object>(), reducer, reducedActionTreeItems, stopTraversing, preserver, preservedActionTreeItems);

                if (value != null)
                    accessTree[subActionTreeItem.TypeName] = value;
            }

            if (actionTreeItem.Action != null)
            {
                var dynamicAction = actionTreeItem.Action as DynamicAction;

                if (dynamicAction != null && !stopTraversing)
                {
                    var actionBankItems = this.TypeAuthContextHelper.LocateActionInBank(dynamicAction);

                    var subItems = actionBankItems.SelectMany(x => x.SubActionBankItems.ToList()).ToList();

                    if (preserver != null)
                    {
                        var preserverActionBankItems = preserver.TypeAuthContextHelper.LocateActionInBank(dynamicAction);

                        subItems.AddRange(preserverActionBankItems.SelectMany(x => x.SubActionBankItems.ToList()));

                        subItems = subItems.Distinct().ToList();
                    }

                    if (subItems.Count() > 0)
                    {
                        var dynamicItems = new Dictionary<string, object>();

                        accessTree[actionTreeItem.TypeName] = dynamicItems;

                        foreach (var item in subItems)
                        {
                            var subActionTreeItem = new ActionTreeItem
                            {
                                Action = actionTreeItem.Action,
                                TypeName = (item.Action as DynamicAction)!.Id!,
                                Type = null
                            };

                            var value = this.TraverseActionTree(subActionTreeItem, dynamicItems, reducer, reducedActionTreeItems, true, preserver, preservedActionTreeItems);

                            if (value != null)
                                dynamicItems![subActionTreeItem.TypeName] = value;
                        }

                        if (dynamicItems.Count == 0)
                            return null;

                        return dynamicItems;
                    }
                }

                if (actionTreeItem.Action.Type == ActionType.Text)
                {
                    var textAction = actionTreeItem.Action as TextAction;
                    var dynamicTextAction = actionTreeItem.Action as DynamicTextAction;

                    string? value = null;

                    if (dynamicTextAction != null)
                        value = this.AccessValue(dynamicTextAction, actionTreeItem.TypeName);
                    else if (textAction != null)
                        value = this.AccessValue(textAction);

                    string? reducedValue = null;

                    if (dynamicTextAction != null)
                        reducedValue = reducer.AccessValue(dynamicTextAction, actionTreeItem.TypeName);
                    else if (textAction != null)
                        reducedValue = reducer.AccessValue(textAction);

                    if (dynamicTextAction != null)
                        value = ReduceValue(dynamicTextAction.Comparer, dynamicTextAction.MinimumAccess, dynamicTextAction.MaximumAccess, value, reducedValue);
                    else if (textAction != null)
                        value = ReduceValue(textAction.Comparer, textAction.MinimumAccess, textAction.MaximumAccess, value, reducedValue);


                    if (value == null || value == textAction?.MinimumAccess || value == dynamicTextAction?.MinimumAccess)
                        return null;

                    if (textAction is DecimalAction || dynamicTextAction is DynamicDecimalAction)
                        return decimal.Parse(value);

                    return value;
                }

                else
                {
                    var accesses = new List<Access>();

                    foreach (var access in Enum.GetValues(typeof(Access)).Cast<Access>())
                    {
                        if (dynamicAction != null)
                        {
                            if (this.Can(dynamicAction, access, actionTreeItem.TypeName) && reducer.Can(dynamicAction, access, actionTreeItem.TypeName))
                                accesses.Add(access);

                            if (preserver != null)
                            {
                                //If the access is reduced.
                                if (!reducer.Can(dynamicAction, access, actionTreeItem.TypeName))
                                {
                                    //If the access should've been preserved.
                                    if (preserver.Can(dynamicAction, access, actionTreeItem.TypeName) && !accesses.Contains(access))
                                    {
                                        accesses.Add(access);
                                    }
                                }
                            }
                        }
                        else if (actionTreeItem.Action is Actions.Action)
                        {
                            var action = (actionTreeItem.Action as Actions.Action)!;

                            if (this.TypeAuthContextHelper.Can(action, access) && reducer.TypeAuthContextHelper.Can(action, access))
                                accesses.Add(access);

                            if (preserver != null)
                            {
                                //If the access is reduced.
                                if (!reducer.Can(action, access))
                                {
                                    //If the access should've been preserved.
                                    if (preserver.Can(action, access) && !accesses.Contains(access))
                                    {
                                        accesses.Add(access);
                                    }
                                }
                            }
                        }
                    }

                    if (accesses.Count > 0)
                        return accesses;
                    else
                        return null;
                }
            }

            if (accessTree.Count() == 0)
                return null;

            return accessTree;
        }

        public Type[] GetRegisteredActionTrees()
        {
            return this.ActionTrees.ToArray();
        }

        public Dictionary<ActionBase, string> FindInAccessibleActionsOn(TypeAuthContext typeAuthContextToCompare)
        {
            var inAccessibleActions = new Dictionary<ActionBase, string>();

            foreach (var actionBankItem in typeAuthContextToCompare.TypeAuthContextHelper.ActionBank)
            {
                var action = actionBankItem.Action;

                if (action is not null)
                {
                    var inAccessibleForThisAction = new List<object>();

                    foreach (var access in actionBankItem.AccessList)
                    {
                        if (!this.Can(action, access))
                        {
                            if (action is BooleanAction && access != Access.Maximum)
                                continue;

                            if (action is ReadAction && access != Access.Read)
                                continue;

                            if (action is ReadWriteAction && !(access == Access.Read || access == Access.Write))
                                continue;

                            if (action is ReadWriteDeleteAction && !(access == Access.Read || access == Access.Write || access == Access.Delete))
                                continue;

                            inAccessibleForThisAction.Add(access);
                        }
                    }

                    if (inAccessibleForThisAction.Count > 0)
                        inAccessibleActions[action] = string.Join(", ", inAccessibleForThisAction);

                    if (action is DecimalAction decimalAction)
                    {
                        var targetValue = typeAuthContextToCompare.AccessValue(decimalAction);
                        var allowedValue = this.AccessValue(decimalAction);


                        if (targetValue > allowedValue)
                        {
                            inAccessibleActions[action] = $"Maximum allowed value is {allowedValue}. You can't grant {targetValue}";
                        }
                    }
                }
            }

            return inAccessibleActions;
        }

        public (bool WildCard, List<string> AccessibleIds) GetAccessibleItems(DynamicAction dynamicAction, Func<Access, bool> predicate, string? selfId = null)
        {
            var locatedActions = this.TypeAuthContextHelper.LocateActionInBank(dynamicAction);

            var wildCardAccess = false;

            foreach (var item in locatedActions)
            {
                if (item.AccessList.Any(predicate))
                {
                    wildCardAccess = true;
                    break;
                }
            }

            List<string> ids = new List<string>();

            if (!wildCardAccess)
            {
                foreach (var item in locatedActions)
                {
                    foreach (var subItem in item.SubActionBankItems)
                    {
                        if (subItem.AccessList.Any(predicate))
                            ids!.Add((subItem.Action as DynamicAction)!.Id!);
                    }
                }
            }

            if (selfId is not null)
                ids = ids.Select(x => x.Equals(SelfRererenceKey) ? selfId : x).ToList();

            return (wildCardAccess, ids);
        }
    }
}
