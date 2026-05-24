using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass]
    public class ReduceValueBehavior
    {
        [TestMethod]
        public void ValueExceedingMaximumAccessIsCapped()
        {
            // DiscountValue: min="0", max="100", comparer=max(int)
            var tAuth = new TypeAuthContext("{}", typeof(CRMActions));

            // value=120 with ceiling=150 — both exceed MaximumAccess=100
            // ReduceValue should cap the result at 100
            tAuth.SetAccessValue(CRMActions.DiscountValue, "120", "150");
            Assert.AreEqual("100", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod]
        public void CeilingExceedsMaximumAccessButValueDoesNot()
        {
            var tAuth = new TypeAuthContext("{}", typeof(CRMActions));

            // value=80 is within bounds even though ceiling=150 exceeds MaximumAccess
            tAuth.SetAccessValue(CRMActions.DiscountValue, "80", "150");
            Assert.AreEqual("80", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod]
        public void ValueWithinBoundsIsNotAltered()
        {
            var tAuth = new TypeAuthContext("{}", typeof(CRMActions));

            // value=50, ceiling=80 — both within MaximumAccess=100
            tAuth.SetAccessValue(CRMActions.DiscountValue, "50", "80");
            Assert.AreEqual("50", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod]
        public void ValueCappedByCeilingBelowMaximumAccess()
        {
            var tAuth = new TypeAuthContext("{}", typeof(CRMActions));

            // value=90, ceiling=60 — value exceeds ceiling, should be reduced to 60
            tAuth.SetAccessValue(CRMActions.DiscountValue, "90", "60");
            Assert.AreEqual("60", tAuth.AccessValue(CRMActions.DiscountValue));
        }
    }
}
