using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos;

namespace TypeAuth.AspNetCore.Sample.Shared.RoleDtos
{
    public class UpdateRoleDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public CRMActionDto? AccessTree { get; set; }
    }
}
