using Microsoft.AspNetCore.Http;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Services
{
    public class TypeAuthService : TypeAuthContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly TypeAuthAspNetCoreOptions options;

        //public TypeAuthContext TypeAuthContext { get; private set; }

        public TypeAuthService(IHttpContextAccessor httpContextAccessor, TypeAuthAspNetCoreOptions options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options;

            this.BuildTypeAuthConext();
        }

        private void BuildTypeAuthConext()
        {
            //Get the access trees from the token
            var accessTrees = httpContextAccessor.HttpContext.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree)
                .Select(x=> x.Value).ToList();


            if (options.ActionTrees is not null && accessTrees is not null)
                base.Init(accessTrees, options.ActionTrees.ToArray());
        }
    }
}
