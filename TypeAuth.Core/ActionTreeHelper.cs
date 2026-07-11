using ShiftSoftware.TypeAuth.Core.Actions;

using System.Reflection;

namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// Reads the actions that an action tree class declares. An action tree declares an action as
    /// a public static field whose value is an <see cref="ActionBase"/>. This class is the single
    /// owner of that convention: the context builder uses it when it builds the tree, and other
    /// libraries can use it to find an action from a tree type and a field name — for example at
    /// service registration time, before any <see cref="TypeAuthContext"/> exists.
    /// </summary>
    /// <remarks>
    /// These methods only look at the given class itself. They do not search nested action tree
    /// classes — pass the nested class directly. For lookups on a built context by dot-delimited
    /// path (for example an action name that arrives as data at request time), use
    /// <see cref="ITypeAuthService.FindActionTreeNode(string)"/> instead.
    /// </remarks>
    public static class ActionTreeHelper
    {
        /// <summary>
        /// Returns every action the class declares, as (field name, action) pairs. The order is
        /// whatever reflection reports for the fields — usually declaration order, but that is
        /// not guaranteed, so do not build position-dependent logic on it. Public static fields
        /// whose value is not an <see cref="ActionBase"/> are skipped. The returned actions are
        /// the field instances themselves, so they are the same objects a built
        /// <see cref="TypeAuthContext"/> checks against.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, ActionBase>> GetDeclaredActions(Type actionTreeType)
        {
            if (actionTreeType is null)
                throw new ArgumentNullException(nameof(actionTreeType));

            return Enumerate(actionTreeType);

            static IEnumerable<KeyValuePair<string, ActionBase>> Enumerate(Type actionTreeType)
            {
                foreach (var field in actionTreeType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.GetValue(null) is ActionBase action)
                        yield return new KeyValuePair<string, ActionBase>(field.Name, action);
                }
            }
        }

        /// <summary>
        /// Finds the action that the class declares under the given field name. The name must
        /// match exactly (case-sensitive). Returns <c>null</c> when the class has no such field,
        /// or when the field's value is not an <see cref="ActionBase"/>.
        /// </summary>
        public static ActionBase? FindDeclaredAction(Type actionTreeType, string actionName)
        {
            if (actionTreeType is null)
                throw new ArgumentNullException(nameof(actionTreeType));

            if (string.IsNullOrWhiteSpace(actionName))
                return null;

            var field = actionTreeType.GetField(actionName, BindingFlags.Public | BindingFlags.Static);

            return field?.GetValue(null) as ActionBase;
        }
    }
}
