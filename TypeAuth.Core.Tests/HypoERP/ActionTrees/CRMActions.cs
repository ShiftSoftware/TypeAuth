using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq;

namespace TypeAuthTests.HypoERP.ActionTrees
{

    [ActionTree("CRM Actions", "Actions Related to the CRM Module.")]
    public class CRMActions
    {
        public readonly static ReadWriteDeleteAction Customers = new ReadWriteDeleteAction("Customers");
        public readonly static ReadWriteDeleteAction DiscountVouchers = new ReadWriteDeleteAction("Discount Vouchers");
       
        public readonly static TextAction DiscountValue = new TextAction("Sale Discount", "", "100", "0", (a, b) =>
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
    }
}
