﻿using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq;

namespace ShiftSoftware.TypeAuth.Shared.ActionTrees
{
    [ActionTree("Data Level Actions", "Actions Data Level Access.")]
    public class DataLevel
    {
        public DynamicBooleanAction Cities = new DynamicBooleanAction("Cities");
        public DynamicReadAction Countries = new DynamicReadAction("Countries");
        public DynamicReadWriteAction Companies = new DynamicReadWriteAction("Companies");
        public DynamicReadWriteDeleteAction Departments = new DynamicReadWriteDeleteAction("Departments");
        public DynamicTextAction DiscountByDepartment = new DynamicTextAction("Discount", "", "0", "100", (a, b) =>
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

        public DynamicDecimalAction DiscountByDepartmentDecimal = new DynamicDecimalAction("Discount (Decimal)", "", 0, 100);
    }
}
