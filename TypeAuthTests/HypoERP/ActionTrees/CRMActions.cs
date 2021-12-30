using ShiftSoftware.TypeAuth.Core;

namespace TypeAuthTests.HypoERP.ActionTrees
{

    [ActionTree("CRM Actions", "Actions Related to the CRM Module.")]
    public class CRMActions
    {
        public readonly static Action Customers = new Action("Customers", ActionType.ReadWriteDelete);
        public readonly static Action DiscountVouchers = new Action("Discount Vouchers", ActionType.ReadWriteDelete);
        public readonly static Action DiscountValue = new Action("Sale Discount", ActionType.Text, "", "100", "0");
    }
}
