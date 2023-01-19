using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using ShiftSoftware.TypeAuth.Shared;

namespace TypeAuthTests.ERP
{
    [TestClass()]
    public class CRMAgent
    {
        [TestMethod("10% Discount Value")]
        public void _10PercentDiscountValue()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.AreEqual("10", tAuth.AccessValue(CRMActions.DiscountValue));
        }

        [TestMethod("Read/Write Tickets")]
        public void ReadWriteTickets()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.Tickets) &&
                tAuth.CanWrite(CRMActions.Tickets)
            );
        }

        [TestMethod("Read Comments")]
        public void ReadComments()
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(AccessTreeFiles.CRMAgent);

            Assert.IsTrue(
                tAuth.CanRead(CRMActions.SocialMediaComments)
            );
        }

        [TestMethod("Morning Shift")]
        public void MorningShift()
        {
            Assert.IsTrue(TimeWithinRange(new TimeSpan(9, 45, 0), AccessTreeFiles.CRMAgent));
        }

        [TestMethod("Afternoon Break")]
        public void AfternoonBreak()
        {
            Assert.IsFalse(TimeWithinRange(new TimeSpan(13, 15, 0), AccessTreeFiles.CRMAgent));
        }

        [TestMethod("After Work")]
        public void AfterWork()
        {
            Assert.IsFalse(TimeWithinRange(new TimeSpan(18, 00, 1), AccessTreeFiles.CRMAgent));
        }

        public static bool TimeWithinRange(TimeSpan currentTime, string accessTree)
        {
            var tAuth = AccessTreeHelper.GetTypeAuthContext(accessTree);

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

            return canWork;
        }
    }
}
