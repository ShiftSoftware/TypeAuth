﻿using Microsoft.AspNetCore.Components.Authorization;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.Blazor.Services
{
    internal class BlazorTypeAuthService : TypeAuthContext, ITypeAuthService
    {
        private readonly AuthenticationStateProvider authStateProvider;
        private readonly TypeAuthBlazorOptions options;

        public BlazorTypeAuthService(AuthenticationStateProvider authStateProvider, TypeAuthBlazorOptions options)
        {
            this.authStateProvider = authStateProvider;
            this.options = options;


            BuildTypeAuthConext();

            authStateProvider.AuthenticationStateChanged += AuthStateProvider_AuthenticationStateChanged;
        }

        private void AuthStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            BuildTypeAuthConext();
        }

        private static bool disableTypeAuthBuild = false;
        private void BuildTypeAuthConext()
        {
            if (disableTypeAuthBuild)
                return;

            disableTypeAuthBuild = true;

            //Get the access trees from the token
            var state = authStateProvider.GetAuthenticationStateAsync().Result;
            var accessTrees = state.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree)
                .Select(x => x.Value).ToList();

            if (options.ActionTrees is not null && accessTrees is not null)
                base.Init(accessTrees, options.ActionTrees.ToArray());

            disableTypeAuthBuild = false;
        }
    }
}
