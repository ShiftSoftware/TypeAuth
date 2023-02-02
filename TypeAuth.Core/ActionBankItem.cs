using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;
using Action = ShiftSoftware.TypeAuth.Core.Actions.Action;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class ActionBankItem
    {
        public ActionBase Action { get; set; }
        public List<Access> AccessList { get; set; }
        public string? AccessValue { get; set; }

        public List<ActionBankItem> SubActionBankItems { get; set; }

        public ActionBankItem(ActionBase action, List<Access> accessTypes, string? acessValue = null, JObject? accessObject = null)
        {
            this.Action = action;
            this.AccessList = accessTypes;
            this.AccessValue = acessValue;

            this.SubActionBankItems = new List<ActionBankItem>();
            
            if (accessObject != null)
            {
                foreach (var key in accessObject.Properties().Select(x => x.Name))
                {
                    var node = new AccessTreeNode(accessObject[key]!);

                    SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = key, Type = action.Type }, node.AccessArray, node.AccessValue));
                }
            }
        }
    }
}
