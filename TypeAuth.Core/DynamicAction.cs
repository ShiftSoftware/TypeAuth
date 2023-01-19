using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    public abstract class DynamicActionBase
    {
        internal Dictionary<string, Actions.Action> Dictionary { get; set; } = new Dictionary<string, Actions.Action>();
       
        internal abstract Actions.Action GenerateAction();

        internal Actions.Action? UnderlyingAction { get; set; }
    }

    public class DynamicAction<T> : DynamicActionBase where T : Actions.Action, new()
    {
        internal Func<string?, string?, string?>? Comparer { get;set; }

        public DynamicAction() { }

        public DynamicAction(string selfActionName)
        {
            var action = new T();

            action.Name = selfActionName;

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
        }

        internal override Actions.Action GenerateAction()
        {
            return new T();
        }

        public DynamicAction(string selfActionName, Func<string?, string?, string?>? comparer)
        {
            this.Comparer = comparer;

            var action = new T();

            action.Name = selfActionName;

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
        }

        public DynamicAction(Func<string?, string?, string?>? comparer)
        {
            this.Comparer = comparer;
        }

        public DynamicAction<T> ExpandWith(Dictionary<string, string> dynamicEntries)
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
