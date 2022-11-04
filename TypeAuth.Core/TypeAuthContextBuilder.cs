using System;
using System.Collections.Generic;
using System.Text;

namespace ShiftSoftware.TypeAuth.Core
{
    public class TypeAuthContextBuilder
    {
        private List<string> AccessTrees { get; set; }
        private List<Type> ActionTrees { get; set; }

        public TypeAuthContextBuilder()
        {
            this.AccessTrees = new List<string>();
            this.ActionTrees = new List<Type>();
        }

        public TypeAuthContextBuilder AddAccessTree(string accessTree)
        {
            this.AccessTrees.Add(accessTree);

            return this;
        }

        public TypeAuthContextBuilder AddActionTree<T>()
        {
            this.ActionTrees.Add(typeof(T));

            return this;
        }

        public TypeAuthContextBuilder AddActionTree(Type actionTreeType)
        {
            this.ActionTrees.Add(actionTreeType);

            return this;
        }

        public TypeAuthContext Build()
        {
            return new TypeAuthContext(this.AccessTrees, this.ActionTrees.ToArray());
        }
    }
}
