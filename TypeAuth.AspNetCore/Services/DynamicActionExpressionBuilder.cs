

using Microsoft.Extensions.DependencyInjection;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Services;

public class DynamicActionExpressionBuilder : IDynamicActionExpressionBuilder
{
    private IServiceProvider ServiceProvider { get; set; }
    private long? UserId { get; set; }
    public Func<Access, bool> AccessPredicate { get; set; }
    public Operator CombineWithExistingFiltersWith { get; set; } = Operator.And;

    public DynamicActionExpressionBuilder(IServiceProvider serviceProvider, Func<Access, bool> accessPredicate, long? LoggedInUserId)
    {
        this.ServiceProvider = serviceProvider;
        this.UserId = LoggedInUserId;
        this.AccessPredicate = accessPredicate;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return this.ServiceProvider.GetRequiredService<T>();
    }

    public long? GetUserId()
    {
        return this.UserId;
    }
}
