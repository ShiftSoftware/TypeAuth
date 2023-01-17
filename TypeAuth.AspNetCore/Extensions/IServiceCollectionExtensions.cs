using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShiftSoftware.TypeAuth.AspNetCore.Services;

namespace ShiftSoftware.TypeAuth.AspNetCore.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Set dependency injections for TypeAuth
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services,Action<TypeAuthAspNetCoreOptions> options)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        //Register HttpContextAccessor for dependency injection,
        //because TypeAuthService need it
        services.AddHttpContextAccessor();

        //Create custom TypeAuthOptions with action trees for dependency injection
        services.TryAddScoped<TypeAuthAspNetCoreOptions>(x =>
        {
            TypeAuthAspNetCoreOptions typeAuthOptions = new();
            options.Invoke(typeAuthOptions);

            return typeAuthOptions;
        });

        services.TryAddScoped<TypeAuthService>();

        return services;
    }
}
