using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;

namespace ShiftSoftware.TypeAuth.Tests.ERP
{
    [TestClass()]
    public class MultiAccessTree
    {
        /// <summary>
        /// We're checking an Action that only exists in one of the Access Trees.
        /// SalesAdmin does not contain "SetOrResetPassword" entry
        /// </summary>
        [TestMethod("Super Admin & Sales Admin (No Merge)")]
        public void SuperAdminAndSalesAdminNoMerge()
        {
            var tAuth_WithSuperAdmin = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.SalesAdmin,
                AccessTreeFiles.SuperAdmin,
            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth_WithoutSuperAdmin = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.SalesAdmin,
            }, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(
                tAuth_WithSuperAdmin.CanAccess(SystemActions.UserModule.SetOrResetPassword) &&
                !tAuth_WithoutSuperAdmin.CanAccess(SystemActions.UserModule.SetOrResetPassword)
            );
        }

        /// <summary>
        /// We're checking an Action that exists in both Access Trees.
        /// </summary>
        [TestMethod("Super Admin & Sales Admin (Merge)")]
        public void SuperAdminAndSalesAdminMerge()
        {
            var tAuth_WithSuperAdmin = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.SalesAdmin,
                AccessTreeFiles.SuperAdmin,
            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth_WithoutSuperAdmin = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.SalesAdmin
            }, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(
                !tAuth_WithoutSuperAdmin.CanDelete(SystemActions.UserModule.Users) &&
                tAuth_WithSuperAdmin.CanDelete(SystemActions.UserModule.Users)
            );
        }

        [TestMethod("100 Discount Value (From Many Trees)")]
        public void _100PercentDiscountValue()
        {
            var tAuth1 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.SalesAdmin,
                AccessTreeFiles.CRMAgent,

            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth2 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.CRMAgent,

            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth3 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
            }, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(
                tAuth1.AccessValue(CRMActions.DiscountValue)!.Equals("100") &&
                tAuth2.AccessValue(CRMActions.DiscountValue)!.Equals("10") &&
                tAuth3.AccessValue(CRMActions.DiscountValue)!.Equals("2")
            );
        }

        [TestMethod("100 Decimal Discount Value (From Many Trees)")]
        public void _100DecimalPercentDiscountValue()
        {
            var tAuth1 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.SalesAdmin,
                AccessTreeFiles.CRMAgent,

            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth2 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.CRMAgent,

            }, typeof(SystemActions), typeof(CRMActions));

            var tAuth3 = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
            }, typeof(SystemActions), typeof(CRMActions));

            Assert.IsTrue(
                tAuth1.AccessValue(CRMActions.DecimalDiscount)!.Equals(100m) &&
                tAuth2.AccessValue(CRMActions.DecimalDiscount)!.Equals(15m) &&
                tAuth3.AccessValue(CRMActions.DecimalDiscount)!.Equals(2.5m)
            );
        }

        [TestMethod("Combined Work Schedule")]
        public void CombinedWorkSchedule()
        {
            var tAuth = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.CRMAgent
            }, typeof(SystemActions), typeof(CRMActions));


            var cantWorkAt7 = new TimeSpan(7, 0, 0);
            var canWorkAt9_15 = new TimeSpan(9, 15, 0);
            var cantWorkAt13_30 = new TimeSpan(13, 30, 0);
            var canWorkAt15_20 = new TimeSpan(15, 20, 0);
            var cantWorkAt19_20 = new TimeSpan(19, 20, 0);
            var canWorkAt20_30 = new TimeSpan(20, 30, 0);
            var cantWorkAt22_30 = new TimeSpan(22, 30, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = new Dictionary<TimeSpan, bool>
            {
                { cantWorkAt7, false },
                { canWorkAt9_15, false },
                { cantWorkAt13_30, false },
                { canWorkAt15_20, false },
                { cantWorkAt19_20, false },
                { canWorkAt20_30, false },
                { cantWorkAt22_30, false },
            };

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = TimeSpan.Parse(slot.Split('-')[1].Trim());

                    foreach (var item in canWork)
                    {
                        if (item.Key >= startTime && item.Key <= endTime)
                            canWork[item.Key] = true;
                    }
                }
            }

            Assert.IsTrue(
                !canWork[cantWorkAt7] &&
                canWork[canWorkAt9_15] &&
                !canWork[cantWorkAt13_30] &&
                canWork[canWorkAt15_20] &&
                !canWork[cantWorkAt19_20] &&
                canWork[canWorkAt20_30] &&
                !canWork[cantWorkAt22_30]
            );
        }
    }
}
