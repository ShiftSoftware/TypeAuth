using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuthTests.HypoERP.ActionTrees;

namespace TypeAuthTests
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
