using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Sample.Shared.ActionTrees
{
    [ActionTree("CRM Actions", "Actions Related to the CRM Module.")]
    public class CRMActions
    {
        public readonly static ReadWriteDeleteAction Sales = new ReadWriteDeleteAction("Sales");

        public readonly static TextAction SalesDiscountValue = new TextAction("Sale Discount", "", "0", "100", (a, b) =>
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
