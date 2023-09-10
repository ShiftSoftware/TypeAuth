
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Linq.Expressions;

namespace ShiftSoftware.TypeAuth.Core;

public class DynamicActionFilterBy<Entity>
{
    public DynamicAction DynamicAction { get; set; }
    public ParameterExpression ParameterExpression { get; set; }
    public InvocationExpression InvocationExpression { get; set; }
    public Type TKey { get; set; }
    public Expression<Func<Entity, long?>>? CreatedByUserIDKeySelector { get; set; }
    public Type? DTOType { get; set; }
    public bool ShowNulls { get; set; }

    public DynamicActionFilterBy(DynamicAction dynamicAction, InvocationExpression invocationExpression, ParameterExpression parameterExpression, Type tKey)
    {
        this.DynamicAction = dynamicAction;
        this.InvocationExpression = invocationExpression;
        this.ParameterExpression = parameterExpression;
        this.TKey = tKey;
    }

    public DynamicActionFilterBy<Entity> IncludeNulls()
    {
        this.ShowNulls = true;

        return this;
    }

    public DynamicActionFilterBy<Entity> IncludeCreatedByCurrentUser(Expression<Func<Entity, long?>>? keySelector)
    {
        this.CreatedByUserIDKeySelector = keySelector;

        return this;
    }

    public DynamicActionFilterBy<Entity> DecodeHashId<DTO>()
    {
        DTOType = typeof(DTO);

        return this;
    }
}
