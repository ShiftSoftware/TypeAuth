namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// This is what you use to check Action Trees against an Access Tree
    /// </summary>
    public class TypeAuthContext
    {
        public const string SelfRererenceKey = "shift-software:type-auth:self-reference";
        protected private List<ActionBankItem> ActionBank { get; set; }

        /// <summary>
        /// Constructs a Context by Providing a list of Action Trees and an Access Tree provided as a serialized JSON string.
        /// </summary>
        /// <param name="actionTrees">A list of Action Trees to Check your Access Tree against.</param>
        /// <param name="accessTreeJSONString">The Access Tree provided as a JSON string. Access Tree contains the Actions that a Subject can perform.</param>
        public TypeAuthContext(string accessTreeJSONString = "{}", params Type[] actionTrees)
        {
            ActionBank = new List<ActionBankItem>();

            var actionTree = GenerateActionTree(actionTrees.ToList());

            var accessTree = Newtonsoft.Json.JsonConvert.DeserializeObject(accessTreeJSONString);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionTree, Newtonsoft.Json.Formatting.Indented));
            Console.WriteLine();

            if (accessTree != null)
            {
                Console.WriteLine(accessTree.ToString());
                this.PopulateActionBank(actionTree, accessTree);
            }
        }

        private Dictionary<string, object> GenerateActionTree(List<Type> actionTrees, Dictionary<string, object>? rootDictionary = null)
        {
            if (rootDictionary is null)
                rootDictionary = new Dictionary<string, object>();

            foreach (var tree in actionTrees)
            {
                var treeDictionary = new Dictionary<string, object>();

                rootDictionary[tree.Name] = treeDictionary;

                var childTress = tree.GetNestedTypes().ToList().Where(x => x.GetCustomAttributes(typeof(ActionTree), false) != null).ToList();

                GenerateActionTree(childTress, treeDictionary);

                tree.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).ToList().ForEach(y =>
                {
                    var value = (Action?)y.GetValue(y);

                    if (value is not null)
                        treeDictionary[y.Name] = value;
                });

                tree.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(y => y.ReturnType == typeof(List<DynamicAction>)).ToList().ForEach(y =>
                {
                    var invoked = (y.Invoke(y, new object[] { this }) as List<DynamicAction>);

                    if (invoked != null)
                        treeDictionary[y.Name] = invoked.ToDictionary(z => z.Id.ToString(), z => (object)new DynamicAction(z.Id.ToString(), z.Name, z.Type));
                });
            }

            return rootDictionary;
        }

        private void PopulateActionBank(object actionCursor, object? accessCursor)
        {
            var accessTypes = new List<Access>();
            string? accessValue = null;

            if (accessCursor != null)
            {
                if (accessCursor.GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                {
                    accessValue = accessCursor.ToString();
                }
                else if (accessCursor.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    var theArray = ((Newtonsoft.Json.Linq.JArray)accessCursor).Select(x => x.ToObject<Access>()).ToList();

                    accessTypes.AddRange(theArray);
                }
            }

            if (actionCursor.GetType() == typeof(Dictionary<string, object>))
            {
                var theDictionary = (Dictionary<string, object>)actionCursor;

                foreach (var key in theDictionary.Keys)
                {
                    //Wild Card: Access already provided at this Level of the Access Tree. But the action tree has more child nodes.
                    //The current Access is simply passed to every child node of the the current Action Node
                    if (accessTypes.Count > 0 || accessValue != null)
                    {
                        this.PopulateActionBank(theDictionary[key], accessCursor);
                    }
                    else 
                    {
                        if (accessCursor != null)
                        {
                            var accessCursorDictionary = (Newtonsoft.Json.Linq.JObject)accessCursor;

                            PopulateActionBank(theDictionary[key], accessCursorDictionary[key]);
                        }
                    }

                }
            }

            if ((actionCursor.GetType() == typeof(Action) || actionCursor.GetType() == typeof(DynamicAction)) && (accessTypes.Count > 0 || accessValue != null))
            {
                var thePermission = (Action)actionCursor;

                if (thePermission.Type == ActionType.Text && accessValue == null)
                {
                    if (accessTypes.Contains(Access.Maximum))
                        accessValue = thePermission.MaximumAccess;
                    else
                        accessValue = thePermission.MinimumAccess;
                }

                this.ActionBank.Add(new ActionBankItem(thePermission, accessTypes, accessValue));
            }
        }

        private ActionBankItem? LocatePermissionInBank(Action permissionToCheck)
        {
            ActionBankItem? permissionMatch = null;

            var theDynamicPermission = permissionToCheck as DynamicAction;

            if (theDynamicPermission is not null)
            {
                permissionMatch = this.ActionBank.Where(x => x.Action.GetType() == typeof(DynamicAction)).FirstOrDefault(x => ((DynamicAction)x.Action).Id.Equals(theDynamicPermission.Id));
            }
            else
                permissionMatch = this.ActionBank.FirstOrDefault(x => x.Action == permissionToCheck);

            return permissionMatch;
        }

        private bool CheckPermissionBank(Action permissionToCheck, Access accessTypeToCheck)
        {
            var access = false;

            var permissionMatch = this.LocatePermissionInBank(permissionToCheck);

            if (permissionMatch != null)
                access = permissionMatch.AccessList.IndexOf(accessTypeToCheck) > -1;

            return access;
        }

        public static string ReplaceSelfReferenceVariable(string id, string actualValue)
        {
            return id == SelfRererenceKey ? actualValue : id;
        }

        public List<Access> PermissionAccessTypes(Action permission)
        {
            var accessTypes = new List<Access>();

            var permissionMatch = this.LocatePermissionInBank(permission);

            if (permissionMatch != null)
                accessTypes = permissionMatch.AccessList;

            return accessTypes;
        }
        public bool CanRead(Action permission)
        {
            return this.CheckPermissionBank(permission, Access.Read);
        }
        public bool CanWrite(Action permission)
        {
            return this.CheckPermissionBank(permission, Access.Write);
        }
        public bool CanDelete(Action permission)
        {
            return this.CheckPermissionBank(permission, Access.Delete);
        }
        public bool CanAccess(Action permission)
        {
            return this.CheckPermissionBank(permission, Access.Read);
        }
        public string? AccessValue(Action permission)
        {
            var access = permission.MinimumAccess;

            var permissionMatch = this.LocatePermissionInBank(permission);

            if (permissionMatch != null && permissionMatch.AccessValue != null)
            {
                access = permissionMatch.AccessValue;
            }

            return access;
        }

        public Dictionary<string, List<Access>> AllItems(List<DynamicAction> dataList)
        {
            return dataList.Select(x => new { x.Id, AccessTypes = PermissionAccessTypes(x) }).ToDictionary(x => x.Id, x => x.AccessTypes);
        }
        public List<string> ReadableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => CheckPermissionBank(x, Access.Read)).Select(x => x.Id).ToList();
        }
        public List<string> WritableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => CheckPermissionBank(x, Access.Write)).Select(x => x.Id).ToList();
        }
        public List<string> DeletableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => CheckPermissionBank(x, Access.Delete)).Select(x => x.Id).ToList();
        }
    }
}
