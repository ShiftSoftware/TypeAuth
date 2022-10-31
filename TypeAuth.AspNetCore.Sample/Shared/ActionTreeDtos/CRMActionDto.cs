using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Sample.Shared.ActionTreeDtos
{
    public class CRMActionDto
    {
        public ReadWriteDeleteActionDto Sales { get; set; }

        public string SalesDiscountValue { get; set; }

        public CRMActionDto()
        {
            Sales = new();
        }
    }
}
