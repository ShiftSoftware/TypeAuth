using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.AspNetCore.EndpointFilters;

/// <summary>
/// Fluent helpers that attach a <see cref="TypeAuthEndpointFilter"/> to a minimal API
/// endpoint or group. Mirrors the shape of <see cref="TypeAuthAttribute"/> and the
/// strongly-typed <c>ITypeAuthService.CanRead/CanWrite/CanDelete</c> overloads used
/// inside <c>ShiftEntitySecureControllerAsync</c>.
/// </summary>
public static class TypeAuthEndpointExtensions
{
    // ---- Reflection-based overloads (mirror TypeAuthAttribute) ----

    public static TBuilder RequireTypeAuth<TBuilder>(this TBuilder builder, Type actionTreeType, string actionName, Access access)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilter(new TypeAuthEndpointFilter(actionTreeType, actionName, access));
    }

    public static TBuilder RequireTypeAuth<TActionTree, TBuilder>(this TBuilder builder, string actionName, Access access)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.AddEndpointFilter(new TypeAuthEndpointFilter(typeof(TActionTree), actionName, access));
    }

    // ---- Strongly-typed general overload (explicit action + access) ----

    public static TBuilder RequireTypeAuth<TBuilder>(this TBuilder builder, ActionBase action, Access access)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, access));

    // ---- Strongly-typed overloads (mirror ITypeAuthService.CanRead/Write/Delete) ----

    public static TBuilder RequireTypeAuthRead<TBuilder>(this TBuilder builder, ReadAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Read));

    public static TBuilder RequireTypeAuthRead<TBuilder>(this TBuilder builder, ReadWriteAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Read));

    public static TBuilder RequireTypeAuthRead<TBuilder>(this TBuilder builder, ReadWriteDeleteAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Read));

    public static TBuilder RequireTypeAuthWrite<TBuilder>(this TBuilder builder, ReadWriteAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Write));

    public static TBuilder RequireTypeAuthWrite<TBuilder>(this TBuilder builder, ReadWriteDeleteAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Write));

    public static TBuilder RequireTypeAuthDelete<TBuilder>(this TBuilder builder, ReadWriteDeleteAction action)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter(new TypeAuthEndpointFilter(action, Access.Delete));
}
