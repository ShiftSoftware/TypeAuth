using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.Globalization;
using System.Threading;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass]
    public class CultureInvariance
    {
        [TestMethod]
        public void DecimalAccessValueWorksUnderGermanCulture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                // German uses comma as decimal separator — "2.50" is not valid in de-DE
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");

                var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.Affiliates);
                Assert.AreEqual(2.50m, tAuth.AccessValue(CRMActions.DecimalDiscount));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void DecimalAccessValueWorksUnderArabicCulture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ar-IQ");

                var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);
                Assert.AreEqual(15.0m, tAuth.AccessValue(CRMActions.DecimalDiscount));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void DecimalComparerWorksUnderFrenchCulture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                // French uses comma as decimal separator
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

                // Multi-tree merge uses the comparer (decimal.Parse inside the lambda)
                var tAuth = new TypeAuthContext(
                    new System.Collections.Generic.List<string>
                    {
                        AccessTreeFiles.CRMAgent,
                        AccessTreeFiles.Affiliates
                    },
                    typeof(SystemActions), typeof(CRMActions));

                // Comparer picks max(15.0, 2.50) = 15.0
                Assert.AreEqual(15.0m, tAuth.AccessValue(CRMActions.DecimalDiscount));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }
    }
}
