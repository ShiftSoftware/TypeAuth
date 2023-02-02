using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Diagnostics;

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
        }

        public static string ReplaceSelfReferenceVariable(string id, string actualValue)
        {
            return id == SelfRererenceKey ? actualValue : id;
        }

        //public List<Access> ActionAccessTypes(Action action)
        //{
        //    var accessTypes = new List<Access>();

        //    var actionMatch = this.TypeAuthContextHelper.LocateActionInBank(action);

        //    if (actionMatch != null)
        //        accessTypes = actionMatch.AccessList;

        //    return accessTypes;
        //}

        public bool Can(Actions.Action action, Access access)
        {
            return this.TypeAuthContextHelper.Can(action, access);
        }

        public bool Can(Actions.DynamicAction action, Access access, string Id)
        {
            return this.TypeAuthContextHelper.Can(action, access, Id);
        }

        internal bool Can(Type actionTreeType, string actionName, Access access)
        {
            var instance = Activator.CreateInstance(actionTreeType);

            var action = (Actions.Action) actionTreeType
                .GetFields()
                .FirstOrDefault(x => x.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                ?.GetValue(instance);

            if (action == null)
                throw new Exception($"No Such Action ({actionTreeType.FullName}.{actionName})");

            return this.TypeAuthContextHelper.Can(action, access);
        }

        #region Read
        private bool CanRead(Actions.Action action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        private bool CanRead(Actions.DynamicAction action, string Id, string? selfId = null)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }
        public bool CanRead(ReadAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        public bool CanRead(DynamicReadAction action, string Id, string? selfId = null)
        {
            return this.CanRead((Actions.DynamicAction)action, Id, selfId);
        }
        public bool CanRead(ReadWriteAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        public bool CanRead(DynamicReadWriteAction action, string Id, string? selfId = null)
        {
            return this.CanRead((Actions.DynamicAction)action, Id, selfId);
        }
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        public bool CanRead(DynamicReadWriteDeleteAction action, string Id, string? selfId = null)
        {
            return this.CanRead((Actions.DynamicAction)action, Id, selfId);
        }
        #endregion

        //#region Dynamic

        //public bool CanAccess(DynamicAction<BooleanAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Maximum, key, selfReference);
        //}

        //public bool CanRead(DynamicAction<ReadAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        //}

        //public bool CanRead(DynamicAction<ReadWriteAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        //}

        //public bool CanWrite(DynamicAction<ReadWriteAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Write, key, selfReference);
        //}

        //public bool CanRead(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        //}

        //public bool CanWrite(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Write, key, selfReference);
        //}

        //public bool CanDelete(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        //{
        //    return this.Can(dynamicActionDictionary, Access.Delete, key, selfReference);
        //}

        //public string? AccessValue(DynamicAction<TextAction> dynamicAction, string key, string? selfReference = null)
        //{
        //    string? actionAccess = null;

        //    TextAction? action = null;

        //    if (dynamicAction.UnderlyingAction != null)
        //        action = (TextAction)dynamicAction.UnderlyingAction;

        //    if (dynamicAction.Dictionary.Keys.Contains(key))
        //        action = dynamicAction[key];

        //    if(action != null)
        //        actionAccess = AccessValue(action);

        //    if (selfReference == key && dynamicAction.Dictionary.Keys.Contains(SelfRererenceKey))
        //    {
        //        var selfActionAccess = AccessValue(dynamicAction[SelfRererenceKey]);

        //        if (dynamicAction.Comparer != null)
        //        {
        //            actionAccess = dynamicAction.Comparer(actionAccess, selfActionAccess);
        //        }
        //    }

        //    return actionAccess;
        //}

        //public void SetAccessValue(DynamicAction<TextAction> dynamicAction, string key, string? value, string? maximumValue)
        //{
        //    TextAction? action = null;

        //    if (dynamicAction.UnderlyingAction != null)
        //        action = (TextAction)dynamicAction.UnderlyingAction;

        //    if (dynamicAction.Dictionary.Keys.Contains(key))
        //        action = dynamicAction[key];

        //    var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(action!);

        //    if (actionMatches.Count == 0)
        //    {
        //        var actionBankItem = new ActionBankItem(action!, new List<Access>());

        //        this.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

        //        actionMatches.Add(actionBankItem);
        //    }

        //    foreach (var item in actionMatches)
        //    {
        //        value = ReduceValue((item.Action as TextAction)!, value, maximumValue);

        //        item.AccessValue = value;
        //    }
        //}

        //private bool Can(DynamicActionBase dynamicAction, Access access, string key, string? selfReference = null)
        //{
        //    Actions.Action? action = null;

        //    var actionAccess = false;

        //    var dynamicActionDictionary = dynamicAction.Dictionary;

        //    if (dynamicAction.UnderlyingAction != null)
        //        action = dynamicAction.UnderlyingAction;

        //    if (dynamicActionDictionary.Keys.Contains(key))
        //        action = dynamicActionDictionary[key];

        //    if (action != null)
        //        actionAccess = this.TypeAuthContextHelper.Can(action, access);

        //    if (!actionAccess && selfReference == key && dynamicActionDictionary.Keys.Contains(SelfRererenceKey))
        //    {
        //        actionAccess = this.TypeAuthContextHelper.Can(dynamicActionDictionary[SelfRererenceKey], access);
        //    }

        //    return actionAccess;
        //}

        //#endregion

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
            var access = action.MinimumAccess;

            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(action);

            for (int i = 0; i < actionMatches.Count; i++)
            {
                string? thisAccess = actionMatches[i].AccessValue;

                if (i > 0)
                {
                    if (action.Comparer != null)
                        thisAccess = action.Comparer(access, thisAccess);

                    if (action.Merger != null)
                        thisAccess = action.Merger(access, thisAccess);
                }

                if (thisAccess != null)
                    access = thisAccess;
            }

            return access;
        }

        public string? AccessValue(DynamicTextAction action, string? Id, string? selfId = null)
        {
            var access = action.MinimumAccess;

            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(action, Id, selfId);

            for (int i = 0; i < actionMatches.Count; i++)
            {
                string? thisAccess = actionMatches[i].AccessValue;

                if (i > 0)
                {
                    if (action.Comparer != null)
                        thisAccess = action.Comparer(access, thisAccess);

                    if (action.Merger != null)
                        thisAccess = action.Merger(access, thisAccess);
                }

                if (thisAccess != null)
                    access = thisAccess;
            }

            return access;
        }

        public void SetAccessValue(TextAction theAction, string? value, string? maximumValue)
        {
            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(theAction);

            if (actionMatches.Count == 0)
            {
                var actionBankItem = new ActionBankItem(theAction, new List<Access>());

                this.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

                actionMatches.Add(actionBankItem);
            }

            foreach (var action in actionMatches)
            {
                value = ReduceValue(theAction.Comparer, theAction.MinimumAccess, theAction.MaximumAccess, value, maximumValue);

                action.AccessValue = value;
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

        public void ToggleAccess(Actions.Action theAction, Access access)
        {
            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(theAction);

            if (actionMatches.Count == 0)
            {
                var actionBankItem = new ActionBankItem(theAction, new List<Access>());

                this.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

                actionMatches.Add(actionBankItem);
            }

            foreach (var action in actionMatches)
            {
                if (action.AccessList.Contains(access))
                    action.AccessList.Remove(access);
                else
                    action.AccessList.Add(access);
            }
        }

        public string GenerateAccessTree(TypeAuthContext reducer)
        {
            var flattenedActionTreeItems = new List<ActionTreeItem>();

            this.FlattenActionTree(flattenedActionTreeItems, reducer.ActionTree);

            var accessTree = this.TraverseActionTree(this.ActionTree, new JObject(), reducer, flattenedActionTreeItems);

            if (accessTree == null)
                accessTree = new JObject(); //To return an empty json {}

            return Newtonsoft.Json.JsonConvert.SerializeObject(accessTree);
        }

        private void FlattenActionTree(List<ActionTreeItem> flattenedActionTreeItems, ActionTreeItem root)
        {
            foreach (var item in root.ActionTreeItems)
            {
                FlattenActionTree(flattenedActionTreeItems, item);
            }

            flattenedActionTreeItems.Add(root);
        }

        private JToken? TraverseActionTree(ActionTreeItem actionTreeItem, JToken accessTree, TypeAuthContext reducer, List<ActionTreeItem> reducedActionTreeItems, bool stopTraversing = false)
        {
            if (actionTreeItem.WildCardAccess.Count > 0)
            {
                var reducerActionTreeItem = reducedActionTreeItems.FirstOrDefault(
                    x => x.Type == actionTreeItem.Type
                );

                var access = actionTreeItem.WildCardAccess.Where(x => reducerActionTreeItem.WildCardAccess.Contains(x));

                if (access.Count() == 0)
                    return null;

                return new JArray(access);
            }

            foreach (var subActionTreeItem in actionTreeItem.ActionTreeItems)
            {
                var value = this.TraverseActionTree(subActionTreeItem, new JObject(), reducer, reducedActionTreeItems);

                if (value != null)
                    accessTree[subActionTreeItem.TypeName] = value;
            }

            if (actionTreeItem.Action != null)
            {
                var dynamicAction = actionTreeItem.Action as DynamicAction;

                if (dynamicAction != null && !stopTraversing)
                {
                    var actionBanks = TypeAuthContextHelper.LocateActionInBank(dynamicAction);

                    var subItems = actionBanks.SelectMany(x => x.SubActionBankItems.ToList());

                    if (subItems.Count() > 0)
                    {
                        //var a = new HashSet<ActionTreeItem>();

                        accessTree[actionTreeItem.TypeName] = new JObject();

                        foreach (var item in subItems)
                        {
                            var subActionTreeItem = new ActionTreeItem
                            {
                                Action = actionTreeItem.Action,
                                TypeName = (item.Action as DynamicAction)!.Id!,
                                Type = null
                            };

                            var value = this.TraverseActionTree(subActionTreeItem, accessTree[actionTreeItem.TypeName]!, reducer, reducedActionTreeItems, true);

                            if (value != null)
                                accessTree[actionTreeItem.TypeName]![subActionTreeItem.TypeName] = value;
                        }

                        return accessTree[actionTreeItem.TypeName];
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

                    return new JValue(value);
                }

                else
                {
                    var accesses = new List<Access>();

                    foreach (var access in Enum.GetValues(typeof(Access)).Cast<Access>())
                    {
                        //if (actionTreeItem.DynamicAction != null)
                        //{
                        //    if (this.Can(actionTreeItem.DynamicAction, access, actionTreeItem.TypeName) && reducer.Can(actionTreeItem.DynamicAction!, access, actionTreeItem.TypeName))
                        //        accesses.Add(access);
                        //}
                        //else
                        //{
                        //    if (this.Can(actionTreeItem.Action, access) && reducer.Can(actionTreeItem.Action, access))
                        //        accesses.Add(access);
                        //}

                        if (dynamicAction != null)
                        {
                            if (this.Can(dynamicAction, access, actionTreeItem.TypeName) && reducer.Can(dynamicAction, access, actionTreeItem.TypeName))
                                accesses.Add(access);
                        }
                        else if (actionTreeItem.Action.GetType().BaseType == typeof(Actions.Action))
                        {
                            if (this.Can((actionTreeItem.Action as Actions.Action)!, access) && reducer.Can((actionTreeItem.Action as Actions.Action)!, access))
                                accesses.Add(access);
                        }
                    }

                    if (accesses.Count > 0)
                        return new JArray(accesses);
                    else
                        return null;
                }
            }

            if (accessTree.Count() == 0)
                return null;

            return accessTree;
        }
    }
}
