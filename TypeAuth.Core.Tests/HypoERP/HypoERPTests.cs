using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using TypeAuthTests.HypoERP.ActionTrees;
using System.Collections.Generic;

namespace HypoERP
{
    class AccessTreeFiles
    {
        public static string SuperAdmin = System.IO.File.ReadAllText("HypoERP/AccessTrees/SuperAdmin.json");
        public static string SalesAdmin = System.IO.File.ReadAllText("HypoERP/AccessTrees/SalesAdmin.json");
        public static string CRMAgent = System.IO.File.ReadAllText("HypoERP/AccessTrees/CRMAgent.json");
    }

    class Helper
    {
        public static TypeAuthContext GetSuperAdminTypeAuthContext(string file)
        {
            return new TypeAuthContext(file, typeof(SystemActions), typeof(CRMActions));
        }
    }

    [TestClass()]
    public class SuperAdmin
    {
        [TestMethod("Login via WildCard")]
        public void LoginViaWildCard()
        {
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.SuperAdmin);

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
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.SuperAdmin);

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

    [TestClass()]
    public class SalesAdmin
    {
        [TestMethod("Full Discount via Wild Card")]
        public void FullDiscountViaWildCard()
        {
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.AreEqual("100", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("Only Read/Write users")]
        public void OnlyReadWriteUsers()
        {
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(SystemActions.UserModule.Users) &&
                tAuth.CanWrite(SystemActions.UserModule.Users) &&
                !tAuth.CanDelete(SystemActions.UserModule.Users)
            );
        }

        [TestMethod("Full Access on Discount Voucher")]
        public void FullDiscountOnDiscountVoucher()
        {
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.DiscountVouchers) &&
                tAuth.CanWrite(CRMActions.DiscountVouchers) &&
                tAuth.CanDelete(CRMActions.DiscountVouchers)
            );
        }
    }

    [TestClass()]
    public class CRMAgent
    {
        [TestMethod("10% Discount Value")]
        public void _10PercentDiscountValue()
        {
            var tAuth = Helper.GetSuperAdminTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.AreEqual("10", tAuth.AccessValue(CRMActions.DiscountValue));
        }
    }
}