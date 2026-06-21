namespace ShiftSoftware.TypeAuth.AspNetCore;

public class TypeAuthAspNetCoreOptions
{
    internal List<Type> ActionTrees { get; private set; }

    public TypeAuthAspNetCoreOptions()
    {
        ActionTrees = new();
    }

    public TypeAuthAspNetCoreOptions AddActionTree<T>()
        => AddActionTree(typeof(T));

    public TypeAuthAspNetCoreOptions AddActionTree(Type t)
    {
        // Idempotent: registering the same action tree twice (e.g. explicitly here and again from a
        // feature such as tagging) is a no-op instead of a duplicate entry.
        if (!ActionTrees.Contains(t))
            ActionTrees.Add(t);

        return this;
    }
}
