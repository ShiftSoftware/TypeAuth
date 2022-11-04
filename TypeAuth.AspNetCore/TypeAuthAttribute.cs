using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuth.AspNetCore.Services;

namespace TypeAuth.AspNetCore
{
    public class TypeAuthAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly Type actionTreeType;
        private readonly string actionName;
        private readonly Access access;

        public TypeAuthAttribute(Type actionTreeType, string actionName, Access access): base()
        {
            this.actionTreeType = actionTreeType;
            this.actionName = actionName;
            this.access = access;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var typeAuthService = (TypeAuthService)context?.HttpContext?.RequestServices.GetService(typeof(TypeAuthService));
            
            //Check for authrization
            if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //Check for authentication
            if (!typeAuthService.TypeAuthContext.Can(actionTreeType, actionName, access))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
