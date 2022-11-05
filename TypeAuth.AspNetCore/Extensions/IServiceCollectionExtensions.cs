using Microsoft.Extensions.DependencyInjection;
using ShiftSoftware.TypeAuth.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeAuth.AspNetCore.Services;

namespace TypeAuth.AspNetCore.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Set dependency injections for TypeAuth,
    /// And must add AddHttpContextAccessor to the service collection to make this work correctly
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services,Action<TypeAuthAspNetCoreOptions> options)
    {
        //Create custom TypeAuthOptions with action trees for dependency injection
        services.AddScoped<TypeAuthAspNetCoreOptions>(x =>
        {
            TypeAuthAspNetCoreOptions typeAuthOptions = new();
            options.Invoke(typeAuthOptions);

            return typeAuthOptions;
        });

        return services.AddScoped<TypeAuthService>();
    }
}
