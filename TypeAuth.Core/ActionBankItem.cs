using Action = ShiftSoftware.TypeAuth.Core.Actions.Action;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class ActionBankItem
    {
        public Action Action { get; set; }
        public List<Access> AccessList { get; set; }
        public string? AccessValue { get; set; }

        public ActionBankItem(Action action, List<Access> accessTypes, string? acessValue = null)
        {
            this.Action = action;
            this.AccessList = accessTypes;
            this.AccessValue = acessValue;
        }
    }
}
