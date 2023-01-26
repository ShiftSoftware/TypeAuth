using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    public abstract class DynamicActionBase
    {
        internal Dictionary<string, Actions.Action> Dictionary { get; set; } = new Dictionary<string, Actions.Action>();
       
        internal abstract Actions.Action GenerateAction(string Id);

        internal Actions.Action? UnderlyingAction { get; set; }
    }

    public class DynamicAction<T> : DynamicActionBase where T : Actions.Action, new()
    {
        private string? MaxAccess { get; set; }
        private string? MinAccess { get; set; }

        internal Func<string?, string?, string?>? Comparer { get;set; }
        
        internal override Actions.Action GenerateAction(string id)
        {
            var action = new T() { Id = id };

            if (action.Type == ActionType.Text)
            {
                var textAccess = ((action) as TextAction)!;

                textAccess.Comparer = Comparer;

                textAccess.MinimumAccess = this.MinAccess;
                
                textAccess.MaximumAccess = this.MaxAccess;
            }

            return action;
        }
        
        public DynamicAction() { }

        public DynamicAction(string? selfActionName = null, Func<string?, string?, string?>? comparer = null, string? minAccess = null, string? maxAccess = null)
        {
            this.Comparer = comparer;

            this.MinAccess = minAccess;

            this.MaxAccess = maxAccess;

            var action = new T();

            action.Name = selfActionName;

            if (typeof(T) == typeof(TextAction))
            {
                var textAction = ((TextAction)(object)action);

                textAction.Comparer = this.Comparer;

                textAction.MinimumAccess = this.MinAccess;

                textAction.MaximumAccess = this.MaxAccess;
            }

            this.Dictionary[TypeAuthContext.SelfRererenceKey] = action;
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
