using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using TypeAuthTests.HypoERP.ActionTrees;
using System.Collections.Generic;

namespace HypoERP
{
    [TestClass()]
    public class SuperAdmin
    {
        class AccessTreeFiles
        {
            public static string SuperAdmin = System.IO.File.ReadAllText("HypoERP/AccessTrees/SuperAdmin.json");
        }

        private TypeAuthContext GetSuperAdminTypeAuthContext(string file) {
            return new TypeAuthContext(file, typeof(SystemActions), typeof(CRMActions));
        }

        [TestMethod("Login via WildCard")]
        public void LoginViaWildCard()
        {
            var tAuth = GetSuperAdminTypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(tAuth.CanAccess(SystemActions.Login.MultipleSession));
        }

        [TestMethod("No Login")]
        public void NoLogin()
        {
            var tAuth1 = new TypeAuthContext(AccessTreeFiles.SuperAdmin, typeof(CRMActions));
            var tAuth2 = new TypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsFalse(
                tAuth1.CanAccess(SystemActions.Login.MultipleSession) &&
                tAuth2.CanAccess(SystemActions.Login.MultipleSession)
            );
        }

        [TestMethod("Customer via WildCard")]
        public void CustomerViaWildCard()
        {
            var tAuth = GetSuperAdminTypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(tAuth.CanDelete(CRMActions.Customers));
        }

        //[TestMethod()]
        //public void ReadAccess()
        //{
        //    var ac = this.GetPermissionManager(AccessTreeFiles.ServiceUser);

        //    Assert.IsTrue(ac.CanRead(SystemActions.Users.Users));
        //}

        //[TestMethod()]
        //public void WriteAccess()
        //{
        //    var ac = this.GetPermissionManager(AccessTreeFiles.ServiceUser);

        //    Assert.IsTrue(ac.CanWrite(SystemActions.Users.Users));
        //}

        //[TestMethod()]
        //public void DirectValueTest()
        //{
        //    var ac = this.GetPermissionManager(AccessTreeFiles.ServiceUser);

        //    Assert.AreEqual("10", ac.AccessValue(CRMActions.SaleDiscount));
        //}

        //[TestMethod()]
        //public void WildCardValueTest()
        //{
        //    var ac = this.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

        //    Assert.AreEqual("100", ac.AccessValue(CRMActions.SaleDiscount));
        //}

        //[TestMethod()]
        //public void MinimumValue()
        //{
        //    var ac = this.GetPermissionManager(AccessTreeFiles.Agent);

        //    Assert.AreEqual("0", ac.AccessValue(CRMActions.SaleDiscount));
        //}
    }
}