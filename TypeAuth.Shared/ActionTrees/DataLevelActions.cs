using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Shared
{
    [ActionTree("Data Level Actions", "Actions Data Level Access.")]
    public class DataLevel
    {
        public readonly static DynamicAction<BooleanAction> Cities = new DynamicAction<BooleanAction>("Self City");
        public readonly static DynamicAction<ReadAction> Countries = new DynamicAction<ReadAction>("Self Country");
        public readonly static DynamicAction<ReadWriteAction> Companies = new DynamicAction<ReadWriteAction>("Self Company");
        public readonly static DynamicAction<ReadWriteDeleteAction> Departments = new DynamicAction<ReadWriteDeleteAction>("Self Department");
        public readonly static DynamicAction<TextAction> DiscountByDepartment = new DynamicAction<TextAction>("Self Department", (a, b) =>
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
    }
}
