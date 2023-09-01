using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using System.Collections.Generic;
using System.Text.Json;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class DynamicActions
    {
        public DynamicActions()
        {
            var data = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("_1", "One"),
                new KeyValuePair<string, string>("_2", "Two"),
                new KeyValuePair<string, string>("_3", "Three"),
                new KeyValuePair<string, string>("_4", "Four"),
                new KeyValuePair<string, string>("_5", "Five"),
                new KeyValuePair<string, string>("_6", "Six"),
            };

            DataLevel.Cities.Expand(data);
            DataLevel.Countries.Expand(data);
            DataLevel.Companies.Expand(data);
            DataLevel.Departments.Expand(data);
            DataLevel.DiscountByDepartment.Expand(data);
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

            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_4"));


            Assert.IsFalse(typeAuth.CanRead(DataLevel.Countries, "_1"));
            Assert.IsFalse(typeAuth.CanRead(DataLevel.Countries, "_2"));
            Assert.IsFalse(typeAuth.CanRead(DataLevel.Countries, "_3"));
            Assert.IsFalse(typeAuth.CanRead(DataLevel.Countries, "_4"));

            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_1"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_2"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_3"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_4"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_5"));

            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_2"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_4"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_5"));
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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Countries, "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Countries, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Countries, "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Countries, "_4"));
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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_4"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_5"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Companies, "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Companies, "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Companies, "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Companies, "_4"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Companies, "_5"));
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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_5"));
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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_5"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_3"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_4"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_5"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_3"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_4"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1", "_5"));
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

            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_4"));
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

            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1", null));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, null, null));

            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_1", "_1")); //True
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_1", "_4"));

            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3", "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3", "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_4"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_4", "_1"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_4", "_2"));
            Assert.IsFalse(typeAuth.CanAccess(DataLevel.Cities, "_4", "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_4", "_4")); //True
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

            Assert.IsFalse(typeAuth.CanRead(DataLevel.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_1"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1", "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1", "_1"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1", "_1"));

            Assert.IsFalse(typeAuth.CanRead(DataLevel.Departments, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_1", "_2"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1", "_2"));

            Assert.IsFalse(typeAuth.CanRead(DataLevel.Departments, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_1", "_3"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1", "_3"));

            Assert.IsFalse(typeAuth.CanRead(DataLevel.Departments, "_1", "_5"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_1", "_5"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_1", "_5"));


            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_3"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_4"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_3"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3", "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3", "_3"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_3", "_1"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_3", "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_3", "_3"));
            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Departments, "_3", "_4"));

            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3", "_1"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3", "_2"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3", "_3"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_3", "_4"));

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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_2"));
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

            Assert.AreEqual("20", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_2"));
            Assert.AreEqual("30", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_3"));
            Assert.AreEqual("60", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_4"));

            Assert.AreEqual("85", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_1", "_1"));
            Assert.AreEqual("85", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_2", "_2"));
            Assert.AreEqual("85", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_3", "_3"));
            Assert.AreEqual("85", typeAuth.AccessValue(DataLevel.DiscountByDepartment, "_4", "_4"));
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

            Assert.AreEqual(20, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_2"));
            Assert.AreEqual(30, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_3"));
            Assert.AreEqual(60, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_4"));

            Assert.AreEqual(85, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_1", "_1"));
            Assert.AreEqual(85, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_2", "_2"));
            Assert.AreEqual(85, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_3", "_3"));
            Assert.AreEqual(85, typeAuth.AccessValue(DataLevel.DiscountByDepartmentDecimal, "_4", "_4"));
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

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Companies, "_2"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2"));

            Assert.IsFalse(typeAuth.CanWrite(DataLevel.Companies, "_2"));
            Assert.IsFalse(typeAuth.CanDelete(DataLevel.Departments, "_2"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2", "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1"));

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

            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_2"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_3"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_4"));
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

            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_1"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_10"));
            Assert.IsTrue(typeAuth.CanAccess(DataLevel.Cities, "_20"));

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanWrite(DataLevel.Departments, "_3"));

            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_2"));
            Assert.IsTrue(typeAuth.CanDelete(DataLevel.Departments, "_3"));


            Assert.IsTrue(typeAuth2.CanRead(DataLevel.Departments, "_1"));
            Assert.IsTrue(typeAuth2.CanWrite(DataLevel.Departments, "_1"));
            Assert.IsFalse(typeAuth2.CanDelete(DataLevel.Departments, "_1"));

            Assert.IsFalse(typeAuth2.CanRead(DataLevel.Departments, "_2"));
            Assert.IsFalse(typeAuth2.CanWrite(DataLevel.Departments, "_2"));
            Assert.IsFalse(typeAuth2.CanDelete(DataLevel.Departments, "_2"));

            Assert.IsFalse(typeAuth2.CanRead(DataLevel.Departments, "_3"));
            Assert.IsFalse(typeAuth2.CanWrite(DataLevel.Departments, "_3"));
            Assert.IsFalse(typeAuth2.CanDelete(DataLevel.Departments, "_3"));
        }
    }
}
