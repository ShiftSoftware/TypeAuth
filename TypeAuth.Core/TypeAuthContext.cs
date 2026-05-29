using System.Globalization;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// This is what you use to check Action Trees against an Access Tree.
    /// An un-initialized context (before <see cref="Init"/> is called) is fail-closed: all permission checks return false / deny.
    /// </summary>
    public class TypeAuthContext : ITypeAuthService
    {
        internal TypeAuthContextHelper TypeAuthContextHelper { get; set; } = new TypeAuthContextHelper();

        public const string SelfReferenceKey = "_shift_software_type_auth_core_self_reference";
        public const string EmptyOrNullKey = "_shift_software_type_auth_core_empty_or_null";

        internal List<string> AccessTreeJsonStrings { get; set; } = default!;

        internal Type[] ActionTrees { get; set; } = default!;

        /// <summary>
        /// The hierarchical action tree built from the registered action tree types.
        /// </summary>
        public ActionTreeNode ActionTree { get; set; } = default!;

        /// <summary>
        /// Constructs a context from a single access tree JSON string and one or more action tree types.
        /// </summary>
        public TypeAuthContext(string accessTreeJSONString = "{}", params Type[] actionTrees)
        {
            this.Init(new List<string> { accessTreeJSONString }, actionTrees);
        }

        /// <summary>
        /// Constructs a context from multiple access tree JSON strings (merged together) and one or more action tree types.
        /// </summary>
        /// <param name="accessTreeJSONStrings">Access trees provided as JSON strings. Multiple trees are merged to form the combined permissions.</param>
        /// <param name="actionTrees">Action tree types that define the available actions to check against.</param>
        public TypeAuthContext(List<string> accessTreeJSONStrings, params Type[] actionTrees)
        {
            this.Init(accessTreeJSONStrings, actionTrees);
        }

        internal void Init(List<string> accessTreeJSONStrings, params Type[] actionTrees)
        {
            this.TypeAuthContextHelper = new TypeAuthContextHelper();
            this.AccessTreeJsonStrings = accessTreeJSONStrings;
            this.ActionTrees = actionTrees;

            this.ActionTree = this.TypeAuthContextHelper.GenerateActionTree(actionTrees.ToList(), accessTreeJSONStrings, null);

            foreach (var accessTreeJSONString in accessTreeJSONStrings)
            {
                var accessTree = Newtonsoft.Json.JsonConvert.DeserializeObject(accessTreeJSONString);

                this.TypeAuthContextHelper.PopulateActionBank(this.ActionTree, accessTree);
            }

            this.TypeAuthContextHelper.ExpandDynamicActions(this.ActionTree);
        }

        /// <inheritdoc cref="ITypeAuthService.Can(ActionBase, Access)"/>
        public bool Can(ActionBase action, Access access)
        {
            return this.TypeAuthContextHelper.Can(action, access);
        }

        /// <inheritdoc cref="ITypeAuthService.Can(ActionBase, Access, string, string[])"/>
        public bool Can(ActionBase action, Access access, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, access, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(ReadAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(DynamicReadAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id ?? EmptyOrNullKey, selfId);
        }
        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(DynamicReadWriteAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id ?? EmptyOrNullKey, selfId);
        }
        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read);
        }
        /// <inheritdoc cref="ITypeAuthService.CanRead(ReadAction)"/>
        public bool CanRead(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Read, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.CanWrite(ReadWriteAction)"/>
        public bool CanWrite(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        /// <inheritdoc cref="ITypeAuthService.CanWrite(ReadWriteAction)"/>
        public bool CanWrite(DynamicReadWriteAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.CanWrite(ReadWriteAction)"/>
        public bool CanWrite(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write);
        }

        /// <inheritdoc cref="ITypeAuthService.CanWrite(ReadWriteAction)"/>
        public bool CanWrite(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Write, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.CanDelete(ReadWriteDeleteAction)"/>
        public bool CanDelete(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete);
        }

        /// <inheritdoc cref="ITypeAuthService.CanDelete(ReadWriteDeleteAction)"/>
        public bool CanDelete(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Delete, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.CanAccess(BooleanAction)"/>
        public bool CanAccess(BooleanAction action)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Maximum);
        }

        /// <inheritdoc cref="ITypeAuthService.CanAccess(BooleanAction)"/>
        public bool CanAccess(DynamicBooleanAction action, string? Id, params string[]? selfId)
        {
            return this.TypeAuthContextHelper.Can(action, Access.Maximum, Id ?? EmptyOrNullKey, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.AccessValue(TextAction)"/>
        public string? AccessValue(TextAction action)
        {
            return GetTextAccessValue(action, action);
        }

        /// <inheritdoc cref="ITypeAuthService.AccessValue(DecimalAction)"/>
        public decimal? AccessValue(DecimalAction action)
        {
            var textValue = AccessValue(action as TextAction);

            if (textValue != null)
                return decimal.Parse(textValue, CultureInfo.InvariantCulture);

            return null;
        }

        /// <inheritdoc cref="ITypeAuthService.AccessValue(DynamicTextAction, string, string[])"/>
        public string? AccessValue(DynamicTextAction action, string? Id, params string[]? selfId)
        {
            return GetTextAccessValue(action, action, Id, selfId);
        }

        /// <inheritdoc cref="ITypeAuthService.AccessValue(DynamicDecimalAction, string, string[])"/>
        public decimal? AccessValue(DynamicDecimalAction action, string? Id, params string[]? selfId)
        {
            var textValue = AccessValue(action as DynamicTextAction, Id, selfId);

            if (textValue != null)
                return decimal.Parse(textValue, CultureInfo.InvariantCulture);

            return null;
        }

        internal string? GetTextAccessValue(ActionBase action, ITextAccessProperties textProps, string? Id = null, params string[]? selfId)
        {
            var access = textProps.MinimumAccess;

            var actionMatches = this.TypeAuthContextHelper.LocateActionInBank(action, Id, selfId).ToList();

            for (int i = 0; i < actionMatches.Count; i++)
            {
                string? thisAccess = actionMatches[i].AccessValue;

                if (i > 0)
                {
                    if (textProps.Comparer != null)
                        thisAccess = textProps.Comparer(access, thisAccess);

                    if (textProps.Merger != null)
                        thisAccess = textProps.Merger(access, thisAccess);
                }

                if (thisAccess != null)
                    access = thisAccess;
            }

            return access;
        }

        /// <inheritdoc cref="ITypeAuthService.GetRegisteredActionTrees"/>
        public Type[] GetRegisteredActionTrees()
        {
            return this.ActionTrees.ToArray();
        }

        /// <inheritdoc cref="ITypeAuthService.GetAccessibleItemsByAccess"/>
        public AccessibleItemsByAccess GetAccessibleItemsByAccess(DynamicAction dynamicAction, params string[]? selfId)
        {
            var locatedActions = this.TypeAuthContextHelper.LocateActionInBank(dynamicAction).ToList();

            var wildCardRead = false;
            var wildCardWrite = false;
            var wildCardDelete = false;
            var wildCardMaximum = false;

            foreach (var item in locatedActions)
            {
                if (!wildCardRead && item.AccessList.Contains(Access.Read)) wildCardRead = true;
                if (!wildCardWrite && item.AccessList.Contains(Access.Write)) wildCardWrite = true;
                if (!wildCardDelete && item.AccessList.Contains(Access.Delete)) wildCardDelete = true;
                if (!wildCardMaximum && item.AccessList.Contains(Access.Maximum)) wildCardMaximum = true;

                if (wildCardRead && wildCardWrite && wildCardDelete && wildCardMaximum) break;
            }

            var readIds = new List<string>();
            var writeIds = new List<string>();
            var deleteIds = new List<string>();
            var maximumIds = new List<string>();

            if (!wildCardRead || !wildCardWrite || !wildCardDelete || !wildCardMaximum)
            {
                foreach (var item in locatedActions)
                {
                    foreach (var subItem in item.SubActionBankItems)
                    {
                        var id = (subItem.Action as DynamicAction)!.Id!;
                        var accessList = subItem.AccessList;

                        if (!wildCardRead && accessList.Contains(Access.Read)) readIds.Add(id);
                        if (!wildCardWrite && accessList.Contains(Access.Write)) writeIds.Add(id);
                        if (!wildCardDelete && accessList.Contains(Access.Delete)) deleteIds.Add(id);
                        if (!wildCardMaximum && accessList.Contains(Access.Maximum)) maximumIds.Add(id);
                    }
                }
            }

            ResolveSelfReference(readIds, selfId);
            ResolveSelfReference(writeIds, selfId);
            ResolveSelfReference(deleteIds, selfId);
            ResolveSelfReference(maximumIds, selfId);

            return new AccessibleItemsByAccess(
                new AccessibleItemsResult(wildCardRead, readIds),
                new AccessibleItemsResult(wildCardWrite, writeIds),
                new AccessibleItemsResult(wildCardDelete, deleteIds),
                new AccessibleItemsResult(wildCardMaximum, maximumIds));
        }

        /// <inheritdoc cref="ITypeAuthService.GetAccessibleItems"/>
        public AccessibleItemsResult GetAccessibleItems(DynamicAction dynamicAction, Func<Access, bool> predicate, params string[]? selfId)
        {
            var locatedActions = this.TypeAuthContextHelper.LocateActionInBank(dynamicAction).ToList();

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

            ResolveSelfReference(ids, selfId);

            return new AccessibleItemsResult(wildCardAccess, ids);
        }

        private static void ResolveSelfReference(List<string> ids, string[]? selfId)
        {
            if (selfId is null) return;
            if (!ids.Contains(SelfReferenceKey)) return;

            ids.Remove(SelfReferenceKey);
            ids.InsertRange(0, selfId);
        }


        /// <inheritdoc cref="ITypeAuthService.GetReadableItems(DynamicReadAction, string[])"/>
        public AccessibleItemsResult GetReadableItems(DynamicReadAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Read, selfId);

        /// <inheritdoc cref="ITypeAuthService.GetReadableItems(DynamicReadAction, string[])"/>
        public AccessibleItemsResult GetReadableItems(DynamicReadWriteAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Read, selfId);

        /// <inheritdoc cref="ITypeAuthService.GetReadableItems(DynamicReadAction, string[])"/>
        public AccessibleItemsResult GetReadableItems(DynamicReadWriteDeleteAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Read, selfId);

        /// <inheritdoc cref="ITypeAuthService.GetWritableItems(DynamicReadWriteAction, string[])"/>
        public AccessibleItemsResult GetWritableItems(DynamicReadWriteAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Write, selfId);

        /// <inheritdoc cref="ITypeAuthService.GetWritableItems(DynamicReadWriteAction, string[])"/>
        public AccessibleItemsResult GetWritableItems(DynamicReadWriteDeleteAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Write, selfId);

        /// <inheritdoc cref="ITypeAuthService.GetDeletableItems(DynamicReadWriteDeleteAction, string[])"/>
        public AccessibleItemsResult GetDeletableItems(DynamicReadWriteDeleteAction action, params string[]? selfId)
            => GetAccessibleItems(action, x => x == Access.Delete, selfId);
    }
}
