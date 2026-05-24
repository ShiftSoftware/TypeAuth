using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests
{
    [TestClass]
    public class MalformedAccessTree
    {
        [TestMethod]
        public void EmptyJsonGrantsNoAccess()
        {
            var tAuth = new TypeAuthContext("{}", typeof(SystemActions), typeof(CRMActions));

            Assert.IsFalse(tAuth.CanRead(CRMActions.Customers));
            Assert.IsFalse(tAuth.CanAccess(SystemActions.Login.MultipleSession));
        }

        [TestMethod]
        public void NullAccessValueDoesNotCrash()
        {
            var json = "{\"SystemActions\": {\"Login\": {\"MultipleSession\": null}}}";
            var tAuth = new TypeAuthContext(json, typeof(SystemActions), typeof(CRMActions));

            Assert.IsFalse(tAuth.CanAccess(SystemActions.Login.MultipleSession));
        }

        [TestMethod]
        public void UnexpectedNumberValueDoesNotCrash()
        {
            var json = "{\"CRMActions\": 42}";
            var tAuth = new TypeAuthContext(json, typeof(SystemActions), typeof(CRMActions));

            Assert.IsFalse(tAuth.CanRead(CRMActions.Customers));
        }

        [TestMethod]
        public void InvalidEnumValuesAreSkipped()
        {
            var json = "{\"CRMActions\": [\"r\", \"xyz\"]}";
            var tAuth = new TypeAuthContext(json, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(tAuth.CanRead(CRMActions.Customers));
            Assert.IsFalse(tAuth.CanWrite(CRMActions.Customers));
        }

        [TestMethod]
        public void NullInAccessArrayIsSkipped()
        {
            var json = "{\"CRMActions\": [\"r\", null]}";
            var tAuth = new TypeAuthContext(json, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(tAuth.CanRead(CRMActions.Customers));
        }

        [TestMethod]
        public void StringValueWhereObjectExpectedDoesNotCrash()
        {
            var json = "{\"CRMActions\": \"not-an-object-or-array\"}";
            var tAuth = new TypeAuthContext(json, typeof(SystemActions), typeof(CRMActions));

            Assert.IsFalse(tAuth.CanRead(CRMActions.Customers));
        }
    }
}
