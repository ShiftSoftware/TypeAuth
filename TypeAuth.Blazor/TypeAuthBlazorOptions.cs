namespace ShiftSoftware.TypeAuth.Blazor;

public class TypeAuthBlazorOptions
{
    internal List<Type> ActionTrees { get; private set; }

    public TypeAuthBlazorOptions()
    {
        ActionTrees = new();
    }

    public TypeAuthBlazorOptions AddActionTree<T>()
    {
        ActionTrees.Add(typeof(T));

        return this;
    }
}
