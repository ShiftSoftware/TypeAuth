
namespace ShiftSoftware.TypeAuth.Core;

public interface IDynamicActionExpressionBuilder
{
    public T GetRequiredService<T>() where T : notnull;
    public long? GetUserId();
    public Func<Access, bool> AccessPredicate { get; set; }
}
