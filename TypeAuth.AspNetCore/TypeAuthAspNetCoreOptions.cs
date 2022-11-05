using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore;

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
