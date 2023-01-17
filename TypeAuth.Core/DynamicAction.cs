using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    public class DynamicActionDictionaryBase
    {
        internal Dictionary<string, Actions.Action> Dictionary { get; set; } = new Dictionary<string, Actions.Action>();
    }

    public class DynamicAction<T> : DynamicActionDictionaryBase where T : Actions.Action, new()
    {
        private Func<Dictionary<string, string>>? ExpandFunction { get; set; }
        private Func<Task<Dictionary<string, string>>>? ExpandFunctionAsync { get; set; }
        private Func<string?, string?, string?>? Comparer { get;set; }

        public DynamicAction() { }

        public DynamicAction(string selfActionName)
        {
            var action = new T();

            action.Name = selfActionName;

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
        }

        public DynamicAction(string selfActionName, Func<string?, string?, string?>? comparer)
        {
            this.Comparer = comparer;

            var action = new T();

            action.Name = selfActionName;

            if (typeof(T) == typeof(TextAction))
            {
                ((TextAction)(object)action).Comparer = this.Comparer;
            }

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
        }

        public DynamicAction(Func<string?, string?, string?>? comparer)
        {
            this.Comparer = comparer;
        }

        private DynamicAction<T> ExpandWith(Dictionary<string, string> dynamicEntries)
        {
            foreach (var entry in dynamicEntries)
            {
                var action = new T();

                action.Name = entry.Value;

                if (typeof(T) == typeof(TextAction))
                {
                    ((TextAction)(object)action).Comparer = this.Comparer;
                }

                this.Dictionary[entry.Key] = action;
            }

            return this;
        }

        public async Task ExpandAsync(Func<Task<Dictionary<string, string>>> expandFunctionAsync)
        {
            this.ExpandFunctionAsync = expandFunctionAsync;
            this.ExpandWith(await this.ExpandFunctionAsync.Invoke());
        }

        public void Expand(Func<Dictionary<string, string>> expandFunction)
        {
            this.ExpandFunction = expandFunction;
            this.ExpandWith(this.ExpandFunction.Invoke());
        }

        internal void ReExpand()
        {
            this.ExpandWith(this.ExpandFunction!.Invoke());
        }

        internal async Task ReExpandAsync()
        {
            this.ExpandWith(await this.ExpandFunctionAsync!.Invoke());
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
