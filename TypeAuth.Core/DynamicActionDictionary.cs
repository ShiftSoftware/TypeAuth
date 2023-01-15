namespace ShiftSoftware.TypeAuth.Core
{
    public class DynamicActionDictionaryBase
    {
        internal Dictionary<string, Actions.Action> Dictionary { get; set; } = new Dictionary<string, Actions.Action>();
    }

    public class DynamicActionDictionary<T> : DynamicActionDictionaryBase where T : Actions.Action, new()
    {
        public DynamicActionDictionary() { }

        public DynamicActionDictionary(string selfActionName)
        {
            var action = new T();

            action.Name = selfActionName;

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
        }

        public DynamicActionDictionary<T> ExpandWith(Dictionary<string, string> dynamicEntries)
        {
            foreach (var entry in dynamicEntries)
            {
                var action = new T();

                action.Name = entry.Value;

                this.Dictionary[entry.Key] = action;
            }

            return this;
        }

        public T this[string key]
        {
            get
            {
                return (T)Dictionary[key];
            }
            set { Dictionary.Add(key, value); }
        }
    }
}
