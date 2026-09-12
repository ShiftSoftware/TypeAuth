using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Tests;

[TestClass]
public class AccessTreeComparerTests
{
    [TestMethod]
    [DataRow(null, "{}")]
    [DataRow(" \r\n", "null")]
    [DataRow("null", "{ }")]
    [DataRow("{\"a\":[\"r\"],\"b\":[\"w\"]}", "{ \"b\": [\"w\"], \"a\": [\"r\"] }")]
    [DataRow("[\"r\",\"w\",\"r\"]", "[\"w\",\"r\"]")]
    [DataRow("[1,2,3,4]", "[\"m\",\"d\",\"w\",\"r\"]")]
    [DataRow("[\"Read\",\"Write\"]", "[\"r\",\"w\"]")]
    [DataRow("{\"Records\":{\"42\":[\"r\",\"r\"],\"43\":[\"w\"]}}", "{\"Records\":{\"43\":[2],\"42\":[1]}}")]
    [DataRow("{\"Limit\":10}", "{\"Limit\":10.0}")]
    [DataRow("{\"Limit\":10.0}", "{\"Limit\":1e1}")]
    [DataRow("{\"Text\":\"east\"}", "{\"Text\":\"e\\u0061st\"}")]
    [DataRow("{broken", "{broken")]
    public void EquivalentStoredGrants(string? first, string? second)
    {
        Assert.IsTrue(AccessTreeComparer.Equivalent(first, second));
        Assert.IsTrue(AccessTreeComparer.Equivalent(second, first));
    }

    [TestMethod]
    [DataRow("[\"r\"]", "[\"w\"]")]
    [DataRow("[\"r\"]", "[\"r\",\"w\"]")]
    [DataRow("{\"Records\":{\"42\":[\"r\"]}}", "{}")]
    [DataRow("{\"Records\":{\"42\":[\"r\"]}}", "{\"Records\":{\"43\":[\"r\"]}}")]
    [DataRow("{\"Records\":[\"r\"]}", "{\"Records\":{\"42\":[\"r\"]}}")]
    [DataRow("{\"Unknown\":[\"r\"]}", "{}")]
    [DataRow("{\"Users\":[\"r\"]}", "{\"users\":[\"r\"]}")]
    [DataRow("{\"Limit\":10}", "{\"Limit\":11}")]
    [DataRow("{\"Limit\":10}", "{\"Limit\":\"10\"}")]
    [DataRow("{\"Text\":\"east\"}", "{\"Text\":\"west\"}")]
    [DataRow("{\"Text\":\"2026-09-12T00:00:00Z\"}", "{\"Text\":\"2026-09-12T00:00:00+00:00\"}")]
    [DataRow("{\"Limit\":9007199254740993}", "{\"Limit\":9007199254740992.0}")]
    [DataRow("{\"Limit\":1.0000000000000002}", "{\"Limit\":1.0000000000000004}")]
    [DataRow("{\"Limit\":1e-100}", "{\"Limit\":0}")]
    [DataRow("{\"Node\":null}", "{}")]
    [DataRow("{\"Node\":{}}", "{}")]
    [DataRow("{\"Node\":[]}", "{}")]
    [DataRow("[]", "{}")]
    [DataRow("[\"unknown\",\"r\"]", "[\"r\",\"unknown\"]")]
    [DataRow("[\"r\",null]", "[\"r\"]")]
    [DataRow("[2147483648]", "[\"r\"]")]
    [DataRow("[9999999999999999999999999999999999999999999]", "[\"r\"]")]
    [DataRow("{broken", "{}")]
    [DataRow("{broken", "{broken ")]
    [DataRow("{} {}", "{}")]
    [DataRow("{\"Users\":[\"r\"],\"Users\":[\"w\"]}", "{\"Users\":[\"w\"]}")]
    public void DifferentStoredGrants(string first, string second)
    {
        Assert.IsFalse(AccessTreeComparer.Equivalent(first, second));
        Assert.IsFalse(AccessTreeComparer.Equivalent(second, first));
    }

    [TestMethod]
    public void GeneratedTreePreservesNormalizedStaticDynamicAndDecimalGrants()
    {
        const string stored = "{\"ComparisonActions\":{\"Users\":[2,1,1],\"Records\":{\"42\":[1,1]},\"Limit\":10.0}}";
        var context = new TypeAuthContext(stored, typeof(ComparisonActions));
        var generated = context.GenerateAccessTree(context);

        Assert.IsTrue(context.CanWrite(ComparisonActions.Users));
        Assert.IsTrue(context.CanRead(ComparisonActions.Records, "42"));
        Assert.AreEqual(10m, context.AccessValue(ComparisonActions.Limit));
        Assert.IsTrue(AccessTreeComparer.Equivalent(stored, generated));
    }

    [TestMethod]
    public void StoredWildcardRemainsDifferentFromCurrentlyEquivalentExplicitGrants()
    {
        const string wildcard = "{\"ComparisonActions\":{\"Records\":[\"r\"]}}";
        const string explicitGrant = "{\"ComparisonActions\":{\"Records\":{\"42\":[\"r\"]}}}";
        var wildcardContext = new TypeAuthContext(wildcard, typeof(ComparisonActions));
        var explicitContext = new TypeAuthContext(explicitGrant, typeof(ComparisonActions));

        Assert.IsTrue(wildcardContext.CanRead(ComparisonActions.Records, "42"));
        Assert.IsTrue(explicitContext.CanRead(ComparisonActions.Records, "42"));
        Assert.IsTrue(wildcardContext.CanRead(ComparisonActions.Records, "43"));
        Assert.IsFalse(explicitContext.CanRead(ComparisonActions.Records, "43"));
        Assert.IsFalse(AccessTreeComparer.Equivalent(wildcard, explicitGrant));
    }

    public class ComparisonActions
    {
        public static readonly ReadWriteDeleteAction Users = new("Users");
        public static readonly DynamicReadWriteDeleteAction Records = new("Records");
        public static readonly DecimalAction Limit = new("Limit", minimumAccess: 0, maximumAccess: 100);
    }
}
