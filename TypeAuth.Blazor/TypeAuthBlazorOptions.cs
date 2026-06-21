namespace ShiftSoftware.TypeAuth.Blazor;

public class TypeAuthBlazorOptions
{
    internal List<Type> ActionTrees { get; private set; }

    public TypeAuthBlazorOptions()
    {
        ActionTrees = new();
    }

    public TypeAuthBlazorOptions AddActionTree<T>()
        => AddActionTree(typeof(T));

    public TypeAuthBlazorOptions AddActionTree(Type actionTree)
    {
        // Idempotent: registering the same action tree twice (e.g. explicitly here and again from a
        // feature such as tagging) is a no-op instead of a duplicate entry.
        if (!ActionTrees.Contains(actionTree))
            ActionTrees.Add(actionTree);

        return this;
    }
}
