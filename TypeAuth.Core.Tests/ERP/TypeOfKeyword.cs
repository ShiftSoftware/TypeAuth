using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using System;
using ShiftSoftware.TypeAuth.Shared;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class TypeOfKeyword
    {
        [TestMethod("Read ")]
        public void AffiliatesCanOnlyReadCustomers()
        {

            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                tAuth.Can(typeof(CRMActions), nameof(CRMActions.Customers), Access.Read) &&
                !tAuth.Can(typeof(CRMActions), nameof(CRMActions.Customers), Access.Write) &&
                !tAuth.Can(typeof(CRMActions), nameof(CRMActions.Customers), Access.Delete)
            );
        }

        [TestMethod("Sales Admin Full Access on Discount Voucher")]
        public void SalesAdminFullAccessOnDiscountVoucher()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.Can(typeof(CRMActions), nameof(CRMActions.DiscountVouchers), Access.Read) &&
                tAuth.Can(typeof(CRMActions), nameof(CRMActions.DiscountVouchers), Access.Write) &&
                tAuth.Can(typeof(CRMActions), nameof(CRMActions.DiscountVouchers), Access.Delete)
            );
        }

        [TestMethod("Checking an Invalid Action")]
        public void CheckingAnInvalidAction()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            var message = "";

            try
            {
                tAuth.Can(typeof(CRMActions), nameof(SystemActions.UserModule), Access.Read);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            System.Diagnostics.Debug.WriteLine(message);

            Assert.IsTrue(
                message.Equals($"No Such Action ({typeof(CRMActions).FullName}.{nameof(SystemActions.UserModule)})")
            );
        }
    }
}
