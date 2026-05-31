using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass()]
    public class AccessibleItemsTests
    {
        private static AccessibleItemsResult Set(params string[] ids) => new AccessibleItemsResult(false, new List<string>(ids));
        private static AccessibleItemsResult Wildcard() => new AccessibleItemsResult(true, new List<string>());

        private static AccessibleItemsByAccess ByAccess() => new AccessibleItemsByAccess(
            read: Set("r"),
            write: Set("w"),
            delete: Set("d"),
            maximum: Set("m"));

        // --- AccessibleItemsByAccess.For ----------------------------------------------------------

        [TestMethod("For - selects the set matching the access level")]
        public void For_SelectsMatchingSet()
        {
            var byAccess = ByAccess();

            Assert.AreSame(byAccess.Read, byAccess.For(Access.Read));
            Assert.AreSame(byAccess.Write, byAccess.For(Access.Write));
            Assert.AreSame(byAccess.Delete, byAccess.For(Access.Delete));
            Assert.AreSame(byAccess.Maximum, byAccess.For(Access.Maximum));
        }

        [TestMethod("For - undefined access level throws")]
        public void For_UndefinedAccess_Throws()
        {
            ArgumentException? caught = null;
            try
            {
                _ = ByAccess().For((Access)999);
            }
            catch (ArgumentException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "Expected For with an undefined Access to throw ArgumentOutOfRangeException.");
        }

        // --- AccessibleItemsResult.HasAccessTo<TKey> (row check) ----------------------------------
        // Same string -> TKey converter (long.Parse) as the query path; keys are the entity's (nullable) FK values.

        [TestMethod("HasAccessTo - wildcard grants any key")]
        public void HasAccessTo_Wildcard_AlwaysTrue()
        {
            var wildcard = Wildcard();

            Assert.IsTrue(wildcard.HasAccessTo(long.Parse, 999L));
            Assert.IsTrue(wildcard.HasAccessTo(long.Parse, (long?)null));
        }

        [TestMethod("HasAccessTo - true only when a key is in the accessible set")]
        public void HasAccessTo_IdSet_MatchesMember()
        {
            var set = Set("1", "2");

            Assert.IsTrue(set.HasAccessTo(long.Parse, 2L));
            Assert.IsFalse(set.HasAccessTo(long.Parse, 3L));
        }

        [TestMethod("HasAccessTo - matches when any one key is accessible (OR)")]
        public void HasAccessTo_OrAcrossKeys()
        {
            var set = Set("1", "2");

            Assert.IsTrue(set.HasAccessTo(long.Parse, 3L, 2L));   // 2 is in the set
            Assert.IsFalse(set.HasAccessTo(long.Parse, 3L, 9L));  // neither is
        }

        [TestMethod("HasAccessTo - null key matches only when EmptyOrNullKey is granted")]
        public void HasAccessTo_NullKey_MatchesEmptyOrNullKey()
        {
            var withNull = Set("1", TypeAuthContext.EmptyOrNullKey);
            var withoutNull = Set("1", "2");

            Assert.IsTrue(withNull.HasAccessTo(long.Parse, (long?)null));
            Assert.IsFalse(withoutNull.HasAccessTo(long.Parse, (long?)null));
            Assert.IsTrue(withNull.HasAccessTo(long.Parse, 9L, (long?)null)); // null leg matches via OR
        }

        [TestMethod("HasAccessTo - no keys throws (fail-closed)")]
        public void HasAccessTo_NoKeys_Throws()
        {
            ArgumentException? caught = null;
            try
            {
                // params with zero keys — the easy "forgot the keys" mistake; must fail closed (mirrors WhereIn).
                _ = Set("1").HasAccessTo<long>(long.Parse);
            }
            catch (ArgumentException ex)
            {
                caught = ex;
            }

            Assert.IsNotNull(caught, "Expected HasAccessTo with no keys to throw ArgumentException.");
        }
    }
}
