using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Tests;

/// <summary>
/// The cases of the delegated-administration review's harness (.shift/repos/shift-identity/delegated-administration-review.md),
/// with their IDs, asserting today's behavior. The calls mirror ShiftIdentity's:
/// <list type="bullet">
/// <item>save = Build(submitted).GenerateAccessTree(saver, stored)</item>
/// <item>assign = saver.FindInAccessibleActionsOn(Build(tree))</item>
/// </list>
/// The saver context is built from the saver's trees (own tree and assigned trees), as the request's ITypeAuthService is.
/// <para>
/// Tests in the <see cref="FlipsInTextCombine"/> category pin behavior that the TextCombine plan
/// (.shift/repos/typeauth/text-combine-plan.md, Work 2) changes. Each says what the fixed result should be. The rest are
/// regression tests that keep passing.
/// </para>
/// </summary>
[TestClass]
public class DelegatedAdministrationTests
{
    private const string FlipsInTextCombine = "FlipsInTextCombine";
    private const string Passes = "PASSES (no error)";

    private static TypeAuthContext Build(params string[] trees) => new TypeAuthContext(trees.ToList(), typeof(App));

    private static string Save(string submitted, string[] saver, string? stored = null)
        => Build(submitted).GenerateAccessTree(Build(saver), stored == null ? null : Build(stored));

    private static string Assign(string tree, string[] saver)
    {
        var result = Build(saver).FindInAccessibleActionsOn(Build(tree));

        return result.Count == 0 ? Passes : "REJECTED: " + string.Join("; ", result.Select(x => $"{x.Key.Name}={x.Value}"));
    }

    // ---------- Tick list (ReadWriteDeleteAction) ----------

    [TestMethod]
    public void T1_TickListAddBeyondSaverIsLimited()
    {
        Assert.AreEqual("""{"App":{"Tickets":["r"]}}""",
            Save("""{"App":{"Tickets":["r","w","d"]}}""", new[] { """{"App":{"Tickets":["r"]}}""" }));
    }

    [TestMethod]
    public void T2_TickListRemoveHeld()
    {
        Assert.AreEqual("""{"App":{"Tickets":["r"]}}""",
            Save("""{"App":{"Tickets":["r"]}}""", new[] { """{"App":{"Tickets":["r","w"]}}""" }, """{"App":{"Tickets":["r","w"]}}"""));
    }

    [TestMethod]
    public void T3_TickListRemoveNotHeldKeepsWhatSaverCannotGive()
    {
        Assert.AreEqual("""{"App":{"Tickets":["w","d"]}}""",
            Save("""{}""", new[] { """{"App":{"Tickets":["r"]}}""" }, """{"App":{"Tickets":["r","w","d"]}}"""));
    }

    [TestMethod]
    public void T4_TickListUnchangedNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"Tickets":["r","w","d"]}}""",
            Save("""{"App":{"Tickets":["r","w","d"]}}""", new[] { """{"App":{"Tickets":["r"]}}""" }, """{"App":{"Tickets":["r","w","d"]}}"""));
    }

    [TestMethod]
    public void T5_AssignTickListBeyondSaverIsRejected()
    {
        Assert.AreEqual("REJECTED: Tickets=Write, Delete",
            Assign("""{"App":{"Tickets":["r","w","d"]}}""", new[] { """{"App":{"Tickets":["r"]}}""" }));
    }

    // ---------- Boolean ----------

    [TestMethod]
    public void B1_BooleanAddNotHeldIsDropped()
    {
        Assert.AreEqual("""{}""", Save("""{"App":{"Export":["m"]}}""", new[] { """{}""" }));
    }

    [TestMethod]
    public void B2_BooleanRemoveNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"Export":["m"]}}""", Save("""{}""", new[] { """{}""" }, """{"App":{"Export":["m"]}}"""));
    }

    [TestMethod]
    public void B3_BooleanRemoveHeld()
    {
        Assert.AreEqual("""{}""", Save("""{}""", new[] { """{"App":{"Export":["m"]}}""" }, """{"App":{"Export":["m"]}}"""));
    }

    [TestMethod]
    public void B4_AssignBooleanNotHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Export=Maximum", Assign("""{"App":{"Export":["m"]}}""", new[] { """{}""" }));
    }

    // ---------- Group wildcards ----------

    [TestMethod]
    public void G1_GroupWildcardExpandsToLeavesTheSaverHolds()
    {
        Assert.AreEqual("""{"App":{"Sales":{"Orders":["r","w","d","m"],"Quotes":["r","w","d","m"]}}}""",
            Save("""{"App":{"Sales":["r","w","d","m"]}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"],"Quotes":["r","w","d","m"]}}}""" }));
    }

    // Fixed: written as leaves the saver holds, {"App":{"Sales":{"Orders":["r","w","d"],"Quotes":["r","w"]}}}; Delete on Orders is kept.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void G2_GroupWildcardPartlyHeldLosesLeafGrants()
    {
        Assert.AreEqual("""{"App":{"Sales":["r","w"]}}""",
            Save("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":["r","w"]}}""", """{"App":{"Sales":{"Orders":["d"]}}}""" }));
    }

    [TestMethod]
    public void G3_StoredGroupWildcardCannotLoseALeafTheSaverHolds()
    {
        Assert.AreEqual("""{"App":{"Sales":["r","w","d","m"]}}""",
            Save("""{"App":{"Sales":{"Quotes":["r","w","d","m"]}}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"]}}}""" }, """{"App":{"Sales":["r","w","d","m"]}}"""));
    }

    [TestMethod]
    public void G4_SubmittedGroupWildcardOverStoredLeaves()
    {
        Assert.AreEqual("""{"App":{"Sales":["r"]}}""",
            Save("""{"App":{"Sales":["r"]}}""", new[] { """{"App":{"Sales":["r"]}}""" }, """{"App":{"Sales":{"Orders":["r","w","d"]}}}"""));
    }

    [TestMethod]
    public void G5_AssignGroupWildcardHeldLeafByLeafPasses()
    {
        Assert.AreEqual(Passes,
            Assign("""{"App":{"Sales":["r","w","d","m"]}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"],"Quotes":["r","w","d","m"]}}}""" }));
    }

    [TestMethod]
    public void G6_AssignGroupWildcardPartlyHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Quotes=Read, Write",
            Assign("""{"App":{"Sales":["r","w"]}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"]}}}""" }));
    }

    // ---------- Dynamic rows ----------

    [TestMethod]
    public void D1_RowsBeyondSaverAreLimited()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""",
            Save("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"],"B":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }));
    }

    [TestMethod]
    public void D2_RowsAreLimitedToSaverWildcard()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":{"B":["r"]}}}}""",
            Save("""{"App":{"DLA":{"Companies":{"B":["r","w"]}}}}""", new[] { """{"App":{"DLA":{"Companies":["r"]}}}""" }));
    }

    [TestMethod]
    public void D3_RowsNotHeldAreKeptOnRemove()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""",
            Save("""{}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }, """{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}"""));
    }

    [TestMethod]
    public void D4_RowsHeldCanBeRemoved()
    {
        Assert.AreEqual("""{}""",
            Save("""{}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""" }, """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}"""));
    }

    // Fixed: rejected, Companies row B is beyond the saver's self row.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void D5_AssignRowsBeyondSaverSelfRowPasses()
    {
        Assert.AreEqual(Passes,
            Assign("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }));
    }

    // Fixed: rejected, the saver holds no Companies grant at all.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void D6_AssignRowsWithNoSaverGrantPasses()
    {
        Assert.AreEqual(Passes, Assign("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""", new[] { """{}""" }));
    }

    // ---------- Dynamic wildcard vs rows ----------

    [TestMethod]
    public void W1_DynamicWildcardDroppedWhenSaverHoldsOnlyRows()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"DLA":{"Companies":["r"]}}}""", new[] { """{"App":{"DLA":{"Companies":{"A":["r","w","d","m"]}}}}""" }));
    }

    [TestMethod]
    public void W2_SubmittedRowsDroppedUnderStoredDynamicWildcard()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":["r","w"]}}}""",
            Save("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }, """{"App":{"DLA":{"Companies":["r","w"]}}}"""));
    }

    // Fixed: the action is left as stored, {"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}; the stored rows hold accesses the saver can't grant.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void W3_SubmittedDynamicWildcardWipesStoredRows()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":["r"]}}}""",
            Save("""{"App":{"DLA":{"Companies":["r"]}}}""", new[] { """{"App":{"DLA":{"Companies":["r"]}}}""" }, """{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}"""));
    }

    [TestMethod]
    public void W4_AssignDynamicWildcardHeldOnlyAsRowsIsRejected()
    {
        Assert.AreEqual("REJECTED: Companies=Read",
            Assign("""{"App":{"DLA":{"Companies":["r"]}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }));
    }

    // ---------- Self row key ----------

    [TestMethod]
    public void S1_SelfRowNotGrantableFromOwnCompanyRow()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"A":["r","w","d"]}}}}""" }));
    }

    [TestMethod]
    public void S2_SelfRowGrantableFromWildcard()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""",
            Save("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":["r"]}}}""" }));
    }

    [TestMethod]
    public void S3_SelfRowResolvesAgainstTheCheckedUser()
    {
        var tree = Build("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""");

        Assert.IsTrue(tree.Can(App.DLA.Companies, Access.Write, "B", "B"));
        Assert.IsFalse(tree.Can(App.DLA.Companies, Access.Write, "A", "B"));
    }

    // ---------- Decimal ----------

    [TestMethod]
    public void C1_DecimalBeyondSaverIsClamped()
    {
        Assert.AreEqual("""{"App":{"Discount":20.0}}""", Save("""{"App":{"Discount":50}}""", new[] { """{"App":{"Discount":20}}""" }));
    }

    // Fixed: the stored 90 is kept, {"App":{"Discount":90.0}}.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void C2_DecimalUnchangedNotHeldIsLowered()
    {
        Assert.AreEqual("""{"App":{"Discount":20.0}}""",
            Save("""{"App":{"Discount":90}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    // Fixed: the stored 90 is kept, {"App":{"Discount":90.0}}; a saver holding 20 can't lower what they couldn't have granted.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void C3_DecimalLoweredBySaverHoldingLess()
    {
        Assert.AreEqual("""{"App":{"Discount":10.0}}""",
            Save("""{"App":{"Discount":10}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    // Fixed: the stored 90 is kept, {"App":{"Discount":90.0}}.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void C4_DecimalRemovedBySaverHoldingLess()
    {
        Assert.AreEqual("""{}""", Save("""{}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    [TestMethod]
    public void C5_AssignDecimalBeyondSaverIsRejected()
    {
        Assert.AreEqual("REJECTED: Discount=Maximum allowed value is 20. You can't grant 50",
            Assign("""{"App":{"Discount":50}}""", new[] { """{"App":{"Discount":20}}""" }));
    }

    // ---------- Dynamic decimal ----------

    [TestMethod]
    public void DD1_DynamicDecimalRowsAreClamped()
    {
        Assert.AreEqual("""{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20.0}}}}""",
            Save("""{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":50,"B":90}}}}""", new[] { """{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20}}}}""" }));
    }

    // Fixed: the stored row is kept, {"App":{"DLA":{"RowDiscount":{"B":90.0}}}}.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void DD2_DynamicDecimalUnchangedRowNotHeldIsDeleted()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"DLA":{"RowDiscount":{"B":90}}}}""", new[] { """{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20}}}}""" }, """{"App":{"DLA":{"RowDiscount":{"B":90}}}}"""));
    }

    // Fixed: rejected, row B 90 is beyond the saver's self row 20.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void DD3_AssignDynamicDecimalRowBeyondSaverPasses()
    {
        Assert.AreEqual(Passes,
            Assign("""{"App":{"DLA":{"RowDiscount":{"B":90}}}}""", new[] { """{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20}}}}""" }));
    }

    // Fixed: rejected, the saver holds no RowDiscount.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void DD4_AssignDynamicDecimalWildcardNotHeldPasses()
    {
        Assert.AreEqual(Passes, Assign("""{"App":{"DLA":{"RowDiscount":90}}}""", new[] { """{}""" }));
    }

    // ---------- Text with comparer ----------

    [TestMethod]
    public void X1_TextComparerBeyondSaverIsClamped()
    {
        Assert.AreEqual("""{"App":{"CmpText":"20"}}""", Save("""{"App":{"CmpText":"50"}}""", new[] { """{"App":{"CmpText":"20"}}""" }));
    }

    // Fixed: the stored "90" is kept, {"App":{"CmpText":"90"}}.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void X2_TextComparerUnchangedNotHeldIsLowered()
    {
        Assert.AreEqual("""{"App":{"CmpText":"20"}}""",
            Save("""{"App":{"CmpText":"90"}}""", new[] { """{"App":{"CmpText":"20"}}""" }, """{"App":{"CmpText":"90"}}"""));
    }

    // Fixed: rejected, "90" is beyond the saver's "20".
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void X3_AssignTextComparerBeyondSaverPasses()
    {
        Assert.AreEqual(Passes, Assign("""{"App":{"CmpText":"90"}}""", new[] { """{"App":{"CmpText":"20"}}""" }));
    }

    // ---------- Text with merger ----------

    // Fixed: "/" is dropped, since the saver's "/{company}" doesn't cover it.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void M1_TextMergerBeyondSaverIsNotLimited()
    {
        Assert.AreEqual("""{"App":{"PathsMerger":"/"}}""",
            Save("""{"App":{"PathsMerger":"/"}}""", new[] { """{"App":{"PathsMerger":"/{company}"}}""" }));
    }

    [TestMethod]
    public void M2_TextMergerUnchangedNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"PathsMerger":"/"}}""",
            Save("""{"App":{"PathsMerger":"/"}}""", new[] { """{"App":{"PathsMerger":"/{company}"}}""" }, """{"App":{"PathsMerger":"/"}}"""));
    }

    // Fixed: the stored "/" is kept, {"App":{"PathsMerger":"/"}}; the saver never held it.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void M3_TextMergerRemovedBySaverWhoNeverHeldIt()
    {
        Assert.AreEqual("""{}""",
            Save("""{}""", new[] { """{"App":{"PathsMerger":"/{company}"}}""" }, """{"App":{"PathsMerger":"/"}}"""));
    }

    // Fixed: rejected, "/" is beyond the saver's "/{company}".
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void M4_AssignTextMergerBeyondSaverPasses()
    {
        Assert.AreEqual(Passes, Assign("""{"App":{"PathsMerger":"/"}}""", new[] { """{"App":{"PathsMerger":"/{company}"}}""" }));
    }

    // ---------- Text with neither (Work 2 removes this kind: PathsNone gets a rule, and these become cases of that rule) ----------

    // Fixed: "/" is dropped, since the saver's "/{company}" doesn't cover it.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void N1_TextNoRuleBeyondSaverIsNotLimited()
    {
        Assert.AreEqual("""{"App":{"PathsNone":"/"}}""",
            Save("""{"App":{"PathsNone":"/"}}""", new[] { """{"App":{"PathsNone":"/{company}"}}""" }));
    }

    // Fixed: {}, the saver holds nothing to give.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void N2_TextNoRuleSaverHoldingNothingCanGrantMaximum()
    {
        Assert.AreEqual("""{"App":{"PathsNone":"/"}}""", Save("""{"App":{"PathsNone":"/"}}""", new[] { """{}""" }));
    }

    // Fixed: the stored "/" is kept, {"App":{"PathsNone":"/"}}; the saver never held it.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void N3_TextNoRuleRemovedBySaverWhoNeverHeldIt()
    {
        Assert.AreEqual("""{}""", Save("""{}""", new[] { """{}""" }, """{"App":{"PathsNone":"/"}}"""));
    }

    // Fixed: rejected, the saver holds nothing.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void N4_AssignTextNoRuleNotHeldPasses()
    {
        Assert.AreEqual(Passes, Assign("""{"App":{"PathsNone":"/"}}""", new[] { """{}""" }));
    }

    [TestMethod]
    public void N5_RootWildcardHeldIsKept()
    {
        Assert.AreEqual("""{"App":["r"]}""", Save("""{"App":["r"]}""", new[] { """{"App":["r"]}""" }));
    }

    // ---------- Union only ----------

    [TestMethod]
    public void U1_ATreeGrantingNothingCannotNarrow()
    {
        var one = Build("""{"App":{"Tickets":["r","w","d"]}}""");
        var two = Build("""{"App":{"Tickets":["r","w","d"]}}""", """{"App":{"Tickets":[]}}""");

        Assert.IsTrue(one.Can(App.Tickets, Access.Delete));
        Assert.IsTrue(two.Can(App.Tickets, Access.Delete));
    }

    // ---------- Combining a user's trees ----------

    // Fixed: every order gives one value. Work 2 removes the no-rule kind; with File Explorer's union rule this would be "/A|/B|/C" in some fixed order.
    [TestMethod]
    [TestCategory(FlipsInTextCombine)]
    public void TextNoRuleLastTreeWinsInEveryOrder()
    {
        var values = new HashSet<string?>();

        foreach (var order in Orders("/A", "/B", "/C"))
        {
            var context = Build(order.Select(value => $$$"""{"App":{"PathsNone":"{{{value}}}"}}""").ToArray());

            Assert.AreEqual(order.Last(), context.AccessValue(App.PathsNone), string.Join(", ", order));

            values.Add(context.AccessValue(App.PathsNone));
        }

        Assert.AreEqual(3, values.Count);
    }

    private static IEnumerable<string[]> Orders(params string[] items)
    {
        if (items.Length <= 1)
        {
            yield return items;
            yield break;
        }

        for (var i = 0; i < items.Length; i++)
        {
            var first = items[i];

            foreach (var rest in Orders(items.Where((_, index) => index != i).ToArray()))
                yield return new[] { first }.Concat(rest).ToArray();
        }
    }

    // The harness's action tree.
    [ActionTree("App", "")]
    public class App
    {
        public static readonly ReadWriteDeleteAction Tickets = new("Tickets");
        public static readonly BooleanAction Export = new("Export");
        public static readonly DecimalAction Discount = new("Discount", null, 0, 100);

        public static readonly TextAction CmpText = new("CmpText", null, "0", "100", (a, b) =>
        {
            var xs = new[] { a, b }.Where(x => x != null).Select(x => int.Parse(x!)).ToList();
            return xs.Count > 0 ? xs.Max().ToString() : null;
        });

        public static readonly TextAction PathsMerger = new("PathsMerger", null, "!", "/", null, (a, b) => string.Join("|", new[] { a, b }.Where(x => x != null)));
        public static readonly TextAction PathsNone = new("PathsNone", null, "!", "/");

        [ActionTree("Sales", "")]
        public class Sales
        {
            public static readonly ReadWriteDeleteAction Orders = new("Orders");
            public static readonly ReadWriteDeleteAction Quotes = new("Quotes");
        }

        [ActionTree("DLA", "")]
        public class DLA
        {
            public static readonly DynamicReadWriteDeleteAction Companies = new("Companies");
            public static readonly DynamicDecimalAction RowDiscount = new("RowDiscount", null, 0, 100);
        }
    }
}
