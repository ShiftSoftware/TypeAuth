using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace TypeAuthTests.ERP.ActionTrees
{
    [ActionTree("Data Level Actions", "Actions Data Level Access.")]
    public class DataLevel
    {
        public readonly static DynamicActionDictionary<ReadWriteDeleteAction> Departments =
            new DynamicActionDictionary<ReadWriteDeleteAction>("IT");
    }
}
