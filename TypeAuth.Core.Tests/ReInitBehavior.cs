using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.Collections.Generic;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass]
    public class ReInitBehavior
    {
        [TestMethod]
        public void ReInitClearsPreviousPermissions()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);
            Assert.IsTrue(tAuth.CanRead(CRMActions.Customers));

            tAuth.Init(new List<string> { "{}" }, typeof(SystemActions), typeof(CRMActions));
            Assert.IsFalse(tAuth.CanRead(CRMActions.Customers));
        }

        [TestMethod]
        public void ReInitWithDifferentPermissionsReplacesOld()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);
            Assert.IsTrue(tAuth.CanWrite(CRMActions.Customers));
            Assert.IsTrue(tAuth.CanDelete(CRMActions.Customers));

            tAuth.Init(new List<string> { AccessTreeFiles.CRMAgent }, typeof(SystemActions), typeof(CRMActions));
            Assert.IsTrue(tAuth.CanRead(CRMActions.Customers));
            Assert.IsTrue(tAuth.CanWrite(CRMActions.Customers));
            Assert.IsFalse(tAuth.CanDelete(CRMActions.Customers));
        }

        [TestMethod]
        public void ReInitDoesNotAccumulateTextValues()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);
            Assert.AreEqual("10", tAuth.AccessValue(CRMActions.DiscountValue));

            tAuth.Init(new List<string> { AccessTreeFiles.Affiliates }, typeof(SystemActions), typeof(CRMActions));
            Assert.AreEqual("2", tAuth.AccessValue(CRMActions.DiscountValue));
        }
    }
}
