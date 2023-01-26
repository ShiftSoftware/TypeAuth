using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using static ShiftSoftware.TypeAuth.Shared.SystemActions;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass()]
    public class GenerateAccessTree
    {
        [TestMethod]
        public void SameContext()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            var accessTree = tAuth.GenerateAccessTree(tAuth);

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString());

            Assert.AreEqual(
                Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString(),
                Newtonsoft.Json.Linq.JObject.Parse(AccessTreeFiles.Affiliates).ToString()
            );
        }

        [TestMethod]
        public void SameContext2()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            var accessTree = tAuth.GenerateAccessTree(tAuth);

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString());

            Assert.AreEqual(
                Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString(),
                Newtonsoft.Json.Linq.JObject.Parse(AccessTreeFiles.CRMAgent).ToString()
            );
        }

        [TestMethod]
        public void SameContext3()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

            var accessTree = tAuth.GenerateAccessTree(tAuth);

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString());

            Assert.AreEqual(
                Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString(),
                Newtonsoft.Json.Linq.JObject.Parse(AccessTreeFiles.SuperAdmin).ToString()
            );
        }

        [TestMethod]
        public void SameContext4()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            var accessTree = tAuth.GenerateAccessTree(tAuth);

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString());

            Assert.AreEqual(
                Newtonsoft.Json.Linq.JObject.Parse(accessTree).ToString(),
                Newtonsoft.Json.Linq.JObject.Parse(AccessTreeFiles.SalesAdmin).ToString()
            );
        }

        [TestMethod]
        public void AccessReducer()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read, Access.Write, Access.Delete }
                    }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        Login = new
                        {
                            MultipleSession = new List<Access> { Access.Maximum }
                        }
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read }
                    }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        Login = new
                        {
                            MultipleSession = new List<Access> { }
                        }
                    }
                }))
                .Build();

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                tAuth_Reducer.GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void AccessValueReducer()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        DiscountValue = "90"
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        DiscountValue = "50"
                    }
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                tAuth_Reducer.GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void WildCardAccessReducer()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read, Access.Write }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        Login = new
                        {
                            MultipleSession = new List<Access> { }
                        }
                    }
                }))
                .Build();

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                tAuth_Reducer.GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void DynamicActions()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _2 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _3 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        }
                    },
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Read },
                            _2 = new List<Access> { Access.Read, Access.Write },
                        }
                    },
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        Login = new
                        {
                            MultipleSession = new List<Access> { }
                        }
                    }
                }))
                .Build();

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                tAuth_Reducer.GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void DynamicActionWildCard()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Departments = new List<Access> { Access.Read, Access.Write, Access.Delete },
                    },
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Departments = new List<Access> { Access.Read }
                    },
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        Login = new
                        {
                            MultipleSession = new List<Access> { }
                        }
                    }
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                tAuth_Reducer.GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void DynamicActionAccessValue()
        {
            var tAuth = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartment = new { 
                            _1 = "90",
                            _2 = "100",
                            _3 = "70"

                        },
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartment = new
                        {
                            _1 = "95",
                            _2 = "85",
                            _3 = "60"
                        },
                    },
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(tAuth.GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                tAuth.GenerateAccessTree(tAuth_Reducer),
                JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartment = new
                        {
                            _1 = "90",
                            _2 = "85",
                            _3 = "60"
                        },
                    },
                })
            );
        }
    }
}
