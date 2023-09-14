
namespace ShiftSoftware.TypeAuth.Core;

public interface IDynamicActionExpressionBuilder
{
    public T GetRequiredService<T>() where T : notnull;
    public Operator CombineWithExistingFiltersWith { get; set; }
    public long? GetUserId();
    public Func<Access, bool> AccessPredicate { get; set; }
}

public enum Operator
{
    Or = 0,
    And = 1,
}