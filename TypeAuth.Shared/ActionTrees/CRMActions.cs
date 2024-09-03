using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Collections.Generic;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Shared.ActionTrees
{

    [ActionTree("CRM Actions", "Actions Related to the CRM Module.")]
    public class CRMActions
    {
        public ReadWriteDeleteAction Customers = new ReadWriteDeleteAction("Customers");
        public ReadWriteDeleteAction DiscountVouchers = new ReadWriteDeleteAction("Discount Vouchers");

        public TextAction DiscountValue = new TextAction("Sale Discount", "", "0", "100", (a, b) =>
        {
            var numbers = new List<int>();

            if (a != null)
                numbers.Add(int.Parse(a));
            if (b != null)
                numbers.Add(int.Parse(b));

            if (numbers.Count > 0)
                return numbers.Max().ToString();

            return null;
        });

        public DecimalAction DecimalDiscount = new DecimalAction("Sale Discount (Decimal)", null, 0, 100);

        public ReadWriteAction Tickets = new ReadWriteAction("Tickets");
        public ReadAction SocialMediaComments = new ReadAction("Social Media Comments");

        public TextAction WorkSchedule = new TextAction(
            "Work Schedule",
            "One or more time slots allowed for operation. Certain actions are not allowed outside work schedule.",
            null,
            "00:00:00 - 23:59:59",
            null,
            (a, b) =>
            {
                var joined = new List<string>();

                if (a != null)
                    joined.AddRange(a.Split(',').Select(x => x.Trim()).ToList());

                if (b != null)
                    joined.AddRange(b.Split(',').Select(x => x.Trim()).ToList());

                return string.Join(", ", joined);
            }
        );
    }
}
