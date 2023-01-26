using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShiftSoftware.TypeAuth.Blazor.Providers;

public interface ITokenProvider
{
    public string? GetToken();
}
