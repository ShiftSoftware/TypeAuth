using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShiftSoftware.TypeAuth.Blazor.Services;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.Blazor.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Set dependency injections for TypeAuth
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services, Action<TypeAuthBlazorOptions> options)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        //Create custom TypeAuthOptions with action trees for dependency injection
        services.TryAddScoped(x =>
        {
            TypeAuthBlazorOptions typeAuthOptions = new();
            options.Invoke(typeAuthOptions);

            return typeAuthOptions;
        });

        services.TryAddScoped<ITypeAuthService, BlazorTypeAuthService>();

        return services;
    }
}
