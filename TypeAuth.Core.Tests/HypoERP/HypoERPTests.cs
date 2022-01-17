using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShiftSoftware.TypeAuth.Core;
using TypeAuthTests.HypoERP.ActionTrees;
using System.Collections.Generic;
using System.Linq;

namespace HypoERP
{
    class AccessTreeFiles
    {
        public static string SuperAdmin = System.IO.File.ReadAllText("HypoERP/AccessTrees/SuperAdmin.json");
        public static string SalesAdmin = System.IO.File.ReadAllText("HypoERP/AccessTrees/SalesAdmin.json");
        public static string CRMAgent = System.IO.File.ReadAllText("HypoERP/AccessTrees/CRMAgent.json");
        public static string Affiliates = System.IO.File.ReadAllText("HypoERP/AccessTrees/Affiliates.json");
    }

    class Helper
    {
        public static TypeAuthContext GetTypeAuthContext(string file)
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
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

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
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SuperAdmin);

            Assert.IsTrue(tAuth.CanDelete(CRMActions.Customers));
        }
    }

    [TestClass()]
    public class SalesAdmin
    {
        [TestMethod("Full Discount via Wild Card")]
        public void FullDiscountViaWildCard()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.AreEqual("100", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("Only Read/Write users")]
        public void OnlyReadWriteUsers()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(SystemActions.UserModule.Users) &&
                tAuth.CanWrite(SystemActions.UserModule.Users) &&
                !tAuth.CanDelete(SystemActions.UserModule.Users)
            );
        }

        [TestMethod("Full Access on Discount Voucher")]
        public void FullDiscountOnDiscountVoucher()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.DiscountVouchers) &&
                tAuth.CanWrite(CRMActions.DiscountVouchers) &&
                tAuth.CanDelete(CRMActions.DiscountVouchers)
            );
        }

        [TestMethod("Full Access on Tickets & Comments")]
        public void FullDiscountOnTicketsAndComments()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.SalesAdmin);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Tickets) &&
                tAuth.CanWrite(CRMActions.Tickets) &&
                tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }
    }

    [TestClass()]
    public class CRMAgent
    {
        [TestMethod("10% Discount Value")]
        public void _10PercentDiscountValue()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.AreEqual("10", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("Read/Write Tickets")]
        public void ReadWriteTickets()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Tickets) &&
                tAuth.CanWrite(CRMActions.Tickets)
            );
        }

        [TestMethod("Read Comments")]
        public void ReadComments()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }

        [TestMethod("Morning Shift")]
        public void MorningShift()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            var currentTime = new System.TimeSpan(9, 45, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = false;

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    if (currentTime >= startTime && currentTime <= endTime)
                        canWork = true;
                }
            }

            Assert.IsTrue(canWork);
        }

        [TestMethod("Afternoon Break")]
        public void AfternoonBreak()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            var currentTime = new System.TimeSpan(13, 15, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = false;

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    if (currentTime >= startTime && currentTime <= endTime)
                        canWork = true;
                }
            }

            Assert.IsFalse(canWork);
        }

        [TestMethod("After Work")]
        public void AfterWork()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            var currentTime = new System.TimeSpan(18, 00, 1);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = false;

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    if (currentTime >= startTime && currentTime <= endTime)
                        canWork = true;
                }
            }

            Assert.IsFalse(canWork);
        }
    }

    [TestClass()]
    public class Affiliate
    {
        [TestMethod("Read Only on Customers")]
        public void ReadOnlyOnCustomers()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Customers) &&
                !tAuth.CanWrite(CRMActions.Customers) &&
                !tAuth.CanDelete(CRMActions.Customers)
            );
        }

        [TestMethod("2% Discount Value")]
        public void _2PercentDiscount()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.AreEqual("2", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("No Access on Tickets")]
        public void ReadWriteTickets()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead(CRMActions.Tickets) &&
                !tAuth.CanWrite(CRMActions.Tickets)
            );
        }

        [TestMethod("No Access on Comments")]
        public void ReadComments()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            Assert.IsTrue(
                !tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }

        [TestMethod("Work at 20 to 21")]
        public void WorkTime()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            var currentTime = new System.TimeSpan(20, 15, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = false;

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    if (currentTime >= startTime && currentTime <= endTime)
                        canWork = true;
                }
            }

            Assert.IsTrue(canWork);
        }

        [TestMethod("Can't Work Outside 20 to 21")]
        public void AfterWork()
        {
            var tAuth = Helper.GetTypeAuthContext(AccessTreeFiles.Affiliates);

            var currentTime = new System.TimeSpan(17, 0, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            var canWork = false;

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    if (currentTime >= startTime && currentTime <= endTime)
                        canWork = true;
                }
            }

            Assert.IsFalse(canWork);
        }
    }

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
        /// SalesAdmin does not contain "SetOrResetPassword" entry
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
        public void _10PercentDiscountValue()
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

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.IsTrue(
                tAuth1.AccessValue(CRMActions.DiscountValue).Equals("100") &&
                tAuth2.AccessValue(CRMActions.DiscountValue).Equals("10") &&
                tAuth3.AccessValue(CRMActions.DiscountValue).Equals("2")
            );
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [TestMethod("Combined Work Schedule")]
        public void CombinedWorkSchedule()
        {
            var tAuth = new TypeAuthContext(new List<string>
            {
                AccessTreeFiles.Affiliates,
                AccessTreeFiles.CRMAgent,

            }, typeof(SystemActions), typeof(CRMActions));

            var cantWorkAt7 = new System.TimeSpan(7, 0, 0);
            var canWorkAt9 = new System.TimeSpan(9, 15, 0);
            var cantWorkAt1350 = new System.TimeSpan(13, 30, 0);
            var canWorkAt15 = new System.TimeSpan(15, 20, 0);
            var cantWorkAt19 = new System.TimeSpan(19, 20, 0);
            var canWorkAt20 = new System.TimeSpan(20, 30, 0);
            var cantWorkAt22 = new System.TimeSpan(22, 30, 0);

            var schedule = tAuth.AccessValue(CRMActions.WorkSchedule);

            System.Console.WriteLine(schedule);

            var canWork = new Dictionary<System.TimeSpan, bool>
            {
                { cantWorkAt7, false },
                { canWorkAt9, false },
                { cantWorkAt1350, false },
                { canWorkAt15, false },
                { cantWorkAt19, false },
                { canWorkAt20, false },
                { cantWorkAt22, false },
            };

            if (schedule != null)
            {
                var timeSlots = schedule.Split(',').Select(x => x.Trim());

                foreach (var slot in timeSlots)
                {
                    var startTime = System.TimeSpan.Parse(slot.Split('-')[0].Trim());
                    var endTime = System.TimeSpan.Parse(slot.Split('-')[1].Trim());

                    foreach (var item in canWork)
                    {
                        if (item.Key >= startTime && item.Key <= endTime)
                            canWork[item.Key] = true;
                    }
                }
            }

            Assert.IsTrue(
                !canWork[cantWorkAt7] &&
                canWork[canWorkAt9] &&
                !canWork[cantWorkAt1350] &&
                canWork[canWorkAt15] &&
                !canWork[cantWorkAt19] &&
                canWork[canWorkAt20] &&
                !canWork[cantWorkAt22]
            );
        }
    }
}