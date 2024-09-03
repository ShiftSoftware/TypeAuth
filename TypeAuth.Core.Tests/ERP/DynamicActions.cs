using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.Collections.Generic;
using System.Text.Json;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class DynamicActions
    {
        public DynamicActions()
        {
            //var data = new List<KeyValuePair<string, string>>
            //{
            //    new KeyValuePair<string, string>("_1", "One"),
            //    new KeyValuePair<string, string>("_2", "Two"),
            //    new KeyValuePair<string, string>("_3", "Three"),
            //    new KeyValuePair<string, string>("_4", "Four"),
            //    new KeyValuePair<string, string>("_5", "Five"),
            //    new KeyValuePair<string, string>("_6", "Six"),
            //};

            //DataLevel.Cities.Expand(data);
            //DataLevel.Countries.Expand(data);
            //DataLevel.Companies.Expand(data);
            //DataLevel.Departments.Expand(data);
            //DataLevel.DiscountByDepartment.Expand(data);
        }

        [TestMethod("Full Access On All Cities")]
        public void FullAccessOnAllCities()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4"));


            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x => x.Countries, "_1"));
            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x => x.Countries, "_2"));
            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x => x.Countries, "_3"));
            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x => x.Countries, "_4"));

            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_1"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_2"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_3"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_4"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_5"));

            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_4"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_5"));
        }

        [TestMethod("Full Access On All Countries")]
        public void FullAccessOnAllCountries()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Countries = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Countries, "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Countries, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Countries, "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Countries, "_4"));
        }

        [TestMethod("Full Access On All Companies")]
        public void FullAccessOnAllCompanies()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Companies = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_4"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_5"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_4"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_5"));
        }

        [TestMethod("Full Access On All Departments")]
        public void FullAccessOnAllDepartments()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_5"));
        }

        [TestMethod("Full Access On All Departments - WildCard")]
        public void FullAccessOnAllDepartments_WildCard()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1", "_5"));
        }

        [TestMethod("Access On Certain Cities")]
        public void AccessOnCertainCities()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new
                        {
                            _2 = new List<Access> { Access.Maximum },
                            _3 = new List<Access> { Access.Maximum },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4"));
        }

        [TestMethod("Access On Self City")]
        public void AccessOnSelfCity()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Maximum },
                            _2 = new List<Access> { Access.Maximum },
                            _3 = new List<Access> { Access.Maximum },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1", null));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, null, null));

            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1", "_1")); //True
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1", "_4"));

            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3", "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3", "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4", "_1"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4", "_2"));
            Assert.IsFalse(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4", "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4", "_4")); //True
        }

        [TestMethod("Certain Access On Departments")]
        public void CertainAccessOnDepartments()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read, Access.Write },
                            _2 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _3 = new List<Access> { Access.Read },
                            _4 = new List<Access> { Access.Read, Access.Write },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_1"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1", "_1"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_1", "_1"));

            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_1", "_2"));

            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_1", "_3"));

            Assert.IsFalse(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1", "_5"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1", "_5"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_1", "_5"));


            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_3", "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_3", "_3"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_3", "_1"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_3", "_3"));
            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_3", "_1"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_3", "_2"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_3", "_3"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_3", "_4"));

        }

        [TestMethod("Multiple Access Trees")]
        public void MultipleAccessTress()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read },
                            _1 = new List<Access> { Access.Read }
                        }
                    }
                }))
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _1 = new List<Access> { Access.Write }
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x=> x.Departments, "_1"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x=> x.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x=> x.Departments, "_2", "_2"));
        }

        [TestMethod("Text Action")]
        public void TextAction()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartment = new
                        {
                            _shift_software_type_auth_core_self_reference = "80",
                            _2 = "20",
                            _3 = "30",
                            _4 = "40",
                        }
                    }
                }))
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartment = new
                        {
                            _shift_software_type_auth_core_self_reference = "85",
                            _4 = "60"
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.AreEqual("20", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_2"));
            Assert.AreEqual("30", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_3"));
            Assert.AreEqual("60", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_4"));

            Assert.AreEqual("85", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_1", "_1"));
            Assert.AreEqual("85", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_2", "_2"));
            Assert.AreEqual("85", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_3", "_3"));
            Assert.AreEqual("85", typeAuth.AccessValue<DataLevel>(x=> x.DiscountByDepartment, "_4", "_4"));
        }

        [TestMethod("Decimal Action")]
        public void DecimalAction()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartmentDecimal = new
                        {
                            _shift_software_type_auth_core_self_reference = 80,
                            _2 = 20,
                            _3 = 30,
                            _4 = 40,
                        }
                    }
                }))
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        DiscountByDepartmentDecimal = new
                        {
                            _shift_software_type_auth_core_self_reference = 85,
                            _4 = 60
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            System.Diagnostics.Debug.WriteLine(typeAuth.GenerateAccessTree(typeAuth));

            Assert.AreEqual(20, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_2"));
            Assert.AreEqual(30, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_3"));
            Assert.AreEqual(60, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_4"));

            Assert.AreEqual(85, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_1", "_1"));
            Assert.AreEqual(85, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_2", "_2"));
            Assert.AreEqual(85, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_3", "_3"));
            Assert.AreEqual(85, typeAuth.AccessValue<DataLevel>(x => x.DiscountByDepartmentDecimal, "_4", "_4"));
        }

        [TestMethod("Wild Card And Normal Access Tree")]
        public void WildCardAndNormalAccessTree()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new List<Access> { Access.Read }
                }))
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new List<Access> { Access.Read, Access.Write }
                    }
                }))
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete }
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Companies, "_2"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_2"));

            Assert.IsFalse(typeAuth.CanWrite<DataLevel>(x => x.Companies, "_2"));
            Assert.IsFalse(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2", "_2"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1"));
        }

        [TestMethod("Multiple Action Trees")]
        public void MultipleActionTrees()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<CRMActions>()
                .AddActionTree<DataLevel>()
                .AddActionTree<SystemActions>()
                .Build();

            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_4"));
        }

        [TestMethod("Multiple TypeAuth Context")]
        public void MultipleContext()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access> { Access.Maximum },
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _2 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                            _3 = new List<Access> { Access.Read, Access.Write, Access.Delete },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            var typeAuth2 = new TypeAuthContextBuilder()
               .AddAccessTree(JsonSerializer.Serialize(new
               {
                   DataLevel = new
                   {
                       Cities = new
                       {
                           _1 = new List<Access> { Access.Maximum },
                           _10 = new List<Access> { Access.Maximum },
                           _20 = new List<Access> { Access.Maximum },
                       },
                       Departments = new
                       {
                           _1 = new List<Access> { Access.Read, Access.Write },
                       }
                   }
               }))
               .AddActionTree<DataLevel>()
               .Build();

            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_10"));
            Assert.IsTrue(typeAuth.CanAccess<DataLevel>(x => x.Cities, "_20"));

            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead<DataLevel>(x => x.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite<DataLevel>(x => x.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete<DataLevel>(x => x.Departments, "_3"));


            Assert.IsTrue(typeAuth2.CanRead<DataLevel>(x => x.Departments, "_1"));
            Assert.IsTrue(typeAuth2.CanWrite<DataLevel>(x => x.Departments, "_1"));
            Assert.IsFalse(typeAuth2.CanDelete<DataLevel>(x => x.Departments, "_1"));

            Assert.IsFalse(typeAuth2.CanRead<DataLevel>(x => x.Departments, "_2"));
            Assert.IsFalse(typeAuth2.CanWrite<DataLevel>(x => x.Departments, "_2"));
            Assert.IsFalse(typeAuth2.CanDelete<DataLevel>(x => x.Departments, "_2"));

            Assert.IsFalse(typeAuth2.CanRead<DataLevel>(x => x.Departments, "_3"));
            Assert.IsFalse(typeAuth2.CanWrite<DataLevel>(x => x.Departments, "_3"));
            Assert.IsFalse(typeAuth2.CanDelete<DataLevel>(x => x.Departments, "_3"));
        }

        [TestMethod("Get Accessible Items - Wildcard")]
        public void GetAccessibleItems_Wildcard()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).WildCard);
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).WildCard);
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).WildCard);
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).WildCard);

            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read || x == Access.Write).WildCard);
        }

        [TestMethod("Get Accessible Items - Ids")]
        public void GetAccessibleItems_Ids()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Cities = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read, Access.Write },
                            _1 = new List<Access> { },
                            _2 = new List<Access> { Access.Read },
                            _3 = new List<Access> { Access.Write },
                            _4 = new List<Access> { Access.Delete },
                            _5 = new List<Access> { Access.Read, Access.Write },
                            _6 = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).WildCard);
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).WildCard);
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).WildCard);
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).WildCard);
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read || x == Access.Write).WildCard);

            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).AccessibleIds.Contains("_1"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).AccessibleIds.Contains("_1"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).AccessibleIds.Contains("_1"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).AccessibleIds.Contains("_1"));

            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).AccessibleIds.Contains("_2"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).AccessibleIds.Contains("_2"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).AccessibleIds.Contains("_2"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).AccessibleIds.Contains("_2"));

            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).AccessibleIds.Contains("_3"));
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).AccessibleIds.Contains("_3"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).AccessibleIds.Contains("_3"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).AccessibleIds.Contains("_3"));

            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read).AccessibleIds.Contains("_5"));
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write).AccessibleIds.Contains("_5"));
            Assert.IsTrue(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read || x == Access.Write).AccessibleIds.Contains("_5"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete).AccessibleIds.Contains("_5"));
            Assert.IsFalse(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Maximum).AccessibleIds.Contains("_5"));

            CollectionAssert.AreEqual(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Read, "self").AccessibleIds, new List<string> { "self", "_2", "_5", "_6" });
            CollectionAssert.AreEqual(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Write, "I").AccessibleIds, new List<string> { "I", "_3", "_5", "_6" });
            CollectionAssert.AreEqual(typeAuth.GetAccessibleItems<DataLevel>(x=> x.Cities, x => x == Access.Delete, "me").AccessibleIds, new List<string> { "_4", "_6" });
        }
    }
}
