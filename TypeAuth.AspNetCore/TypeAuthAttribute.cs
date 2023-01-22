using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.AspNetCore.Services;

namespace ShiftSoftware.TypeAuth.AspNetCore
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
            var service = context.HttpContext.RequestServices.GetService(typeof(TypeAuthService));

            if(service == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var typeAuthService = (TypeAuthService)service;
            
            //Check for authrization
            if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //Check for authentication
            if (!typeAuthService.Can(actionTreeType, actionName, access))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
