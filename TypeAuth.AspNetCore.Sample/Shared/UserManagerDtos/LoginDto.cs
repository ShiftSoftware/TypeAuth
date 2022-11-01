using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Sample.Shared.UserManagerDtos
{
    public class LoginDto
    {
        [Required]
        [MaxLength(255)]
        public string Username { get; set; }

        [Required]
        [MaxLength(50)]
        public string Password { get; set; }
    }
}
