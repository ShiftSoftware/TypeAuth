using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Tests;

/// <summary>
/// Every built-in combine rule, as a value and as rows: one value in every order of three trees, and the same rule for
/// reducing a save, preserving a stored value and checking an assignment (.shift/repos/typeauth/text-combine-plan.md).
/// </summary>
[TestClass]
public class TextCombineTests
{
    private const string Passes = "PASSES";

    // Per rule: what the saver holds, a value beyond it, a value within it, the first reduced against the saver, and
    // what is kept of the first when a saver removes it.
    private static readonly Dictionary<string, (string Held, string Beyond, string Within, string Reduced, string Preserved)> Cases = new()
    {
        ["MaxDecimal"] = ("20", "90", "10", "20", "90"),
        ["Pick"] = ("20", "90", "10", "20", "90"),
        ["Union"] = ("a", "a|b", "a", "a", "b"),
        ["Paths"] = ("/x", "/x/y|/z", "/x/y", "/x/y", "/z"),
    };

    // Three trees' values per rule, and their one combined value.
    private static readonly Dictionary<string, (string[] Values, string Combined)> Combined = new()
    {
        ["MaxDecimal"] = (new[] { "20", "90", "50" }, "90"),
        ["Pick"] = (new[] { "20", "90", "50" }, "90"),
        ["Union"] = (new[] { "b", "c|a", "a" }, "a|b|c"),
        ["Paths"] = (new[] { "/x/y", "/z", "/x" }, "/x|/x/y|/z"),
    };

    [TestMethod]
    [DataRow("MaxDecimal", false)]
    [DataRow("MaxDecimal", true)]
    [DataRow("Pick", false)]
    [DataRow("Pick", true)]
    [DataRow("Union", false)]
    [DataRow("Union", true)]
    [DataRow("Paths", false)]
    [DataRow("Paths", true)]
    public void EveryOrderOfThreeTreesGivesOneValue(string rule, bool row)
    {
        var (values, combined) = Combined[rule];

        foreach (var order in Orders(values))
        {
            var context = Build(order.Select(value => Tree(rule, row, value)).ToArray());

            Assert.AreEqual(combined, Value(context, rule, row), string.Join(", ", order));
        }
    }

    [TestMethod]
    [DataRow("MaxDecimal", false)]
    [DataRow("MaxDecimal", true)]
    [DataRow("Pick", false)]
    [DataRow("Pick", true)]
    [DataRow("Union", false)]
    [DataRow("Union", true)]
    [DataRow("Paths", false)]
    [DataRow("Paths", true)]
    public void ReducingKeepsOnlyWhatTheSaverHolds(string rule, bool row)
    {
        var c = Cases[rule];
        var saver = Build(Tree(rule, row, c.Held));

        AssertSameValue(rule, c.Reduced, Value(Build(Build(Tree(rule, row, c.Beyond)).GenerateAccessTree(saver)), rule, row));

        //A value within what the saver holds is written as the tree writes itself.
        var within = Build(Tree(rule, row, c.Within));
        Assert.IsTrue(AccessTreeComparer.Equivalent(within.GenerateAccessTree(within), within.GenerateAccessTree(saver)));

        //A saver holding nothing gives nothing.
        Assert.AreEqual("{}", Build(Tree(rule, row, c.Beyond)).GenerateAccessTree(Build("{}")));
    }

    [TestMethod]
    [DataRow("MaxDecimal", false)]
    [DataRow("MaxDecimal", true)]
    [DataRow("Pick", false)]
    [DataRow("Pick", true)]
    [DataRow("Union", false)]
    [DataRow("Union", true)]
    [DataRow("Paths", false)]
    [DataRow("Paths", true)]
    public void PreservingKeepsWhatTheSaverCouldNotHaveGiven(string rule, bool row)
    {
        var c = Cases[rule];
        var saver = Build(Tree(rule, row, c.Held));

        //Removed by the saver.
        AssertSameValue(rule, c.Preserved, Value(Build(Build("{}").GenerateAccessTree(saver, Build(Tree(rule, row, c.Beyond)))), rule, row));

        //Saved unchanged.
        var unchanged = Build(Tree(rule, row, c.Beyond)).GenerateAccessTree(saver, Build(Tree(rule, row, c.Beyond)));
        Assert.IsTrue(Within(rule, Value(Build(unchanged), rule, row), c.Beyond), unchanged);

        //What the saver could have given, they can remove.
        Assert.AreEqual("{}", Build("{}").GenerateAccessTree(saver, Build(Tree(rule, row, c.Within))));
    }

    [TestMethod]
    [DataRow("MaxDecimal", false)]
    [DataRow("MaxDecimal", true)]
    [DataRow("Pick", false)]
    [DataRow("Pick", true)]
    [DataRow("Union", false)]
    [DataRow("Union", true)]
    [DataRow("Paths", false)]
    [DataRow("Paths", true)]
    public void AssigningIsCheckedWithTheSameRule(string rule, bool row)
    {
        var c = Cases[rule];
        var saver = new[] { Tree(rule, row, c.Held) };

        Assert.AreEqual(Passes, Assign(Tree(rule, row, c.Within), saver));
        Assert.AreEqual(Passes, Assign(Tree(rule, row, c.Held), saver));
        Assert.AreNotEqual(Passes, Assign(Tree(rule, row, c.Beyond), saver));
        Assert.AreNotEqual(Passes, Assign(Tree(rule, row, c.Within), new[] { "{}" }));
    }

    [TestMethod]
    public void AssigningListsWhatTheSaverCannotGrant()
    {
        Assert.AreEqual("REJECTED: Paths=You can't grant /z", Assign(Tree("Paths", false, "/x/y|/z"), new[] { Tree("Paths", false, "/x") }));
        Assert.AreEqual("REJECTED: Row Paths=B: You can't grant /z", Assign(Tree("Paths", true, "/x/y|/z"), new[] { Tree("Paths", true, "/x") }));
        Assert.AreEqual("REJECTED: Pick=Maximum allowed value is 20. You can't grant 90", Assign(Tree("Pick", false, "90"), new[] { Tree("Pick", false, "20") }));
    }

    [TestMethod]
    public void TheMaximumCoversEveryEntry()
    {
        var saver = Build(Tree("Union", false, "*"));

        Assert.AreEqual("""{"Rules":{"Union":"x|y"}}""", Build(Tree("Union", false, "x|y")).GenerateAccessTree(saver));
        Assert.AreEqual("*", Build(Tree("Union", false, "x"), Tree("Union", false, "*")).AccessValue(Rules.Union));
    }

    [TestMethod]
    public void TheMinimumIsNothingInAList()
    {
        Assert.AreEqual("x", Build(Tree("Union", false, "!"), Tree("Union", false, "x")).AccessValue(Rules.Union));
        Assert.AreEqual("!", Build(Tree("Union", false, "!"), Tree("Union", false, "!")).AccessValue(Rules.Union));
        Assert.AreEqual("!", Build("{}").AccessValue(Rules.Union));
    }

    [TestMethod]
    public void EditorReducesListsToTheSaver()
    {
        var context = Build("{}");

        context.SetAccessValue(Rules.Paths, "/x/y|/z", "/x");

        Assert.AreEqual("/x/y", context.AccessValue(Rules.Paths));
    }

    [TestMethod]
    public void DynamicActionWildcardValueIsWritten()
    {
        const string tree = """{"Rules":{"RowMaxDecimal":90}}""";

        Assert.AreEqual("""{"Rules":{"RowMaxDecimal":90.0}}""", Build(tree).GenerateAccessTree(Build(tree)));
        Assert.AreEqual(Passes, Assign(tree, new[] { tree }));
    }

    [TestMethod]
    public void OrderIndependentPassesForEveryBuiltInRule()
    {
        TextCombineAssert.OrderIndependent(Rules.MaxDecimal, "20", "90", "50", "90.0", null);
        TextCombineAssert.OrderIndependent(Rules.Pick, "20", "90", "50", null);
        TextCombineAssert.OrderIndependent(Rules.Union, "b", "c|a", "a", "!", null);
        TextCombineAssert.OrderIndependent(Rules.Paths, "/x/y", "/z", "/x", null);
        TextCombineAssert.OrderIndependent(Rules.RowMaxDecimal, "20", "90", "50");
        TextCombineAssert.OrderIndependent(Rules.RowPaths, "/x/y", "/z", "/x");
    }

    [TestMethod]
    public void OrderIndependentCatchesLastAssignedWins()
    {
        var lastWins = new TextAction("Last Wins", TextCombine.Pick((a, b) => b ?? a));

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => TextCombineAssert.OrderIndependent(lastWins, "/A", "/B", "/C"));

        StringAssert.Contains(exception.Message, "Last Wins");
    }

    [TestMethod]
    public void ARuleIsRequired()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new TextAction("No Rule", (TextCombine)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new DynamicTextAction("No Rule", (TextCombine)null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => TextCombine.Pick(null!));
        Assert.ThrowsExactly<ArgumentException>(() => TextCombine.List("", (_, _) => true));
    }

    [TestMethod]
    public void ObsoleteComparerAndMergerAreDerivedFromTheRule()
    {
#pragma warning disable CS0618
        Assert.IsNotNull(Rules.MaxDecimal.Comparer);
        Assert.IsNull(Rules.MaxDecimal.Merger);
        Assert.AreEqual("90", Rules.MaxDecimal.Comparer!("20", "90"));

        Assert.IsNull(Rules.Union.Comparer);
        Assert.AreEqual("a|b", Rules.Union.Merger!("b", "a|!"));
        Assert.AreEqual("a|b", Rules.RowUnion.Merger!("b", "a"));
#pragma warning restore CS0618
    }

    private static string Tree(string rule, bool row, string value)
    {
        var action = row ? "Row" + rule : rule;
        var json = Newtonsoft.Json.JsonConvert.ToString(value);

        return row
            ? "{\"Rules\":{\"" + action + "\":{\"B\":" + json + "}}}"
            : "{\"Rules\":{\"" + action + "\":" + json + "}}";
    }

    private static string? Value(TypeAuthContext context, string rule, bool row)
    {
        if (row)
            return context.AccessValue(RowAction(rule), "B");

        return context.AccessValue(Action(rule));
    }

    private static void AssertSameValue(string rule, string expected, string? actual)
    {
        Assert.IsTrue(Within(rule, expected, actual) && Within(rule, actual, expected), $"Expected {expected}, got {actual}");
    }

    private static bool Within(string rule, string? held, string? value)
    {
        var action = Action(rule);

        return action.Combine.Within(held, value, action.MinimumAccess, action.MaximumAccess);
    }

    private static TextAction Action(string rule) => rule switch
    {
        "MaxDecimal" => Rules.MaxDecimal,
        "Pick" => Rules.Pick,
        "Union" => Rules.Union,
        _ => Rules.Paths,
    };

    private static DynamicTextAction RowAction(string rule) => rule switch
    {
        "MaxDecimal" => Rules.RowMaxDecimal,
        "Pick" => Rules.RowPick,
        "Union" => Rules.RowUnion,
        _ => Rules.RowPaths,
    };

    private static TypeAuthContext Build(params string[] trees) => new TypeAuthContext(trees.ToList(), typeof(Rules));

    private static string Assign(string tree, string[] saver)
    {
        var result = Build(saver).FindInAccessibleActionsOn(Build(tree));

        return result.Count == 0 ? Passes : "REJECTED: " + string.Join("; ", result.Select(x => $"{x.Key.Name}={x.Value}"));
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

    private static string? MaxInt(string? a, string? b)
    {
        var numbers = new[] { a, b }.Where(x => x != null).Select(x => int.Parse(x!)).ToList();

        return numbers.Count > 0 ? numbers.Max().ToString() : null;
    }

    private static bool PathCovers(string held, string entry) => held == entry || entry.StartsWith(held.TrimEnd('/') + "/", StringComparison.Ordinal);

    [ActionTree("Rules", "")]
    public class Rules
    {
        public static readonly DecimalAction MaxDecimal = new("MaxDecimal", null, 0, 100);
        public static readonly TextAction Pick = new("Pick", TextCombine.Pick(MaxInt), null, "0", "100");
        public static readonly TextAction Union = new("Union", TextCombine.UnionList("|"), null, "!", "*");
        public static readonly TextAction Paths = new("Paths", TextCombine.List("|", PathCovers), null, "!", "/");

        public static readonly DynamicDecimalAction RowMaxDecimal = new("Row MaxDecimal", null, 0, 100);
        public static readonly DynamicTextAction RowPick = new("Row Pick", TextCombine.Pick(MaxInt), null, "0", "100");
        public static readonly DynamicTextAction RowUnion = new("Row Union", TextCombine.UnionList("|"), null, "!", "*");
        public static readonly DynamicTextAction RowPaths = new("Row Paths", TextCombine.List("|", PathCovers), null, "!", "/");
    }
}
