﻿using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq;

namespace TypeAuthTests.HypoERP.ActionTrees
{

    [ActionTree("CRM Actions", "Actions Related to the CRM Module.")]
    public class CRMActions
    {
        public readonly static ReadWriteDeleteAction Customers = new ReadWriteDeleteAction("Customers");
        public readonly static ReadWriteDeleteAction DiscountVouchers = new ReadWriteDeleteAction("Discount Vouchers");
       
        public readonly static TextAction DiscountValue = new TextAction("Sale Discount", "", "0", "100", (a, b) =>
        {
            var numbers = new System.Collections.Generic.List<int>();

            if (a != null)
                numbers.Add(int.Parse(a));
            if (b != null)
                numbers.Add(int.Parse(b));

            if (numbers.Count > 0)
                return numbers.Max().ToString();

            return null;
        });

        public readonly static ReadWriteAction Tickets = new ReadWriteAction("Tickets");
        public readonly static ReadAction SocialMediaComments = new ReadAction("Social Media Comments");

        public readonly static TextAction WorkSchedule = new TextAction(
            "Work Schedule",
            "One or more time slots allowed for operation. Certain actions are not allowed outside work schedule.",
            null,
            "00:00:00 - 23:59:59",
            (a, b) =>
            {
                var joined = new System.Collections.Generic.List<string>();

                if (a != null)
                    joined.AddRange(a.Split(',').Select(x => x.Trim()).ToList());

                if (b != null)
                    joined.AddRange(b.Split(',').Select(x => x.Trim()).ToList());

                return string.Join(", ", joined);
            }
        );
    }
}
