using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Services
{
    internal class AspNetCoreTypeAuthService : TypeAuthContext, ITypeAuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly TypeAuthAspNetCoreOptions options;

        public AspNetCoreTypeAuthService(IHttpContextAccessor httpContextAccessor, IOptions<TypeAuthAspNetCoreOptions> options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options.Value;

            this.BuildTypeAuthConext();
        }

        private void BuildTypeAuthConext()
        {
            var accessTrees = new List<string>();

            if (httpContextAccessor.HttpContext is not null)
            {
                //Get the access trees from the token
                accessTrees = httpContextAccessor.HttpContext.User?.Claims?
                    .Where(c => c.Type == TypeAuthClaimTypes.AccessTree)
                    .Select(x => x.Value).ToList();
            }

            if (options.ActionTrees is not null && accessTrees is not null)
                base.Init(accessTrees, options.ActionTrees.ToArray());
        }
    }
}
