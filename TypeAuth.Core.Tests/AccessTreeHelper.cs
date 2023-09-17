using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.Collections.Generic;

namespace ShiftSoftware.TypeAuth.Tests
{
    class AccessTreeFiles
    {
        public static string SuperAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum },
            CRMActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum },
        });

        public static string SalesAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            SystemActions = new
            {
                UserModule = new
                {
                    Users = new List<Access> { Access.Read, Access.Write }
                }
            },
            CRMActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum },
        });

        public static string CRMAgent = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            CRMActions = new
            {
                Customers = new List<Access> { Access.Read, Access.Write },
                DiscountVouchers = new List<Access> { Access.Read, Access.Write },
                DiscountValue = "10",
                DecimalDiscount = 15.0,
                Tickets = new List<Access> { Access.Read, Access.Write },
                SocialMediaComments = new List<Access> { Access.Read },
                WorkSchedule = "08:00:00 - 13:00:00, 14:00:00 - 18:00:00"
            }
        });

        public static string Affiliates = Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            CRMActions = new
            {
                Customers = new List<Access> { Access.Read },
                DiscountVouchers = new List<Access> { Access.Read },
                DiscountValue = "2",
                DecimalDiscount = 2.50,
                WorkSchedule = "20:00:00 - 21:00:00"
            }
        });
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
