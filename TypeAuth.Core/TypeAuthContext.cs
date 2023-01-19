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

        private void Init(List<string> accessTreeJSONStrings, params Type[] actionTrees)
        {
            this.AccessTreeJsonStrings = accessTreeJSONStrings;
            this.ActionTrees = actionTrees;

            var actionTree = this.TypeAuthContextHelper.GenerateActionTree(actionTrees.ToList(), accessTreeJSONStrings);

            //Console.WriteLine("Action Trees Are:");
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionTree, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            //{
            //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            //}));

            //Console.WriteLine();

            foreach (var accessTreeJSONString in accessTreeJSONStrings)
            {
                var accessTree = Newtonsoft.Json.JsonConvert.DeserializeObject(accessTreeJSONString);

                //Console.WriteLine(accessTree.ToString());
                this.TypeAuthContextHelper.PopulateActionBank(actionTree, accessTree);
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

        internal bool Can(Type actionTreeType, string actionName, Access access)
        {
            var instance = Activator.CreateInstance(actionTreeType);

            var action = (Actions.Action) actionTreeType
                .GetFields()
                .FirstOrDefault(x => x.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                ?.GetValue(instance);

            if (action == null)
                throw new Exception($"No Such Action ({actionTreeType.FullName}.{actionName})");

            return this.TypeAuthContextHelper.CheckActionBank(action, access);
        }

        #region Read
        private bool CanRead(Actions.Action action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Read);
        }
        public bool CanRead(ReadAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        public bool CanRead(ReadWriteAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.CanRead((Actions.Action)action);
        }
        #endregion

        #region Dynamic

        public bool CanAccess(DynamicAction<BooleanAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        }

        public bool CanRead(DynamicAction<ReadAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        }

        public bool CanRead(DynamicAction<ReadWriteAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        }

        public bool CanWrite(DynamicAction<ReadWriteAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Write, key, selfReference);
        }

        public bool CanRead(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Read, key, selfReference);
        }

        public bool CanWrite(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Write, key, selfReference);
        }

        public bool CanDelete(DynamicAction<ReadWriteDeleteAction> dynamicActionDictionary, string key, string? selfReference = null)
        {
            return this.Can(dynamicActionDictionary, Access.Delete, key, selfReference);
        }

        public string? AccessValue(DynamicAction<TextAction> dynamicAction, string key, string? selfReference = null)
        {
            string? actionAccess = null;

            TextAction? action = null;

            if (dynamicAction.UnderlyingAction != null)
                action = (TextAction)dynamicAction.UnderlyingAction;

            if (dynamicAction.Dictionary.Keys.Contains(key))
                action = dynamicAction[key];

            if(action != null)
                actionAccess = AccessValue(action);

            if (selfReference == key && dynamicAction.Dictionary.Keys.Contains(SelfRererenceKey))
            {
                var selfActionAccess = AccessValue(dynamicAction[SelfRererenceKey]);

                if (dynamicAction.Comparer != null)
                {
                    actionAccess = dynamicAction.Comparer(actionAccess, selfActionAccess);
                }
            }

            return actionAccess;
        }

        private bool Can(DynamicActionBase dynamicAction, Access access, string key, string? selfReference = null)
        {
            Actions.Action? action = null;
            
            var actionAccess = false;

            var dynamicActionDictionary = dynamicAction.Dictionary;

            if (dynamicAction.UnderlyingAction != null)
                action = dynamicAction.UnderlyingAction;

            if (dynamicActionDictionary.Keys.Contains(key))
                action = dynamicActionDictionary[key];

            if (action != null)
                actionAccess = this.TypeAuthContextHelper.CheckActionBank(action, access);

            if (!actionAccess && selfReference == key && dynamicActionDictionary.Keys.Contains(SelfRererenceKey))
            {
                actionAccess = this.TypeAuthContextHelper.CheckActionBank(dynamicActionDictionary[SelfRererenceKey], access);
            }

            return actionAccess;
        }

        #endregion

        public bool CanWrite(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Write);
        }

        public bool CanWrite(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Write);
        }

        public bool CanDelete(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Delete);
        }

        public bool CanAccess(BooleanAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Read);
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
                }

                if (thisAccess != null)
                    access = thisAccess;
            }

            return access;
        }
    }
}
