using Microsoft.AspNetCore.Http;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.AspNetCore.EndpointFilters;

/// <summary>
/// Minimal-API counterpart of <see cref="TypeAuthAttribute"/>. Mirrors the auth check
/// performed by <see cref="TypeAuthAttribute.OnAuthorization"/>: resolves
/// <see cref="ITypeAuthService"/> from DI, enforces an authenticated user, then calls
/// the appropriate <c>Can*</c> method.
///
/// Two construction modes:
/// <list type="bullet">
///   <item>Reflection-based: <c>(Type actionTreeType, string actionName, Access access)</c>
///   — matches the attribute's original signature.</item>
///   <item>Strongly-typed: <c>(ActionBase action, Access access)</c> — matches the
///   <c>ITypeAuthService.Can(action, access)</c> overload used inside
///   <c>ShiftEntitySecureControllerAsync</c>.</item>
/// </list>
/// </summary>
public sealed class TypeAuthEndpointFilter : IEndpointFilter
{
    private readonly Type? actionTreeType;
    private readonly string? actionName;
    private readonly ActionBase? action;
    private readonly Access access;

    public TypeAuthEndpointFilter(Type actionTreeType, string actionName, Access access)
    {
        this.actionTreeType = actionTreeType;
        this.actionName = actionName;
        this.access = access;
    }

    public TypeAuthEndpointFilter(ActionBase action, Access access)
    {
        this.action = action;
        this.access = access;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var service = context.HttpContext.RequestServices.GetService(typeof(ITypeAuthService));

        if (service is null)
            return Results.Unauthorized();

        if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            return Results.Unauthorized();

        var typeAuthService = (ITypeAuthService)service;

        bool allowed;

        if (action is not null)
        {
            allowed = typeAuthService.Can(action, access);
        }
        else
        {
            // Reflection path — mirrors TypeAuthContextExtensions.Can(Type, string, Access).
            var typeAuthContext = (TypeAuthContext)service;
            allowed = typeAuthContext.Can(actionTreeType!, actionName!, access);
        }

        if (!allowed)
            return Results.Forbid();

        return await next(context);
    }
}
