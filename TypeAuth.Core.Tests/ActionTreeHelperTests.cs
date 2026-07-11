using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass()]
    public class ActionTreeHelperTests
    {
        // A tree with members of every kind the discovery must distinguish: only the public
        // static ActionBase fields (First, Second) count as declared actions.
        private class MixedMembersTree
        {
            public static readonly ReadWriteDeleteAction First = new ReadWriteDeleteAction("First");
            public static readonly BooleanAction Second = new BooleanAction("Second", "");

            public static string NotAnAction = "text";
            public static readonly int AlsoNotAnAction = 5;
            public static ReadWriteDeleteAction? NullValued = null;
            public readonly ReadWriteDeleteAction InstanceField = new ReadWriteDeleteAction("Instance");
            private static readonly ReadWriteDeleteAction Private = new ReadWriteDeleteAction("Private");
            public static ReadWriteDeleteAction Property { get; } = new ReadWriteDeleteAction("Property");

            // Uses the otherwise-unused members so the compiler stays quiet about them.
            public override string ToString() => $"{NullValued}{InstanceField}{Private}";
        }

        [TestMethod("Finds a declared action and returns the field instance itself")]
        public void FindsDeclaredAction()
        {
            var action = ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "Customers");

            Assert.IsNotNull(action);
            Assert.AreSame(CRMActions.Customers, action);
        }

        [TestMethod("Works without a built TypeAuthContext")]
        public void WorksWithoutContext()
        {
            // This test builds no context — the helper needs none. This is the whole point of
            // the helper: resolving an action at service registration time, before any context
            // exists. (Other tests in the process may have built one already; the helper's
            // result is the static field instance either way.)
            var action = ActionTreeHelper.FindDeclaredAction(typeof(SystemActions.UserModule), "Users");

            Assert.AreSame(SystemActions.UserModule.Users, action);
        }

        [TestMethod("The found instance is the same one a built context authorizes against")]
        public void FoundInstanceWorksWithABuiltContext()
        {
            var action = (ReadWriteDeleteAction)ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "Customers")!;

            var context = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.IsTrue(context.CanRead(action));
            Assert.IsFalse(context.CanDelete(action));
        }

        [TestMethod("Name matching is exact and case-sensitive")]
        public void MatchingIsOrdinal()
        {
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "customers"));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "CUSTOMERS"));
        }

        [TestMethod("Returns null for an unknown name")]
        public void ReturnsNullForUnknownName()
        {
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "DoesNotExist"));
        }

        [TestMethod("Returns null for null, empty, or whitespace names")]
        public void ReturnsNullForBlankName()
        {
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), null!));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), ""));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(CRMActions), "   "));
        }

        [TestMethod("Does not search nested action tree classes")]
        public void DoesNotSearchNestedTrees()
        {
            // Users is declared on the nested UserModule class, not on SystemActions itself.
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(SystemActions), "Users"));
        }

        [TestMethod("Only public static ActionBase fields count as declared actions")]
        public void OnlyPublicStaticActionFieldsCount()
        {
            var declared = ActionTreeHelper.GetDeclaredActions(typeof(MixedMembersTree)).ToList();

            // Looked up by key, not by position: the enumeration order is not part of the
            // contract (reflection does not guarantee field order).
            Assert.AreEqual(2, declared.Count);
            Assert.AreSame(MixedMembersTree.First, declared.Single(x => x.Key == "First").Value);
            Assert.AreSame(MixedMembersTree.Second, declared.Single(x => x.Key == "Second").Value);

            // The same rule applies to single-name lookup.
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(MixedMembersTree), "NotAnAction"));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(MixedMembersTree), "NullValued"));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(MixedMembersTree), "InstanceField"));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(MixedMembersTree), "Private"));
            Assert.IsNull(ActionTreeHelper.FindDeclaredAction(typeof(MixedMembersTree), "Property"));
        }

        [TestMethod("Returns every declared action, of every action kind")]
        public void ReturnsAllDeclaredActions()
        {
            var declared = ActionTreeHelper.GetDeclaredActions(typeof(CRMActions)).ToList();

            // CRMActions declares seven actions: two ReadWriteDelete, two Text, one Decimal,
            // one ReadWrite, one Read.
            Assert.AreEqual(7, declared.Count);
            Assert.AreSame(CRMActions.Customers, declared.Single(x => x.Key == "Customers").Value);
            Assert.AreSame(CRMActions.DiscountValue, declared.Single(x => x.Key == "DiscountValue").Value);
            Assert.AreSame(CRMActions.DecimalDiscount, declared.Single(x => x.Key == "DecimalDiscount").Value);
            Assert.AreSame(CRMActions.Tickets, declared.Single(x => x.Key == "Tickets").Value);
            Assert.AreSame(CRMActions.SocialMediaComments, declared.Single(x => x.Key == "SocialMediaComments").Value);
        }

        [TestMethod("A null tree type throws")]
        public void NullTreeTypeThrows()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ActionTreeHelper.FindDeclaredAction(null!, "Customers"));
            Assert.ThrowsExactly<ArgumentNullException>(() => ActionTreeHelper.GetDeclaredActions(null!).ToList());
        }
    }
}
