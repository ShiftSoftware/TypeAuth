using System;
using System.Collections.Generic;
using System.Text;

namespace ShiftSoftware.TypeAuth.Core
{
    public class DynamicActionList<T> : List<T> where T : Actions.Action
    {
        public void AddSlefReference(string name, string? description = null)
        {
            T instance = (T)Activator.CreateInstance(typeof(T));
            var anItem = this.FirstOrDefault();

            if (anItem != null)
            {
                instance.Id = TypeAuthContext.SelfRererenceKey;
                instance.Name = name;
                instance.Description = description;
                instance.Type = anItem.Type;

                this.Insert(0, instance);
            }
        }
    }
}
