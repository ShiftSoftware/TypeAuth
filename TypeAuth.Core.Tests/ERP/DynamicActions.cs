using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using System.Collections.Generic;
using System.Text.Json;
using TypeAuthTests.ERP.ActionTrees;

namespace TypeAuthTests.ERP
{
    [TestClass()]
    public class DynamicActions
    {
        Dictionary<string, string> DatabaseItems = new Dictionary<string, string> {
            { "1", "Marketting" },
            { "2", "Sales" },
            { "3", "CRM" },
            { "4", "Finance" },
            { "5", "IT" },
        };

        public DynamicActions()
        {
            DataLevel.Departments.ExpandWith(DatabaseItems);
        }

        [TestMethod("Full Access On All Departments")]
        public void FullAccessOnAllDepartments()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _shift_software_type_auth_core_self_reference = new List<Access> { Access.Read, Access.Write, Access.Delete, Access.Maximum }
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            Assert.IsTrue(typeAuth.CanRead(DataLevel.Departments, "5", "1"));
        }
    }
}
