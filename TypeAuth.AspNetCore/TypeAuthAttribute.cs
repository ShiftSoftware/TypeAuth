﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class TypeAuthAttribute<TActionTree> : TypeAuthAttribute
{
    public TypeAuthAttribute(string actionName, Access access) :
        base(typeof(TActionTree), actionName, access)
    {

    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
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
        var service = context.HttpContext.RequestServices.GetService(typeof(ITypeAuthService));

        if(service == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var typeAuthService = (TypeAuthContext) service;
        
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
