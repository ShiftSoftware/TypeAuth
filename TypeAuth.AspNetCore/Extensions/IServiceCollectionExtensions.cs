using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShiftSoftware.TypeAuth.AspNetCore.Services;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Set dependency injections for TypeAuth with action tree configuration.
    /// Multiple calls to <c>services.Configure&lt;TypeAuthAspNetCoreOptions&gt;(...)</c> are additive.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services, Action<TypeAuthAspNetCoreOptions> configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.Configure(configure);

        return services.AddTypeAuth();
    }

    /// <summary>
    /// Set dependency injections for TypeAuth without configuring action trees.
    /// Action trees can be registered separately via <c>services.Configure&lt;TypeAuthAspNetCoreOptions&gt;(o => o.AddActionTree&lt;T&gt;())</c>.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddHttpContextAccessor();
        services.TryAddScoped<ITypeAuthService, AspNetCoreTypeAuthService>();

        return services;
    }
}
