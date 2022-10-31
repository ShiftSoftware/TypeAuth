using TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos;

namespace TypeAuth.AspNetCore.Sample.Server.ActionTreeModels
{
    public class UpdateRoleModel
    {
        public string Name { get; set; }

        public CRMActionModel AccessTree { get; set; }
    }
}
