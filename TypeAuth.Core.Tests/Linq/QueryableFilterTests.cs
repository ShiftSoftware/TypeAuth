using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Linq;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ShiftSoftware.TypeAuth.Tests.Linq
{
    [TestClass()]
    public class QueryableFilterTests
    {
        private class Row
        {
            public long? CategoryId { get; set; }
            public string Name { get; set; } = "";
        }

        private static IQueryable<Row> Rows() => new List<Row>
        {
            new Row { CategoryId = 1, Name = "a" },
            new Row { CategoryId = 2, Name = "b" },
            new Row { CategoryId = 3, Name = "c" },
            new Row { CategoryId = null, Name = "none" },
        }.AsQueryable();

        [TestMethod("WhereIn - null values applies no filter")]
        public void WhereIn_NullValues_NoFilter()
        {
            var result = Rows().WhereIn(x => x.CategoryId, null).ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod("WhereIn - filters to matching values")]
        public void WhereIn_FiltersToMatching()
        {
            var result = Rows().WhereIn(x => x.CategoryId, new List<long?> { 1, 3 }).ToList();

            CollectionAssert.AreEquivalent(new long?[] { 1, 3 }, result.Select(x => x.CategoryId).ToList());
        }

        [TestMethod("WhereIn - empty list filters out everything")]
        public void WhereIn_EmptyList_FiltersAll()
        {
            var result = Rows().WhereIn(x => x.CategoryId, new List<long?>()).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod("WhereIn - null in values includes null-keyed rows")]
        public void WhereIn_NullInValues_IncludesNullKeys()
        {
            var result = Rows().WhereIn(x => x.CategoryId, new List<long?> { 2, null }).ToList();

            CollectionAssert.AreEquivalent(new long?[] { 2, null }, result.Select(x => x.CategoryId).ToList());
        }

        [TestMethod("WhereAccessible - wildcard returns the query unchanged")]
        public void WhereAccessible_Wildcard_ReturnsAll()
        {
            var accessible = new AccessibleItemsResult(true, new List<string>());

            var result = Rows().WhereAccessible(accessible, x => x.CategoryId, long.Parse).ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod("WhereAccessible - filters to converted ids and maps EmptyOrNullKey to null")]
        public void WhereAccessible_FiltersAndMapsEmptyOrNull()
        {
            var accessible = new AccessibleItemsResult(
                false,
                new List<string> { "1", "2", TypeAuthContext.EmptyOrNullKey });

            var result = Rows().WhereAccessible(accessible, x => x.CategoryId, long.Parse).ToList();

            CollectionAssert.AreEquivalent(new long?[] { 1, 2, null }, result.Select(x => x.CategoryId).ToList());
        }

        [TestMethod("WhereAccessible - end to end with GetReadableItems")]
        public void WhereAccessible_EndToEnd()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _1 = new List<Access> { Access.Read },
                            _2 = new List<Access> { Access.Read },
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            var readable = typeAuth.GetReadableItems(DataLevel.Departments);

            var result = Rows()
                .WhereAccessible(readable, x => x.CategoryId, x => long.Parse(x.TrimStart('_')))
                .ToList();

            CollectionAssert.AreEquivalent(new long?[] { 1, 2 }, result.Select(x => x.CategoryId).ToList());
        }

        [TestMethod("WhereAccessible - wildcard from access tree returns all rows")]
        public void WhereAccessible_WildcardFromAccessTree()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new List<Access> { Access.Read }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            var readable = typeAuth.GetReadableItems(DataLevel.Departments);

            var result = Rows()
                .WhereAccessible(readable, x => x.CategoryId, x => long.Parse(x.TrimStart('_')))
                .ToList();

            Assert.IsTrue(readable.WildCard);
            Assert.AreEqual(4, result.Count);
        }
    }
}
