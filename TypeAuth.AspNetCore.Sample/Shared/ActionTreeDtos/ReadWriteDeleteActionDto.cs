using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos
{
    public class ReadWriteDeleteActionDto
    {
        public bool Read { get; set; }
        public bool Write { get; set; }
        public bool Delete { get; set; }
    }
}
