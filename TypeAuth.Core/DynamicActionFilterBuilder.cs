
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq.Expressions;

namespace ShiftSoftware.TypeAuth.Core;

public class DynamicActionFilterBuilder<Entity>
{
    public List<DynamicActionFilterBy<Entity>> DynamicActionFilters { get; set; } = new List<DynamicActionFilterBy<Entity>>();

    public Func<IDynamicActionExpressionBuilder, Expression<Func<Entity, bool>>>? DynamicActionExpressionBuilder { get; set; }

    public DynamicActionFilterBy<Entity> FilterBy<TKey>(Expression<Func<Entity, TKey>> keySelector, DynamicAction dynamicAction)
    {
        var parameter = Expression.Parameter(typeof(Entity));

        // Build expression for ids.Contains(x.ID)
        var keySelectorInvoke = Expression.Invoke(keySelector, parameter);

        var createdFilter = new DynamicActionFilterBy<Entity>(dynamicAction, keySelectorInvoke, parameter, typeof(TKey));

        DynamicActionFilters.Add(createdFilter);

        return createdFilter;
    }
}