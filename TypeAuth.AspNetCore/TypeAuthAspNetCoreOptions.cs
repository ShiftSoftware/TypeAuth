namespace ShiftSoftware.TypeAuth.AspNetCore;

public class TypeAuthAspNetCoreOptions
{
    internal List<Type> ActionTrees { get; private set; }

    public TypeAuthAspNetCoreOptions()
    {
        ActionTrees = new();
    }

    public TypeAuthAspNetCoreOptions AddActionTree<T>()
    {
        ActionTrees.Add(typeof(T));

        return this;
    }
}
