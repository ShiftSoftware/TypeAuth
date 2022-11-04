using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeAuth.AspNetCore.Extensions;

public class TypeAuthOptions
{
    internal List<Type> ActionTrees { get; private set; }

    public TypeAuthOptions()
    {
        ActionTrees = new();
    }

    public TypeAuthOptions AddActionTree<T>()
    {
        ActionTrees.Add(typeof(T));

        return this;
    }
}
