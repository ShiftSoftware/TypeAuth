using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests
{
    class AccessTreeFiles
    {
        public static string SuperAdmin = System.IO.File.ReadAllText("ERP/AccessTrees/SuperAdmin.json");
        public static string SalesAdmin = System.IO.File.ReadAllText("ERP/AccessTrees/SalesAdmin.json");
        public static string CRMAgent = System.IO.File.ReadAllText("ERP/AccessTrees/CRMAgent.json");
        public static string Affiliates = System.IO.File.ReadAllText("ERP/AccessTrees/Affiliates.json");
    }

    class AccessTreeHelper
    {
        public static TypeAuthContext GetTypeAuthContext(string jsonAccessTree)
        {
            var typeAuthContext = new TypeAuthContextBuilder()
                .AddAccessTree(jsonAccessTree)
                .AddActionTree<SystemActions>()
                .AddActionTree<CRMActions>()
                .Build();

            return typeAuthContext;

            //return new TypeAuthContext(jsonAccessTree, typeof(SystemActions), typeof(CRMActions));
        }
    }
}
