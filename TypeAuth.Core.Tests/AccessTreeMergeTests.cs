using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Tests;

/// <summary>
/// A context merges its access trees as a union: Can is an Any over the action bank. The wildcards on its nodes and the
/// tree it generates must not depend on the order the trees were added in, and the generated tree must grant what the
/// context grants.
/// </summary>
[TestClass]
public class AccessTreeMergeTests
{
    // Each tree grants a wildcard on the same nodes as the other, with different accesses: a dynamic action, and the
    // root of an action tree class.
    private const string ReadCities = """{"DirectoryActions":{"Rows":{"Cities":["r"]}},"StorageActions":["r","w","d","m"]}""";
    private const string ManageCities = """{"DirectoryActions":{"Rows":{"Cities":["r","w","d","m"]}},"StorageActions":["r"]}""";

    // A wildcard in the first tree, grants beneath it in the others that go beyond it, and rows of one dynamic action
    // that two trees list.
    private const string ReadDirectory = """{"DirectoryActions":["r"]}""";
    private const string ManageUsers = """{"DirectoryActions":{"Rows":{"Cities":["r","w"]},"Users":["r","w","d"],"Discount":25},"StorageActions":{"Buckets":{"9":["r"],"3":["r","w"]}}}""";
    private const string ManageFiles = """{"DirectoryActions":{"Rows":{"Regions":["r","w","d","m"]}},"StorageActions":{"Files":["r","w","d"],"MaxUploadSize":40,"Buckets":{"12":["r"],"3":["d"]}}}""";

    private static readonly Access[] Accesses = { Access.Read, Access.Write, Access.Delete, Access.Maximum };

    // The rows the trees name, and one they do not.
    private static readonly string[] RowIds = { "3", "9", "12", "unlisted" };

    [TestMethod]
    public void BothOrdersGiveTheSameTree()
    {
        foreach (var order in Orders(ReadCities, ManageCities))
        {
            var context = Build(order);

            Assert.AreEqual(
                """{"DirectoryActions":{"Rows":{"Cities":["r","w","d","m"]}},"StorageActions":["r","w","d","m"]}""",
                context.GenerateAccessTree(context));
        }
    }

    [TestMethod]
    public void EveryOrderGivesTheSameTree()
    {
        var generated = Orders(ReadDirectory, ManageUsers, ManageFiles)
            .Select(order => Build(order))
            .Select(context => context.GenerateAccessTree(context))
            .Distinct()
            .ToList();

        Assert.AreEqual(1, generated.Count, string.Join(Environment.NewLine, generated));
    }

    [TestMethod]
    public void ReducerAndPreserverOrderDoesNotChangeTheTree()
    {
        var producer = Build("""{"DirectoryActions":{"Rows":{"Cities":["r","w","d"]}},"StorageActions":["r","w","d","m"]}""");

        var reducerTrees = new[]
        {
            """{"DirectoryActions":{"Rows":{"Cities":["r"]}},"StorageActions":["r"]}""",
            """{"DirectoryActions":{"Rows":{"Cities":["r","w","d"]}},"StorageActions":["r","w","d","m"]}""",
        };

        var preserverTrees = new[]
        {
            """{"DirectoryActions":{"Rows":{"Regions":["r"]}}}""",
            """{"DirectoryActions":{"Rows":{"Regions":["r","w"]}}}""",
        };

        foreach (var reducerOrder in Orders(reducerTrees))
        {
            foreach (var preserverOrder in Orders(preserverTrees))
            {
                Assert.AreEqual(
                    """{"DirectoryActions":{"Rows":{"Cities":["r","w","d"],"Regions":["r","w"]}},"StorageActions":["r","w","d","m"]}""",
                    producer.GenerateAccessTree(Build(reducerOrder), Build(preserverOrder)));
            }
        }
    }

    [TestMethod]
    public void NodeWildcardHoldsTheAccessesOfEveryTree()
    {
        foreach (var order in Orders(ReadCities, ManageCities))
        {
            var context = Build(order);

            CollectionAssert.AreEquivalent(Accesses, context.FindActionTreeNode("StorageActions")!.WildCardAccess);
            CollectionAssert.AreEquivalent(Accesses, context.FindActionTreeNode("DirectoryActions.Rows.Cities")!.WildCardAccess);
        }
    }

    [TestMethod]
    public void GeneratedTreeGivesTheSameDecisions()
    {
        foreach (var trees in new[] { new[] { ReadCities, ManageCities }, new[] { ReadDirectory, ManageUsers, ManageFiles } })
        {
            foreach (var order in Orders(trees))
            {
                var context = Build(order);

                AssertSameDecisions(context, Build(context.GenerateAccessTree(context)));
            }
        }
    }

    [TestMethod]
    public void WildcardIsKeptWhenNothingBeneathItGoesBeyondIt()
    {
        foreach (var order in Orders("""{"DirectoryActions":["r","w"]}""", """{"DirectoryActions":{"Users":["r"],"Rows":{"Cities":["r"]}}}"""))
        {
            var context = Build(order);

            Assert.AreEqual("""{"DirectoryActions":["r","w"]}""", context.GenerateAccessTree(context));
        }
    }

    [TestMethod]
    public void DynamicWildcardIsKeptOverRowsThatGoBeyondIt()
    {
        // A dynamic action is written as a wildcard or as rows, never both, so a row that another tree grants beyond
        // the wildcard cannot be written. The wildcard is kept: it is the broader grant.
        foreach (var order in Orders("""{"DirectoryActions":{"Rows":{"Cities":["r"]}}}""", """{"DirectoryActions":{"Rows":{"Cities":{"3":["r","w"]}}}}"""))
        {
            var context = Build(order);
            var generated = context.GenerateAccessTree(context);

            Assert.AreEqual("""{"DirectoryActions":{"Rows":{"Cities":["r"]}}}""", generated);
            Assert.IsTrue(context.CanWrite(DirectoryActions.Rows.Cities, "3"));
            Assert.IsFalse(Build(generated).CanWrite(DirectoryActions.Rows.Cities, "3"));
        }
    }

    [TestMethod]
    public void WildcardEditedInPlaceWinsOverWhatIsBeneathIt()
    {
        // An access-tree editor changes WildCardAccess in place and leaves the action bank as it was loaded, so Users
        // still holds its write. The tree is generated against the editing user's context. As its own reducer it is
        // written the same way: one tree cannot hold a wildcard and grants beneath it, so they come from the edit.
        var edited = Build("""{"DirectoryActions":{"Users":["r","w"]}}""");
        var editingUser = Build("""{"DirectoryActions":["r","w","d","m"],"StorageActions":["r","w","d","m"]}""");

        edited.FindActionTreeNode("DirectoryActions")!.WildCardAccess.Add(Access.Read);

        Assert.AreEqual("""{"DirectoryActions":["r"]}""", edited.GenerateAccessTree(editingUser));
        Assert.AreEqual("""{"DirectoryActions":["r"]}""", edited.GenerateAccessTree(edited));
    }

    [TestMethod]
    public void DynamicWildcardRemovedInPlaceIsRemovedFromTheActionBank()
    {
        // An access-tree editor removes a dynamic action's wildcard through WildCardAccess. With one tree that list is
        // the action bank item's access list, so the rows lose the access too.
        var edited = Build("""{"DirectoryActions":{"Rows":{"Cities":["r"]}}}""");
        var editingUser = Build("""{"DirectoryActions":["r","w","d","m"],"StorageActions":["r","w","d","m"]}""");

        edited.FindActionTreeNode("DirectoryActions.Rows.Cities")!.WildCardAccess.Remove(Access.Read);

        Assert.IsFalse(edited.CanRead(DirectoryActions.Rows.Cities, "3"));
        Assert.AreEqual("{}", edited.GenerateAccessTree(editingUser));
    }

    private static TypeAuthContext Build(params string[] accessTrees)
    {
        var builder = new TypeAuthContextBuilder()
            .AddActionTree<DirectoryActions>()
            .AddActionTree<StorageActions>();

        foreach (var accessTree in accessTrees)
            builder.AddAccessTree(accessTree);

        return builder.Build();
    }

    private static IEnumerable<string[]> Orders(params string[] trees)
    {
        if (trees.Length <= 1)
        {
            yield return trees;
            yield break;
        }

        for (var i = 0; i < trees.Length; i++)
        {
            var first = trees[i];

            foreach (var rest in Orders(trees.Where((_, index) => index != i).ToArray()))
                yield return new[] { first }.Concat(rest).ToArray();
        }
    }

    private static void AssertSameDecisions(TypeAuthContext expected, TypeAuthContext actual)
    {
        foreach (var action in Actions(expected.ActionTree))
        {
            if (action is DecimalAction decimalAction)
            {
                Assert.AreEqual(expected.AccessValue(decimalAction), actual.AccessValue(decimalAction), action.Path);
                continue;
            }

            if (action is TextAction textAction)
            {
                Assert.AreEqual(expected.AccessValue(textAction), actual.AccessValue(textAction), action.Path);
                continue;
            }

            foreach (var access in Accesses)
            {
                if (action is DynamicAction dynamicAction)
                {
                    foreach (var id in RowIds)
                        Assert.AreEqual(expected.Can(dynamicAction, access, id), actual.Can(dynamicAction, access, id), $"{action.Path} {access} {id}");

                    var expectedItems = expected.GetAccessibleItems(dynamicAction, x => x == access);
                    var actualItems = actual.GetAccessibleItems(dynamicAction, x => x == access);

                    Assert.AreEqual(expectedItems.WildCard, actualItems.WildCard, $"{action.Path} {access}");
                    CollectionAssert.AreEquivalent(expectedItems.AccessibleIds.Distinct().ToList(), actualItems.AccessibleIds.Distinct().ToList(), $"{action.Path} {access}");
                }
                else
                {
                    Assert.AreEqual(expected.Can(action, access), actual.Can(action, access), $"{action.Path} {access}");
                }
            }
        }
    }

    private static IEnumerable<ActionBase> Actions(ActionTreeNode node)
    {
        foreach (var child in node.ActionTreeItems)
        {
            if (child.Action != null && !child.IsADynamicSubItem)
                yield return child.Action;

            foreach (var action in Actions(child))
                yield return action;
        }
    }

    public class DirectoryActions
    {
        public class Rows
        {
            public static readonly DynamicReadWriteDeleteAction Cities = new("Cities");
            public static readonly DynamicReadWriteDeleteAction Regions = new("Regions");
        }

        public static readonly ReadWriteDeleteAction Users = new("Users");
        public static readonly BooleanAction Export = new("Export");
        public static readonly DecimalAction Discount = new("Discount", minimumAccess: 0, maximumAccess: 100);
    }

    public class StorageActions
    {
        public static readonly DecimalAction MaxUploadSize = new("Max Upload Size", minimumAccess: 0, maximumAccess: 100);
        public static readonly ReadWriteDeleteAction Files = new("Files");
        public static readonly BooleanAction ViewDeletedFiles = new("View Deleted Files");
        public static readonly DynamicReadWriteDeleteAction Buckets = new("Buckets");
    }
}
