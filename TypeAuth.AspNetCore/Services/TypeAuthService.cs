using Microsoft.AspNetCore.Http;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Services
{
    public class TypeAuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly TypeAuthAspNetCoreOptions options;

        public TypeAuthContext TypeAuthContext { get; private set; }

        public TypeAuthService(IHttpContextAccessor httpContextAccessor, TypeAuthAspNetCoreOptions options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options;

            TypeAuthContext = BuildTypeAuthConext();
        }

        private TypeAuthContext BuildTypeAuthConext()
        {
            TypeAuthContextBuilder builder = new();

            //Set action trees to context builder
            if (options.ActionTrees is not null)
                foreach (var actionTree in options.ActionTrees)
                    builder.AddActionTree(actionTree);

            //Get the access trees from the token
            var accessTrees = httpContextAccessor.HttpContext.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree).ToList();

            //Add access trees to TypeAuthContextBuilder
            if (accessTrees is not null)
                foreach (var accessTree in accessTrees)
                    builder.AddAccessTree(accessTree.Value);

            //Build TypeAuthContext
            return builder.Build();
        }
    }
}
