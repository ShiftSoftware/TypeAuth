using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System;
using System.Collections.Generic;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass()]
    public class GenerateAccessTree
    {
        [TestMethod]
        public void SameContext()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            var accessTree = new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth);

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

            var accessTree = new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth);

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

            var accessTree = new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth);

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

            var accessTree = new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth);

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

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
                new AccessTreeGenerator(tAuth_Reducer).GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void AccessPreserver()
        {
            var tAuth_Producer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read },
                        DiscountVouchers = new List<Access> { Access.Read },
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read, Access.Write },
                        Tickets = new List<Access> { Access.Read },
                    }
                }))
                .Build();

            var tAuth_Preserver = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        DiscountVouchers = new List<Access> { Access.Read },
                        Tickets = new List<Access> { Access.Read, Access.Write }
                    },
                    SystemActions = new
                    {
                        UserModule = new
                        {
                            Users = new List<Access> { Access.Read, Access.Write, Access.Delete }
                        }
                    }
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver),
                JsonConvert.SerializeObject(new
                {
                    CRMActions = new
                    {
                        Customers = new List<Access> { Access.Read, Access.Delete },
                        DiscountVouchers = new List<Access> { Access.Read },
                        Tickets = new List<Access> { Access.Write }
                    },
                    SystemActions = new
                    {
                        UserModule = new
                        {
                            Users = new List<Access> { Access.Read, Access.Write, Access.Delete }
                        }
                    }
                })
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

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
                new AccessTreeGenerator(tAuth_Reducer).GenerateAccessTree(tAuth_Reducer)
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

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
                new AccessTreeGenerator(tAuth_Reducer).GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void WildCardAccessPreserver()
        {
            var tAuth_Producer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read, Access.Write, Access.Delete }
                }))
                .Build();

            var tAuth_Preserver = new TypeAuthContextBuilder()
                .AddActionTree<CRMActions>()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete }
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver),
                JsonConvert.SerializeObject(new
                {
                    CRMActions = new List<Access> { Access.Read, Access.Maximum },
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete },
                })
            );
        }

        [TestMethod]
        public void WildCardAccessPreserver2()
        {
            var tAuth_Producer = new TypeAuthContextBuilder()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        UserModule = new List<Access> { Access.Read, Access.Write, Access.Delete }
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .Build();

            var tAuth_Preserver = new TypeAuthContextBuilder()
                .AddActionTree<SystemActions>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    SystemActions = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .Build();

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver),
                JsonConvert.SerializeObject(new
                {
                    SystemActions = new
                    {
                        UserModule = new List<Access> { Access.Read, Access.Write, Access.Delete }
                    }
                })
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

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
                new AccessTreeGenerator(tAuth_Reducer).GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void DynamicActions_Preserver()
        {
            var tAuth_Producer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Companies = new
                        {
                            _1 = new List<Access> {  },
                        },
                        Cities = new {
                            _1 = new List<Access> { Access.Read },
                        },
                        Departments = new
                        {
                            _1 = new List<Access> { },
                            _2 = new List<Access> { },
                            _3 = new List<Access> { },
                        },
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
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
                .Build();

            var tAuth_Preserver = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Countries = new 
                        {
                            _1 = new List<Access> { Access.Read, Access.Write },
                        },
                        Companies = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write },
                        },
                        Cities = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        },
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _2 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _3 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        }
                    },
                }))
                .Build();

            //Assert.IsTrue(tAuth_Preserver.CanWrite(DataLevel.Companies, "_1"));

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver),
                JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Cities = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        },
                        Countries = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write },
                        },
                        Companies = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write },
                        },
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Write, Access.Delete },
                            _2 = new List<Access> { Access.Delete },
                            _3 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        }
                    },
                })
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

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
                new AccessTreeGenerator(tAuth_Reducer).GenerateAccessTree(tAuth_Reducer)
            );
        }

        [TestMethod]
        public void DynamicActionWildCardPreserver()
        {
            var tAuth_Producer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Companies = new List<Access>() { Access.Read },
                        Departments = new List<Access> { Access.Read },
                    }
                }))
                .Build();

            var tAuth_Reducer = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Companies = new List<Access>() { Access.Read },
                        Departments = new List<Access> { Access.Read, Access.Write },
                    },
                }))
                .Build();

            var tAuth_Preserver = new TypeAuthContextBuilder()
                .AddActionTree<DataLevel>()
                .AddAccessTree(JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access>() { Access.Maximum },
                        Companies = new List<Access>() { Access.Read, Access.Write },
                        Departments = new List<Access> { Access.Read, Access.Write, Access.Delete },
                    },
                }))
                .Build();

            //Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth_Producer).GenerateAccessTree(tAuth_Reducer, tAuth_Preserver),
                JsonConvert.SerializeObject(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access>() { Access.Maximum },
                        Companies = new List<Access>() { Access.Read, Access.Write },
                        Departments = new List<Access> { Access.Read, Access.Delete },
                    }
                })
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

            Console.WriteLine(Newtonsoft.Json.Linq.JObject.Parse(new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer)).ToString());

            Assert.AreEqual(
                new AccessTreeGenerator(tAuth).GenerateAccessTree(tAuth_Reducer),
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
