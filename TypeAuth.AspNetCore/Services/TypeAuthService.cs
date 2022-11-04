using Microsoft.AspNetCore.Http;
using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Services
{
    public class TypeAuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly TypeAuthContextBuilder typeAuthContextBuilder;

        public TypeAuthContext TypeAuthContext { get; private set; }

        public TypeAuthService(IHttpContextAccessor httpContextAccessor, TypeAuthContextBuilder typeAuthContextBuilder)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.typeAuthContextBuilder = typeAuthContextBuilder;

            BuildTypeAuthConext();
        }

        private void BuildTypeAuthConext()
        {
            //Get the access trees from the token
            var accessTrees = httpContextAccessor.HttpContext.User?.Claims?
                .Where(c => c.Type == TypeAuthClaimTypes.AccessTree).ToList();

            var test = httpContextAccessor.HttpContext.User?.Claims?
                .FirstOrDefault(c => c.Type == TypeAuthClaimTypes.AccessTree);

            //Check if the access trees form the toke is null abort the operation
            if (accessTrees is null)
                return;

            //Add access trees to TypeAuthContextBuilder
            foreach (var accessTree in accessTrees)
                typeAuthContextBuilder.AddAccessTree(accessTree.Value);

            //Build TypeAuthContext
            TypeAuthContext = typeAuthContextBuilder.Build();
        }
    }
}
