using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuthTests.HypoERP.ActionTrees;

namespace TypeAuthTests.ERP
{
    [TestClass()]
    public class SalesAdmin
    {
        [TestMethod("Full Discount via Wild Card")]
        public void FullDiscountViaWildCard()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.AreEqual("100", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("Only Read/Write users")]
        public void OnlyReadWriteUsers()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(SystemActions.UserModule.Users) &&
                tAuth.CanWrite(SystemActions.UserModule.Users) &&
                !tAuth.CanDelete(SystemActions.UserModule.Users)
            );
        }

        [TestMethod("Full Access on Discount Voucher")]
        public void FullAccessOnDiscountVoucher()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.DiscountVouchers) &&
                tAuth.CanWrite(CRMActions.DiscountVouchers) &&
                tAuth.CanDelete(CRMActions.DiscountVouchers)
            );
        }

        [TestMethod("Full Access on Tickets & Comments")]
        public void FullDiscountOnTicketsAndComments()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Tickets) &&
                tAuth.CanWrite(CRMActions.Tickets) &&
                tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }
    }
}
