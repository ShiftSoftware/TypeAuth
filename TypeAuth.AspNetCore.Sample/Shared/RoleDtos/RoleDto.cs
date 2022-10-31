using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos;

namespace TypeAuth.AspNetCore.Sample.Shared.RoleDtos
{
    public class RoleDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ActionTreeDto AccessTree { get; set; }
    }
}
