using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;

namespace TypeAuth.AspNetCore.Sample.Shared.UserDtos
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public List<RoleDto> Roles { get; set; }
    }
}
