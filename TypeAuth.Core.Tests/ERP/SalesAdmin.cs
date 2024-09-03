using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class SalesAdmin
    {
        [TestMethod("Full Discount via Wild Card")]
        public void FullDiscountViaWildCard()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.AreEqual("100", tAuth.AccessValue<CRMActions>(x => x.DiscountValue));
        }

        [TestMethod("Full Decimal Discount via Wild Card")]
        public void FullDecimalDiscountViaWildCard()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.AreEqual(100, tAuth.AccessValue<CRMActions>(x => x.DecimalDiscount));
        }

        [TestMethod("Only Read/Write users")]
        public void OnlyReadWriteUsers()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead<SystemActions.UserModule>(x => x.Users) &&
                tAuth.CanWrite<SystemActions.UserModule>(x => x.Users) &&
                !tAuth.CanDelete<SystemActions.UserModule>(x => x.Users)
            );
        }

        [TestMethod("Full Access on Discount Voucher")]
        public void FullAccessOnDiscountVoucher()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead<CRMActions>(x => x.DiscountVouchers) &&
                tAuth.CanWrite<CRMActions>(x => x.DiscountVouchers) &&
                tAuth.CanDelete<CRMActions>(x => x.DiscountVouchers)
            );
        }

        [TestMethod("Full Access on Tickets & Comments")]
        public void FullAccessOnTicketsAndComments()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead<CRMActions>(x => x.Tickets) &&
                tAuth.CanWrite<CRMActions>(x => x.Tickets) &&
                tAuth.CanRead<CRMActions>(x => x.SocialMediaComments)
            );
        }
    }
}
