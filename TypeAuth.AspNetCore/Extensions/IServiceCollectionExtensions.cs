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
    /// <param name="actionTrees">Type of action tree classes</param>
    /// <returns></returns>
    public static IServiceCollection AddTypeAuth(this IServiceCollection services,params Type[] actionTrees)
    {
        //Create custom TypeAuthContextBuilder with action trees for dependency injection
        services.AddScoped<TypeAuthContextBuilder>(x =>
        {
            var typeAuthContext = new TypeAuthContextBuilder();

            if (actionTrees is not null)
                foreach (var actionTree in actionTrees)
                    typeAuthContext.AddActionTree(actionTree);

            return typeAuthContext;
        });

        return services.AddScoped<TypeAuthService>();
    }

    public static IServiceCollection AddTypeAuth(this IServiceCollection services,Action<TypeAuthOptions> options)
    {
        TypeAuthOptions typeAuthOptions = new();

        options.Invoke(typeAuthOptions);

        //Create custom TypeAuthContextBuilder with action trees for dependency injection
        services.AddScoped<TypeAuthContextBuilder>(x =>
        {
            var typeAuthContext = new TypeAuthContextBuilder();

            if (typeAuthOptions.ActionTrees is not null)
                foreach (var actionTree in typeAuthOptions.ActionTrees)
                    typeAuthContext.AddActionTree(actionTree);

            return typeAuthContext;
        });

        return services.AddScoped<TypeAuthService>();
    }
}
