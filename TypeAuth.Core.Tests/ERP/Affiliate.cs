using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShiftSoftware.TypeAuth.Shared;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class Affiliate
    {
        [TestMethod("Read Only on Customers")]
        public void ReadOnlyOnCustomers()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Customers) &&
                !tAuth.CanWrite(CRMActions.Customers) &&
                !tAuth.CanDelete(CRMActions.Customers)
            );
        }

        [TestMethod("2% Discount Value")]
        public void _2PercentDiscount()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.AreEqual("2", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("2.50% Decimal Discount Value")]
        public void _2_50DecimalPercentDiscount()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.AreEqual(2.5m, tAuth.AccessValue(CRMActions.DecimalDiscount));
        }

        [TestMethod("No Access on Tickets")]
        public void ReadWriteTickets()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead(CRMActions.Tickets) &&
                !tAuth.CanWrite(CRMActions.Tickets)
            );
        }

        [TestMethod("No Access on Comments")]
        public void ReadComments()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }

        [TestMethod("Work at 20 to 21")]
        public void WorkTime()
        {
            Assert.IsTrue(CRMAgent.TimeWithinRange(new TimeSpan(20, 15, 0), AccessTreeFiles.Affiliates));
        }

        [TestMethod("Can't Work Outside 20 to 21")]
        public void AfterWork()
        {
            Assert.IsFalse(CRMAgent.TimeWithinRange(new TimeSpan(17, 0, 0), AccessTreeFiles.Affiliates));
        }
    }
}
