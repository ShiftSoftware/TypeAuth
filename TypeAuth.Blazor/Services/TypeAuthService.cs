using Microsoft.AspNetCore.Components.Authorization;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.Blazor.Services
{
    public class TypeAuthService : TypeAuthContext
    {
        private readonly AuthenticationStateProvider authStateProvider;
        private readonly TypeAuthBlazorOptions options;

        public TypeAuthService(AuthenticationStateProvider authStateProvider, TypeAuthBlazorOptions options)
        {
            this.authStateProvider = authStateProvider;
            this.options = options;

            BuildTypeAuthConext();
        }

        private async Task BuildTypeAuthConext()
        {
            //Get the access trees from the token
            var state = await authStateProvider.GetAuthenticationStateAsync();
            var accessTrees = state.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree)
                .Select(x => x.Value).ToList();

            foreach (var tree in accessTrees)
            {
                Console.WriteLine(tree);
            }


            if (options.ActionTrees is not null && accessTrees is not null)
                base.Init(accessTrees, options.ActionTrees.ToArray());
        }
    }
}
