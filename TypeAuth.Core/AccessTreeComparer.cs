using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Compares stored access-tree grants and finds permission gaps between contexts.
/// </summary>
public static class AccessTreeComparer
{
    /// <summary>
    /// Compares stored grants without requiring registered action trees.
    /// </summary>
    /// <remarks>
    /// Object keys are order-independent. Arrays containing only recognized access values are sets,
    /// including the numeric and named forms accepted by TypeAuth. Null, blank and JSON null documents
    /// are equivalent to an empty root object. Numbers use TypeAuth's JSON parsing rules; quoted values
    /// remain text. Wildcards, per-record IDs, unknown nodes and nested empty values remain distinct.
    /// This does not compare effective permissions or classify additions and reductions. Other arrays
    /// retain their order. Invalid JSON, including duplicate object keys, is compared as ordinal text.
    /// </remarks>
    public static bool Equivalent(string? first, string? second)
    {
        if (string.Equals(first, second, StringComparison.Ordinal)) return true;

        try
        {
            return SameGrants(ReadTree(first), ReadTree(second));
        }
        catch (JsonException)
        {
            // Identical text was handled above. A rewrite of invalid JSON counts as a change.
            return false;
        }
    }

    private static JToken ReadTree(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new JObject();

        using var text = new StringReader(json!);
        // Keep date-shaped text intact. Parsing dates could hide a change to a text action's value.
        using var reader = new JsonTextReader(text) { DateParseHandling = DateParseHandling.None };
        var tree = JToken.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.Comment)
                throw new JsonReaderException("An access tree must contain one JSON value.");
        }
        return tree.Type == JTokenType.Null ? new JObject() : tree;
    }

    private static bool SameGrants(JToken first, JToken second)
    {
        if (first is JObject objectA && second is JObject objectB)
        {
            return objectA.Count == objectB.Count && objectA.Properties().All(property =>
                objectB.TryGetValue(property.Name, StringComparison.Ordinal, out var other) && SameGrants(property.Value, other));
        }

        if (first is JArray arrayA && second is JArray arrayB)
        {
            if (TryGetAccessSet(arrayA, out var accessA) && TryGetAccessSet(arrayB, out var accessB))
                return accessA.SetEquals(accessB);

            return arrayA.Count == arrayB.Count && arrayA.Zip(arrayB, SameGrants).All(same => same);
        }

        if (first is JValue valueA && second is JValue valueB)
        {
            if (first.Type == second.Type) return Equals(valueA.Value, valueB.Value);

            // Compare integer and floating forms without converting large integers to rounded doubles.
            return IsNumber(first) && IsNumber(second) &&
                TryGetDecimal(valueA, out var numberA) && TryGetDecimal(valueB, out var numberB) && numberA == numberB;
        }

        return false;
    }

    private static bool TryGetAccessSet(JArray array, out HashSet<Access> accesses)
    {
        accesses = new HashSet<Access>();
        foreach (var item in array)
        {
            if (item.Type != JTokenType.String && item.Type != JTokenType.Integer) return false;
            try
            {
                var access = item.ToObject<Access>();
                if (!Enum.IsDefined(typeof(Access), access)) return false;
                accesses.Add(access);
            }
            catch (Exception exception) when (exception is JsonException or ArgumentException or OverflowException)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsNumber(JToken token) => token.Type == JTokenType.Integer || token.Type == JTokenType.Float;

    private static bool TryGetDecimal(JValue value, out decimal number)
    {
        var text = value.Value is double floating
            ? floating.ToString("R", CultureInfo.InvariantCulture)
            : Convert.ToString(value.Value, CultureInfo.InvariantCulture);
        if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return false;

        // A value outside decimal's precision must not become equal through rounding or underflow.
        return value.Value is not double original || (double)number == original;
    }

    /// <summary>
    /// Returns the actions and access levels present in <paramref name="typeAuthContextToCompare"/> but not granted in this context.
    /// </summary>
    public static Dictionary<ActionBase, string> FindInAccessibleActionsOn(this TypeAuthContext typeAuthContext, TypeAuthContext typeAuthContextToCompare)
    {
        var inAccessibleActions = new Dictionary<ActionBase, string>();

        foreach (var actionBankItem in typeAuthContextToCompare.TypeAuthContextHelper.ActionBank)
        {
            var action = actionBankItem.Action;

            if (action is not null)
            {
                var inAccessibleForThisAction = new List<object>();

                foreach (var access in actionBankItem.AccessList)
                {
                    if (!typeAuthContext.Can(action, access))
                    {
                        if (action is BooleanAction && access != Access.Maximum)
                            continue;

                        if (action is ReadAction && access != Access.Read)
                            continue;

                        if (action is ReadWriteAction && !(access == Access.Read || access == Access.Write))
                            continue;

                        if (action is ReadWriteDeleteAction && !(access == Access.Read || access == Access.Write || access == Access.Delete))
                            continue;

                        inAccessibleForThisAction.Add(access);
                    }
                }

                if (inAccessibleForThisAction.Count > 0)
                    inAccessibleActions[action] = string.Join(", ", inAccessibleForThisAction);

                if (action is DecimalAction decimalAction)
                {
                    var targetValue = typeAuthContextToCompare.AccessValue(decimalAction);
                    var allowedValue = typeAuthContext.AccessValue(decimalAction);


                    if (targetValue > allowedValue)
                    {
                        inAccessibleActions[action] = $"Maximum allowed value is {allowedValue}. You can't grant {targetValue}";
                    }
                }
            }
        }

        return inAccessibleActions;
    }
}
