using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Tests;

/// <summary>
/// The cases of the delegated-administration review's harness (.shift/repos/shift-identity/delegated-administration-review.md),
/// with their IDs. The calls mirror ShiftIdentity's:
/// <list type="bullet">
/// <item>save = Build(submitted).GenerateAccessTree(saver, stored)</item>
/// <item>assign = saver.FindInAccessibleActionsOn(Build(tree))</item>
/// </list>
/// The saver context is built from the saver's trees (own tree and assigned trees), as the request's ITypeAuthService is.
/// <para>
/// The TextCombine plan (.shift/repos/typeauth/text-combine-plan.md) fixed 20 of them in Work 2; their names say the fixed
/// behavior and their comments say why. The text actions use the plan's rules: a Pick (CmpText), a union list (PathsUnion)
/// and a path-aware list (Paths), as the hosts' File Explorer paths will.
/// </para>
/// </summary>
[TestClass]
public class DelegatedAdministrationTests
{
    private const string Passes = "PASSES (no error)";

    private static TypeAuthContext Build(params string[] trees) => new TypeAuthContext(trees.ToList(), typeof(App));

    private static string Save(string submitted, string[] saver, string? stored = null)
        => Build(submitted).GenerateAccessTree(Build(saver), stored == null ? null : Build(stored));

    // The save report: what a save would leave out (dropped) and keep (kept) against what was submitted.
    private static string Report(string submitted, string[] saver, string? stored = null)
    {
        var report = Build(submitted).GenerateAccessTreeWithReport(Build(saver), stored == null ? null : Build(stored));

        Assert.AreEqual(Save(submitted, saver, stored), report.AccessTree);

        if (report.IsAsSubmitted)
            return "AS SUBMITTED";

        return string.Join(" | ", new[]
        {
            report.Dropped.Count > 0 ? "DROPPED: " + string.Join("; ", report.Dropped.Select(x => $"{x.Name}={x.Description}")) : null,
            report.Kept.Count > 0 ? "KEPT: " + string.Join("; ", report.Kept.Select(x => $"{x.Name}={x.Description}")) : null,
        }.Where(x => x != null));
    }

    // As the dashboard posts a tree: the user's stored tree with the admin's edits, written as it is.
    private static string Posted(string stored, Action<TypeAuthContext> edit)
    {
        var view = Build(stored);
        edit(view);
        return view.GenerateAccessTree(view);
    }

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

    // A wildcard is given only by the same wildcard: holding every action of Sales isn't holding Sales, whose wildcard
    // also covers actions added later. 1.x wrote the leaves the saver holds.
    [TestMethod]
    public void G1_GroupWildcardNotHeldAsAWildcardGivesNothing()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"Sales":["r","w","d","m"]}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"],"Quotes":["r","w","d","m"]}}}""" }));
    }

    // And what is stored beneath it and beyond the saver stays: the group is left as stored.
    [TestMethod]
    public void G1_GroupWildcardNotHeldLeavesStoredGrantsTheSaverCannotGiveBack()
    {
        Assert.AreEqual("""{"App":{"Sales":{"Orders":["r","w","d"]}}}""",
            Save("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":{"Orders":["r"]}}}""" }, """{"App":{"Sales":{"Orders":["r","w","d"]}}}"""));
    }

    // A wildcard is given only by the same wildcard: the saver holds Delete on Orders, not on Sales, so the wildcard is
    // cut to the saver's own. Not written as leaves (owner, 17 October); the dashboard doesn't offer this tick either.
    [TestMethod]
    public void G2_GroupWildcardPartlyHeldIsCutToTheSaversWildcard()
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

    // The saver holds Read on Sales, so they can write it, but it would drop Write and Delete on Orders, which they
    // couldn't give back: the group is left as stored, as W3 leaves a dynamic action. 1.x wrote ["r"].
    [TestMethod]
    public void G4_SubmittedGroupWildcardOverStoredLeavesTheSaverCannotGiveBackLeavesThemAsStored()
    {
        Assert.AreEqual("""{"App":{"Sales":{"Orders":["r","w","d"]}}}""",
            Save("""{"App":{"Sales":["r"]}}""", new[] { """{"App":{"Sales":["r"]}}""" }, """{"App":{"Sales":{"Orders":["r","w","d"]}}}"""));
    }

    // As the dashboard does it: the Read tick on Sales sets the node's wildcard in place, the client reduces against the
    // admin and posts the result, and the server saves it with the stored tree as the preserver.
    [TestMethod]
    public void G4_ThroughTheDashboardLeavesTheGroupAsStored()
    {
        const string stored = """{"App":{"Sales":{"Orders":["r","w","d"]}}}""";
        var admin = Build("""{"App":{"Sales":["r"]}}""");
        var view = Build(stored);

        view.FindActionTreeNode("App.Sales")!.WildCardAccess.Add(Access.Read);
        var posted = view.GenerateAccessTree(admin);

        Assert.AreEqual("""{"App":{"Sales":["r"]}}""", posted);
        Assert.AreEqual(stored, Build(posted).GenerateAccessTree(admin, Build(stored)));
    }

    // Nothing stored beneath it is beyond the saver, so the wildcard is written.
    [TestMethod]
    public void G4_SubmittedGroupWildcardOverStoredLeavesTheSaverHoldsIsWritten()
    {
        Assert.AreEqual("""{"App":{"Sales":["r","w"]}}""",
            Save("""{"App":{"Sales":["r","w"]}}""", new[] { """{"App":{"Sales":["r","w"]}}""" }, """{"App":{"Sales":{"Orders":["r","w"]}}}"""));
    }

    // The same for a text value beneath the wildcard.
    [TestMethod]
    public void G4_SubmittedWildcardOverAStoredValueTheSaverCannotGiveBackLeavesItAsStored()
    {
        Assert.AreEqual("""{"App":{"Discount":90.0}}""",
            Save("""{"App":["r"]}""", new[] { """{"App":["r"]}""", """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    // As G1: holding every action of Sales isn't holding the Sales wildcard. 1.x passed.
    [TestMethod]
    public void G5_AssignGroupWildcardHeldOnlyLeafByLeafIsRejected()
    {
        Assert.AreEqual("REJECTED: Orders=Read, Write, Delete (from a group wildcard you don't hold); Quotes=Read, Write, Delete (from a group wildcard you don't hold)",
            Assign("""{"App":{"Sales":["r","w","d","m"]}}""", new[] { """{"App":{"Sales":{"Orders":["r","w","d","m"],"Quotes":["r","w","d","m"]}}}""" }));
    }

    // Rejected as in 1.x; Orders now shows too, since its accesses would come only from the Sales wildcard.
    [TestMethod]
    public void G6_AssignGroupWildcardPartlyHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Orders=Read, Write (from a group wildcard you don't hold); Quotes=Read, Write",
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

    // Row B is beyond the saver's self row.
    [TestMethod]
    public void D5_AssignRowsBeyondSaverSelfRowIsRejected()
    {
        Assert.AreEqual("REJECTED: Companies=B: Read, Write, Delete",
            Assign("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w","d"]}}}}""" }));
    }

    // The saver holds no Companies grant at all.
    [TestMethod]
    public void D6_AssignRowsWithNoSaverGrantIsRejected()
    {
        Assert.AreEqual("REJECTED: Companies=B: Read, Write, Delete", Assign("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""", new[] { """{}""" }));
    }

    // ---------- Assignment uses the save's rule (TextCombine Work 2) ----------

    [TestMethod]
    public void AssignSelfRowNotHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Companies=Self: Read",
            Assign("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"A":["r","w","d"]}}}}""" }));
    }

    [TestMethod]
    public void AssignSelfRowHeldPasses()
    {
        Assert.AreEqual(Passes,
            Assign("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r","w"]}}}}""" }));
    }

    [TestMethod]
    public void AssignEmptyOrNullRowNotHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Companies=Unassigned: Read",
            Assign("""{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_empty_or_null":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":{"_shift_software_type_auth_core_self_reference":["r"]}}}}""" }));
    }

    [TestMethod]
    public void AssignRowsHeldThroughTheWildcardPasses()
    {
        Assert.AreEqual(Passes,
            Assign("""{"App":{"DLA":{"Companies":{"B":["r"],"_shift_software_type_auth_core_empty_or_null":["r"]}}}}""", new[] { """{"App":{"DLA":{"Companies":["r"]}}}""" }));
    }

    // As G2: Delete on Sales as a whole isn't held. Orders's Delete is held, but not as part of the Sales wildcard.
    [TestMethod]
    public void AssignGroupWildcardHeldOnlyInPartIsRejected()
    {
        Assert.AreEqual("REJECTED: Orders=Delete (from a group wildcard you don't hold); Quotes=Delete",
            Assign("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":["r","w"]}}""", """{"App":{"Sales":{"Orders":["d"]}}}""" }));
    }

    // Holding Delete on every action of Sales isn't holding it on Sales: the wildcard would also cover actions added later.
    [TestMethod]
    public void AssignGroupWildcardHeldOnEveryActionButNotAsAWildcardIsRejected()
    {
        Assert.AreEqual("REJECTED: Orders=Delete (from a group wildcard you don't hold); Quotes=Delete (from a group wildcard you don't hold)",
            Assign("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":["r","w"]}}""", """{"App":{"Sales":{"Orders":["d"],"Quotes":["d"]}}}""" }));
    }

    // ---------- The save report (what a save dropped and kept) ----------

    [TestMethod]
    public void ReportWithinReachIsAsSubmitted()
    {
        Assert.AreEqual("AS SUBMITTED", Report("""{"App":{"Tickets":["r"]}}""", new[] { """{"App":{"Tickets":["r","w"]}}""" }));
    }

    [TestMethod]
    public void ReportDropsWhatTheSaverCannotGrant()
    {
        Assert.AreEqual("DROPPED: Orders=Delete (from a group wildcard you don't hold); Quotes=Delete",
            Report("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":["r","w"]}}""", """{"App":{"Sales":{"Orders":["d"]}}}""" }));
        Assert.AreEqual("DROPPED: Paths=You can't grant /", Report("""{"App":{"Paths":"/"}}""", new[] { """{}""" }));
    }

    // What the stored tree has already is not dropped: a saver holding 20 leaves the stored 90 as it is.
    [TestMethod]
    public void ReportOfAnUnchangedValueBeyondTheSaverIsAsSubmitted()
    {
        Assert.AreEqual("AS SUBMITTED", Report("""{"App":{"Discount":90}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    [TestMethod]
    public void ReportKeepsWhatTheSaverCannotGiveBack()
    {
        Assert.AreEqual("KEPT: Discount=You can't lower 90",
            Report("""{"App":{"Discount":10}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
        Assert.AreEqual("KEPT: PathsUnion=You can't remove /",
            Report("""{}""", new[] { """{"App":{"PathsUnion":"/{company}"}}""" }, """{"App":{"PathsUnion":"/"}}"""));
        Assert.AreEqual("DROPPED: Companies=Read (not applied: it can't be saved next to what you can't remove) | KEPT: Companies=B: Write, Delete",
            Report("""{"App":{"DLA":{"Companies":["r"]}}}""", new[] { """{"App":{"DLA":{"Companies":["r"]}}}""" }, """{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}"""));
        Assert.AreEqual("DROPPED: Quotes=Read (not applied: it can't be saved next to what you can't remove) | KEPT: Orders=Write, Delete",
            Report("""{"App":{"Sales":["r"]}}""", new[] { """{"App":{"Sales":["r"]}}""" }, """{"App":{"Sales":{"Orders":["r","w","d"]}}}"""));
    }

    // G1 over a stored grant: the group is left as stored, and what the saver couldn't grant of the wildcard is dropped.
    [TestMethod]
    public void ReportOfAGroupWildcardNotHeldDropsWhatIsNotStored()
    {
        Assert.AreEqual("DROPPED: Quotes=Read, Write, Delete",
            Report("""{"App":{"Sales":["r","w","d"]}}""", new[] { """{"App":{"Sales":{"Orders":["r"]}}}""" }, """{"App":{"Sales":{"Orders":["r","w","d"]}}}"""));
    }

    // The dashboard today reduces the tree against the admin before posting it, which leaves out what the admin can't
    // touch: a host that refuses inexact saves would refuse every such save. Posted as it is, the untouched grants are kept.
    [TestMethod]
    public void ReportOfADashboardSaveLeavingGrantsBeyondTheAdminUntouched()
    {
        const string stored = """{"App":{"Tickets":["r","w","d"],"Discount":90}}""";
        var admin = new[] { """{"App":{"Tickets":["r"],"Discount":20}}""" };

        var clientReduced = Build(stored).GenerateAccessTree(Build(admin));
        Assert.AreEqual("KEPT: Tickets=Write, Delete; Discount=You can't lower 90", Report(clientReduced, admin, stored));

        Assert.AreEqual("AS SUBMITTED", Report(Posted(stored, _ => { }), admin, stored));
    }

    // G4 through the dashboard, posted as it is: the Read tick on Sales would remove Write and Delete on Orders.
    [TestMethod]
    public void ReportOfTheDashboardsG4TickKeepsWhatItWouldRemove()
    {
        const string stored = """{"App":{"Sales":{"Orders":["r","w","d"]}}}""";
        var posted = Posted(stored, view => view.FindActionTreeNode("App.Sales")!.WildCardAccess.Add(Access.Read));

        Assert.AreEqual("""{"App":{"Sales":["r"]}}""", posted);
        Assert.AreEqual("DROPPED: Quotes=Read (not applied: it can't be saved next to what you can't remove) | KEPT: Orders=Write, Delete", Report(posted, new[] { """{"App":{"Sales":["r"]}}""" }, stored));
    }

    // An admin holding Discount 20 edits a tree with Discount 90. With the discount left as it is, they can change what they
    // hold; any change to the discount is refused, as they couldn't put 90 back. The dashboard's text field caps what is
    // typed at the admin's own value, so 95 and 80 reach the save as 20.
    [TestMethod]
    public void ReportOfAValueBeyondTheAdminRefusesEveryChangeToIt()
    {
        const string stored = """{"App":{"Discount":90}}""";
        var admin = new[] { """{"App":{"Tickets":["r"],"Discount":20}}""" };

        Assert.AreEqual("AS SUBMITTED", Report("""{"App":{"Tickets":["r"],"Discount":90}}""", admin, stored));
        Assert.AreEqual("DROPPED: Discount=Maximum allowed value is 20. You can't grant 95", Report("""{"App":{"Discount":95}}""", admin, stored));

        foreach (var lowered in new[] { """{"App":{"Discount":80}}""", """{"App":{"Discount":20}}""", """{"App":{"Discount":15}}""", """{}""" })
            Assert.AreEqual("KEPT: Discount=You can't lower 90", Report(lowered, admin, stored), lowered);

        // Through the dashboard, posted as it is.
        Assert.AreEqual("AS SUBMITTED", Report(Posted(stored, view => view.ToggleAccess(App.Tickets, Access.Read)), admin, stored));

        foreach (var typed in new[] { "95", "80", "20", "15" })
        {
            var posted = Posted(stored, view => view.SetAccessValue(App.Discount, typed, Build(admin).AccessValue((TextAction)App.Discount)));

            Assert.AreEqual(Math.Min(decimal.Parse(typed), 20), Build(posted).AccessValue(App.Discount), typed);
            Assert.AreEqual("KEPT: Discount=You can't lower 90", Report(posted, admin, stored), typed);
        }
    }

    // So the dashboard makes such a field read-only: the value can be changed only when it is within the admin's.
    [TestMethod]
    public void AValueBeyondTheAdminCannotBeChanged()
    {
        var admin = Build("""{"App":{"Discount":20,"Paths":"/a","DLA":{"RowDiscount":{"B":20,"C":20}}}}""");
        var discount = admin.AccessValue((TextAction)App.Discount);
        var paths = admin.AccessValue(App.Paths);

        Assert.IsFalse(Build("""{"App":{"Discount":90}}""").CanChangeAccessValue(App.Discount, discount));
        Assert.IsTrue(Build("""{"App":{"Discount":20}}""").CanChangeAccessValue(App.Discount, discount));
        Assert.IsTrue(Build("""{"App":{"Discount":15}}""").CanChangeAccessValue(App.Discount, discount));
        Assert.IsTrue(Build("""{}""").CanChangeAccessValue(App.Discount, discount));
        Assert.IsFalse(Build("""{"App":{"Discount":15}}""").CanChangeAccessValue(App.Discount, null));

        Assert.IsFalse(Build("""{"App":{"Paths":"/a|/b"}}""").CanChangeAccessValue(App.Paths, paths));
        Assert.IsTrue(Build("""{"App":{"Paths":"/a/x"}}""").CanChangeAccessValue(App.Paths, paths));

        var rows = Build("""{"App":{"DLA":{"RowDiscount":{"B":90,"C":15}}}}""");
        Assert.IsFalse(rows.CanChangeAccessValue(App.DLA.RowDiscount, "B", admin.AccessValue((DynamicTextAction)App.DLA.RowDiscount, "B")));
        Assert.IsTrue(rows.CanChangeAccessValue(App.DLA.RowDiscount, "C", admin.AccessValue((DynamicTextAction)App.DLA.RowDiscount, "C")));
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

    // The stored rows hold accesses the saver can't grant, so the action is left as stored.
    [TestMethod]
    public void W3_SubmittedDynamicWildcardLeavesStoredRowsTheSaverCannotGrant()
    {
        Assert.AreEqual("""{"App":{"DLA":{"Companies":{"B":["r","w","d"]}}}}""",
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

    // The saver couldn't have granted 90, so the stored value is kept.
    [TestMethod]
    public void C2_DecimalUnchangedNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"Discount":90.0}}""",
            Save("""{"App":{"Discount":90}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    // A saver holding 20 can't lower what they couldn't have granted.
    [TestMethod]
    public void C3_DecimalNotLoweredBySaverHoldingLess()
    {
        Assert.AreEqual("""{"App":{"Discount":90.0}}""",
            Save("""{"App":{"Discount":10}}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
    }

    // Nor remove it.
    [TestMethod]
    public void C4_DecimalNotRemovedBySaverHoldingLess()
    {
        Assert.AreEqual("""{"App":{"Discount":90.0}}""", Save("""{}""", new[] { """{"App":{"Discount":20}}""" }, """{"App":{"Discount":90}}"""));
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

    // The preserver works per row: the stored row is kept.
    [TestMethod]
    public void DD2_DynamicDecimalUnchangedRowNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"DLA":{"RowDiscount":{"B":90.0}}}}""",
            Save("""{"App":{"DLA":{"RowDiscount":{"B":90}}}}""", new[] { """{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20}}}}""" }, """{"App":{"DLA":{"RowDiscount":{"B":90}}}}"""));
    }

    // Row B 90 is beyond the saver, whose self row doesn't give row B.
    [TestMethod]
    public void DD3_AssignDynamicDecimalRowBeyondSaverIsRejected()
    {
        Assert.AreEqual("REJECTED: RowDiscount=B: Maximum allowed value is 0. You can't grant 90",
            Assign("""{"App":{"DLA":{"RowDiscount":{"B":90}}}}""", new[] { """{"App":{"DLA":{"RowDiscount":{"_shift_software_type_auth_core_self_reference":20}}}}""" }));
    }

    // The saver holds no RowDiscount.
    [TestMethod]
    public void DD4_AssignDynamicDecimalWildcardNotHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: RowDiscount=Maximum allowed value is 0. You can't grant 90", Assign("""{"App":{"DLA":{"RowDiscount":90}}}""", new[] { """{}""" }));
    }

    // ---------- Text, Pick (was: comparer) ----------

    [TestMethod]
    public void X1_TextComparerBeyondSaverIsClamped()
    {
        Assert.AreEqual("""{"App":{"CmpText":"20"}}""", Save("""{"App":{"CmpText":"50"}}""", new[] { """{"App":{"CmpText":"20"}}""" }));
    }

    // The saver couldn't have granted "90", so the stored value is kept.
    [TestMethod]
    public void X2_TextPickUnchangedNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"CmpText":"90"}}""",
            Save("""{"App":{"CmpText":"90"}}""", new[] { """{"App":{"CmpText":"20"}}""" }, """{"App":{"CmpText":"90"}}"""));
    }

    // "90" is beyond the saver's "20".
    [TestMethod]
    public void X3_AssignTextPickBeyondSaverIsRejected()
    {
        Assert.AreEqual("REJECTED: CmpText=Maximum allowed value is 20. You can't grant 90", Assign("""{"App":{"CmpText":"90"}}""", new[] { """{"App":{"CmpText":"20"}}""" }));
    }

    // ---------- Text, union list (was: merger) ----------

    // "/" is dropped, since the saver's "/{company}" doesn't cover it.
    [TestMethod]
    public void M1_TextUnionBeyondSaverIsDropped()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"PathsUnion":"/"}}""", new[] { """{"App":{"PathsUnion":"/{company}"}}""" }));
    }

    [TestMethod]
    public void M2_TextMergerUnchangedNotHeldIsKept()
    {
        Assert.AreEqual("""{"App":{"PathsUnion":"/"}}""",
            Save("""{"App":{"PathsUnion":"/"}}""", new[] { """{"App":{"PathsUnion":"/{company}"}}""" }, """{"App":{"PathsUnion":"/"}}"""));
    }

    // The saver never held "/", so the stored value is kept.
    [TestMethod]
    public void M3_TextUnionNotRemovedBySaverWhoNeverHeldIt()
    {
        Assert.AreEqual("""{"App":{"PathsUnion":"/"}}""",
            Save("""{}""", new[] { """{"App":{"PathsUnion":"/{company}"}}""" }, """{"App":{"PathsUnion":"/"}}"""));
    }

    // "/" is beyond the saver's "/{company}".
    [TestMethod]
    public void M4_AssignTextUnionBeyondSaverIsRejected()
    {
        Assert.AreEqual("REJECTED: PathsUnion=You can't grant /", Assign("""{"App":{"PathsUnion":"/"}}""", new[] { """{"App":{"PathsUnion":"/{company}"}}""" }));
    }

    // ---------- Text, path list (was: no rule; File Explorer's paths get this rule) ----------

    // "/" is dropped, since the saver's "/{company}" doesn't cover it.
    [TestMethod]
    public void N1_TextListBeyondSaverIsDropped()
    {
        Assert.AreEqual("""{}""",
            Save("""{"App":{"Paths":"/"}}""", new[] { """{"App":{"Paths":"/{company}"}}""" }));
    }

    // The saver holds nothing to give.
    [TestMethod]
    public void N2_TextListSaverHoldingNothingCannotGrant()
    {
        Assert.AreEqual("""{}""", Save("""{"App":{"Paths":"/"}}""", new[] { """{}""" }));
    }

    // The saver never held "/", so the stored value is kept.
    [TestMethod]
    public void N3_TextListNotRemovedBySaverWhoNeverHeldIt()
    {
        Assert.AreEqual("""{"App":{"Paths":"/"}}""", Save("""{}""", new[] { """{}""" }, """{"App":{"Paths":"/"}}"""));
    }

    // The saver holds nothing.
    [TestMethod]
    public void N4_AssignTextListNotHeldIsRejected()
    {
        Assert.AreEqual("REJECTED: Paths=You can't grant /", Assign("""{"App":{"Paths":"/"}}""", new[] { """{}""" }));
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

    // Every order gives one value: a union, in ordinal order.
    [TestMethod]
    public void TextListGivesOneValueInEveryOrder()
    {
        foreach (var order in Orders("/C", "/A", "/B"))
        {
            var context = Build(order.Select(value => $$$"""{"App":{"Paths":"{{{value}}}"}}""").ToArray());

            Assert.AreEqual("/A|/B|/C", context.AccessValue(App.Paths), string.Join(", ", order));
        }
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

    private static bool PathCovers(string held, string entry) => held == entry || entry.StartsWith(held.TrimEnd('/') + "/", StringComparison.Ordinal);

    // The harness's action tree.
    [ActionTree("App", "")]
    public class App
    {
        public static readonly ReadWriteDeleteAction Tickets = new("Tickets");
        public static readonly BooleanAction Export = new("Export");
        public static readonly DecimalAction Discount = new("Discount", null, 0, 100);

        public static readonly TextAction CmpText = new("CmpText", TextCombine.Pick((a, b) =>
        {
            var xs = new[] { a, b }.Where(x => x != null).Select(x => int.Parse(x!)).ToList();
            return xs.Count > 0 ? xs.Max().ToString() : null;
        }), null, "0", "100");

        public static readonly TextAction PathsUnion = new("PathsUnion", TextCombine.UnionList("|"), null, "!", "/");

        // As a host's File Explorer paths: a path covers itself and every path under it.
        public static readonly TextAction Paths = new("Paths", TextCombine.List("|", PathCovers), null, "!", "/");

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
            public static readonly DynamicTextAction RowPaths = new("RowPaths", TextCombine.List("|", PathCovers), null, "!", "/");
        }
    }
}
