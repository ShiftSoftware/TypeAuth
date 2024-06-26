﻿using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// This is what you use to check Action Trees against an Access Tree
    /// </summary>
    public class TypeAuthContext
    {
        internal TypeAuthContextHelper TypeAuthContextHelper { get; set; }

        internal const string SelfRererenceKey = "_shift_software_type_auth_core_self_reference";
        internal const string EmptyOrNullKey = "_shift_software_type_auth_core_empty_or_null";

        internal List<string> AccessTreeJsonStrings { get; set; } = default!;

        internal Type[] ActionTrees { get; set; } = default!;

        public ActionTreeNode ActionTree { get; set; } = default!;

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

            this.ActionTree = this.TypeAuthContextHelper.GenerateActionTree(actionTrees.ToList(), accessTreeJSONStrings, null);

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

        public bool Can(ActionBase action, Access access, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, access, Id, selfId);
        }

        public bool CanRead(ReadAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }
        public bool CanRead(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadWriteAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        public bool CanRead(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id, selfId);
        }

        public bool CanWrite(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        public bool CanWrite(DynamicReadWriteAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id, selfId);
        }

        public bool CanWrite(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        public bool CanWrite(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id, selfId);
        }

        public bool CanDelete(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete);
        }

        public bool CanDelete(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete, Id, selfId);
        }

        public bool CanAccess(BooleanAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Maximum);
        }

        public bool CanAccess(DynamicBooleanAction action, string Id, params string[]? selfId)
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

        public string? AccessValue(DynamicTextAction action, string? Id, params string[]? selfId)
        {
            return GetTextAccessValue(action, action.Comparer, action.Merger, action.MinimumAccess, action.MaximumAccess, Id, selfId);
        }

        public decimal? AccessValue(DynamicDecimalAction action, string? Id, params string[]? selfId)
        {
            var textValue = AccessValue(action as DynamicTextAction, Id, selfId);

            if (textValue != null)
                return decimal.Parse(textValue);

            return null;
        }

        private string? GetTextAccessValue(ActionBase action, Func<string?, string?, string?>? comparer, Func<string?, string?, string?>? merger, string? minAccess, string? maxAccess, string? Id = null, params string[]? selfId)
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

        public Type[] GetRegisteredActionTrees()
        {
            return this.ActionTrees.ToArray();
        }

        public (bool WildCard, List<string> AccessibleIds) GetAccessibleItems(DynamicAction dynamicAction, Func<Access, bool> predicate, params string[]? selfId )
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

            //if (selfId is not null)
            //    ids = ids.Select(x => x.Equals(SelfRererenceKey) ? selfId : x).ToList();

            if (selfId is not null)
            {
                if (ids.Contains(SelfRererenceKey))
                {
                    ids.Remove(SelfRererenceKey);

                    ids.InsertRange(0, selfId);
                }

                //ids = ids.Select(x => x.Equals(SelfRererenceKey) ? selfId : x).ToList();
            }

            return (wildCardAccess, ids);
        }
    }
}
