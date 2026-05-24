using System;
using System.Collections.Generic;
using System.Text;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// Fluent builder for constructing a <see cref="TypeAuthContext"/> from one or more access trees and action trees.
    /// </summary>
    public class TypeAuthContextBuilder
    {
        private List<string> AccessTrees { get; set; }
        private List<Type> ActionTrees { get; set; }

        public TypeAuthContextBuilder()
        {
            this.AccessTrees = new List<string>();
            this.ActionTrees = new List<Type>();
        }

        /// <summary>
        /// Adds an access tree (serialized as a JSON string) that defines the permissions granted to a subject.
        /// </summary>
        public TypeAuthContextBuilder AddAccessTree(string accessTreeJSONString = "{}")
        {
            if(accessTreeJSONString == null)
                accessTreeJSONString = "{}";

            this.AccessTrees.Add(accessTreeJSONString);

            return this;
        }

        /// <summary>
        /// Registers an action tree type that defines the available actions to check against.
        /// </summary>
        public TypeAuthContextBuilder AddActionTree<T>()
        {
            this.ActionTrees.Add(typeof(T));

            return this;
        }

        /// <inheritdoc cref="AddActionTree{T}"/>
        public TypeAuthContextBuilder AddActionTree(Type actionTreeType)
        {
            this.ActionTrees.Add(actionTreeType);

            return this;
        }

        /// <summary>
        /// Builds a <see cref="TypeAuthContext"/> from the registered access trees and action trees.
        /// </summary>
        public TypeAuthContext Build()
        {
            return new TypeAuthContext(this.AccessTrees, this.ActionTrees.ToArray());
        }
    }
}
