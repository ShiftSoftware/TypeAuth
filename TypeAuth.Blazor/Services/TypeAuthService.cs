﻿using Microsoft.AspNetCore.Components.Authorization;
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

            authStateProvider.AuthenticationStateChanged += AuthStateProvider_AuthenticationStateChanged;
        }

        private void AuthStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            BuildTypeAuthConext();
        }

        private void BuildTypeAuthConext()
        {
            //Get the access trees from the token
            var state = authStateProvider.GetAuthenticationStateAsync().Result;
            var accessTrees = state.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree)
                .Select(x => x.Value).ToList();

            if (options.ActionTrees is not null && accessTrees is not null)
                base.Init(accessTrees, options.ActionTrees.ToArray());
        }
    }
}
