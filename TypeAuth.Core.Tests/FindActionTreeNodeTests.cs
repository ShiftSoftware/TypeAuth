using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass()]
    public class FindActionTreeNodeTests
    {
        // The access tree is irrelevant to node resolution — every registered action is resolvable
        // regardless of what the subject can do — so an empty access tree is used throughout.
        private static TypeAuthContext Context() => AccessTreeHelper.GetTypeAuthContext("{}");

        [TestMethod("Resolves an action node by its full path")]
        public void ResolvesActionNode()
        {
            var node = Context().FindActionTreeNode("SystemActions.UserModule.Users");

            Assert.IsNotNull(node);
            Assert.AreEqual("SystemActions.UserModule.Users", node!.Path);
            Assert.AreEqual("Users", node.ID);
            Assert.IsNotNull(node.Action);
            Assert.AreEqual("User Access", node.Action!.Name);
            // The action's own Path is stamped during tree building and round-trips with the lookup key.
            Assert.AreEqual(node.Path, node.Action.Path);
        }

        [TestMethod("Resolves a nested grouping (tree) node by its path")]
        public void ResolvesGroupingNode()
        {
            var node = Context().FindActionTreeNode("SystemActions.UserModule");

            Assert.IsNotNull(node);
            Assert.AreEqual("UserModule", node!.ID);
            Assert.IsNull(node.Action, "A grouping node has no action.");
            // DisplayName comes from the [ActionTree(...)] attribute on the nested class.
            Assert.AreEqual("Users", node.DisplayName);
        }

        [TestMethod("Resolves a top-level action tree node by its path")]
        public void ResolvesTopLevelTreeNode()
        {
            var node = Context().FindActionTreeNode("SystemActions");

            Assert.IsNotNull(node);
            Assert.AreEqual("SystemActions", node!.ID);
            Assert.AreEqual("System Actions", node.DisplayName);
        }

        [TestMethod("Resolves an action in a second registered action tree")]
        public void ResolvesAcrossActionTrees()
        {
            var node = Context().FindActionTreeNode("CRMActions.Customers");

            Assert.IsNotNull(node);
            Assert.IsNotNull(node!.Action);
            Assert.AreEqual("Customers", node.Action!.Name);
        }

        [TestMethod("Returns null for a path that matches no node")]
        public void ReturnsNullForUnknownPath()
        {
            var context = Context();

            Assert.IsNull(context.FindActionTreeNode("SystemActions.UserModule.DoesNotExist"));
            Assert.IsNull(context.FindActionTreeNode("NopeActions"));
            // A partial/prefix that isn't itself a node must not resolve.
            Assert.IsNull(context.FindActionTreeNode("SystemActions.UserModule.Users.Extra"));
        }

        [TestMethod("Returns null for null, empty, or whitespace paths")]
        public void ReturnsNullForBlankPath()
        {
            var context = Context();

            Assert.IsNull(context.FindActionTreeNode(null!));
            Assert.IsNull(context.FindActionTreeNode(""));
            Assert.IsNull(context.FindActionTreeNode("   "));
        }

        [TestMethod("Path matching is case-sensitive (ordinal)")]
        public void MatchingIsOrdinal()
        {
            Assert.IsNull(Context().FindActionTreeNode("systemactions.usermodule.users"));
        }
    }
}
