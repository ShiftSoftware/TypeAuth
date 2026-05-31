using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Linq;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System;
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

        // --- WhereIn (single column) ---------------------------------------------------------------
        // Call shape: values first, then the column(s) as a params list — q.WhereIn(values, x => x.Col).

        [TestMethod("WhereIn - null values applies no filter")]
        public void WhereIn_NullValues_NoFilter()
        {
            var result = Rows().WhereIn(null, x => x.CategoryId).ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod("WhereIn - filters to matching values")]
        public void WhereIn_FiltersToMatching()
        {
            var result = Rows().WhereIn(new List<long?> { 1, 3 }, x => x.CategoryId).ToList();

            CollectionAssert.AreEquivalent(new long?[] { 1, 3 }, result.Select(x => x.CategoryId).ToList());
        }

        [TestMethod("WhereIn - empty list filters out everything")]
        public void WhereIn_EmptyList_FiltersAll()
        {
            var result = Rows().WhereIn(new List<long?>(), x => x.CategoryId).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod("WhereIn - null in values includes null-keyed rows")]
        public void WhereIn_NullInValues_IncludesNullKeys()
        {
            var result = Rows().WhereIn(new List<long?> { 2, null }, x => x.CategoryId).ToList();

            CollectionAssert.AreEquivalent(new long?[] { 2, null }, result.Select(x => x.CategoryId).ToList());
        }

        // --- WhereAccessible ------------------------------------------------------------------------

        [TestMethod("WhereAccessible - wildcard returns the query unchanged")]
        public void WhereAccessible_Wildcard_ReturnsAll()
        {
            var accessible = new AccessibleItemsResult(true, new List<string>());

            var result = Rows().WhereAccessible(accessible, long.Parse, x => x.CategoryId).ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod("WhereAccessible - filters to converted ids and maps EmptyOrNullKey to null")]
        public void WhereAccessible_FiltersAndMapsEmptyOrNull()
        {
            var accessible = new AccessibleItemsResult(
                false,
                new List<string> { "1", "2", TypeAuthContext.EmptyOrNullKey });

            var result = Rows().WhereAccessible(accessible, long.Parse, x => x.CategoryId).ToList();

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
                .WhereAccessible(readable, x => long.Parse(x.TrimStart('_')), x => x.CategoryId)
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
                .WhereAccessible(readable, x => long.Parse(x.TrimStart('_')), x => x.CategoryId)
                .ToList();

            Assert.IsTrue(readable.WildCard);
            Assert.AreEqual(4, result.Count);
        }

        // --- WhereIn / WhereAccessible (multi-column OR) -------------------------------------------
        // Two-column twin of the cases above: prove the disjunction across columns, the null/empty
        // conventions per column, and the fail-closed no-selectors guard. A single selector is just the
        // n=1 case (covered by the single-column tests above); two+ selectors OR together.

        private class OrRow
        {
            public long? A { get; set; }
            public long? B { get; set; }
            public string Name { get; set; } = "";
        }

        private static IQueryable<OrRow> OrRows() => new List<OrRow>
        {
            new OrRow { A = 1,    B = 2,    Name = "neither" },
            new OrRow { A = 4,    B = 2,    Name = "a-is-4" },
            new OrRow { A = 1,    B = 4,    Name = "b-is-4" },
            new OrRow { A = 4,    B = 4,    Name = "both-4" },
            new OrRow { A = null, B = 2,    Name = "a-null" },
            new OrRow { A = 2,    B = null, Name = "b-null" },
        }.AsQueryable();

        [TestMethod("WhereIn (multi-column) - null values applies no filter")]
        public void WhereIn_MultiColumn_NullValues_NoFilter()
        {
            var result = OrRows().WhereIn(null, x => x.A, x => x.B).ToList();

            Assert.AreEqual(6, result.Count);
        }

        [TestMethod("WhereIn (multi-column) - empty list filters out everything")]
        public void WhereIn_MultiColumn_EmptyList_FiltersAll()
        {
            var result = OrRows().WhereIn(new List<long?>(), x => x.A, x => x.B).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod("WhereIn (multi-column) - matches when any selector is in values (A==4 || B==4)")]
        public void WhereIn_MultiColumn_OrAcrossSelectors()
        {
            var result = OrRows().WhereIn(new List<long?> { 4 }, x => x.A, x => x.B).ToList();

            CollectionAssert.AreEquivalent(
                new[] { "a-is-4", "b-is-4", "both-4" },
                result.Select(x => x.Name).ToList());
        }

        [TestMethod("WhereIn (multi-column) - null in values matches null on any column")]
        public void WhereIn_MultiColumn_NullInValues_MatchesNullOnAnyColumn()
        {
            var result = OrRows().WhereIn(new List<long?> { null }, x => x.A, x => x.B).ToList();

            CollectionAssert.AreEquivalent(
                new[] { "a-null", "b-null" },
                result.Select(x => x.Name).ToList());
        }

        [TestMethod("WhereIn (multi-column) - value or null combine across columns")]
        public void WhereIn_MultiColumn_ValueAndNull_OrAcrossColumns()
        {
            var result = OrRows().WhereIn(new List<long?> { 4, null }, x => x.A, x => x.B).ToList();

            CollectionAssert.AreEquivalent(
                new[] { "a-is-4", "b-is-4", "both-4", "a-null", "b-null" },
                result.Select(x => x.Name).ToList());
        }

        [TestMethod("WhereIn - no selectors throws (fail-closed, no silent all/none)")]
        public void WhereIn_NoSelectors_Throws()
        {
            ArgumentException? caught = null;
            try
            {
                // params with zero columns — the easy "forgot the selectors" mistake; must fail closed.
                _ = OrRows().WhereIn(new List<long?> { 1 });
            }
            catch (ArgumentException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "Expected WhereIn with no selectors to throw ArgumentException.");
        }

        [TestMethod("WhereAccessible (multi-column) - matches accessible id on any column")]
        public void WhereAccessible_MultiColumn_OrAcrossColumns()
        {
            var typeAuth = new TypeAuthContextBuilder()
                .AddAccessTree(JsonSerializer.Serialize(new
                {
                    DataLevel = new
                    {
                        Departments = new
                        {
                            _4 = new List<Access> { Access.Read }
                        }
                    }
                }))
                .AddActionTree<DataLevel>()
                .Build();

            var readable = typeAuth.GetReadableItems(DataLevel.Departments);

            var result = OrRows()
                .WhereAccessible(readable, x => long.Parse(x.TrimStart('_')), x => x.A, x => x.B)
                .Select(x => x.Name).ToList();

            // Accessible id {4}, OR-matched on either column.
            CollectionAssert.AreEquivalent(new[] { "a-is-4", "b-is-4", "both-4" }, result);
        }
    }
}
