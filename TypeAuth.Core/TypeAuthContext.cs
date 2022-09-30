using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// This is what you use to check Action Trees against an Access Tree
    /// </summary>
    public class TypeAuthContext
    {
        private TypeAuthContextHelper TypeAuthContextHelper { get; set; }
        public const string SelfRererenceKey = "shift-software:type-auth-core:self-reference";

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
            var actionTree = this.TypeAuthContextHelper.GenerateActionTree(actionTrees.ToList());

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionTree, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            }));

            Console.WriteLine();

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
        
        public bool CanRead(ReadAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Read);
        }
        public bool CanRead(ReadWriteAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Read);
        }
        public bool CanRead(ReadWriteDeleteAction action)
        {
            return this.TypeAuthContextHelper.CheckActionBank(action, Access.Read);
        }

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

        public void OverwriteAccessTree(string accessTree)
        {

        }

        //public Dictionary<string, List<Access>> AllItems(List<DynamicAction> dataList)
        //{
        //    return dataList.Select(x => new { x.Id, AccessTypes = ActionAccessTypes(x) }).ToDictionary(x => x.Id, x => x.AccessTypes);
        //}
        public List<string> ReadableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => this.TypeAuthContextHelper.CheckActionBank(x, Access.Read)).Select(x => x.Id).ToList();
        }
        public List<string> WritableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => this.TypeAuthContextHelper.CheckActionBank(x, Access.Write)).Select(x => x.Id).ToList();
        }
        public List<string> DeletableItems(List<DynamicAction> dataList)
        {
            return dataList.Where(x => this.TypeAuthContextHelper.CheckActionBank(x, Access.Delete)).Select(x => x.Id).ToList();
        }
    }
}
