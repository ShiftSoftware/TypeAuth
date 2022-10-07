using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using TypeAuthTests.HypoERP.ActionTrees;

namespace TypeAuthTests.ERP
{
    [TestClass()]
    public class SuperAdmin
    {
        [TestMethod("Login via WildCard")]
        public void LoginViaWildCard()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(tAuth.CanAccess(SystemActions.Login.MultipleSession));
        }

        [TestMethod("No Login")]
        public void NoLogin()
        {
            var tAuth1 = new TypeAuthContext(AccessTreeFiles.SuperAdmin, typeof(CRMActions));
            var tAuth2 = new TypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(
                !tAuth1.CanAccess(SystemActions.Login.MultipleSession) &&
                !tAuth2.CanAccess(SystemActions.Login.MultipleSession)
            );
        }

        [TestMethod("Customer via WildCard")]
        public void CustomerViaWildCard()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(tAuth.CanDelete(CRMActions.Customers));
        }
    }
}
