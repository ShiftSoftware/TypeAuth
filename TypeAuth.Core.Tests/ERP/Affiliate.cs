﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

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
                tAuth.CanRead<CRMActions>(x => x.Customers) &&
                !tAuth.CanWrite<CRMActions>(x => x.Customers) &&
                !tAuth.CanDelete<CRMActions>(x => x.Customers)
            );
        }

        [TestMethod("2% Discount Value")]
        public void _2PercentDiscount()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.AreEqual("2", tAuth.AccessValue<CRMActions>(x => x.DiscountValue));
        }

        [TestMethod("2.50% Decimal Discount Value")]
        public void _2_50DecimalPercentDiscount()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.AreEqual(2.5m, tAuth.AccessValue<CRMActions>(x => x.DecimalDiscount));
        }

        [TestMethod("No Access on Tickets")]
        public void ReadWriteTickets()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead<CRMActions>(x => x.Tickets) &&
                !tAuth.CanWrite<CRMActions>(x => x.Tickets)
            );
        }

        [TestMethod("No Access on Comments")]
        public void ReadComments()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead<CRMActions>(x => x.SocialMediaComments)
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
