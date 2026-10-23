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
    /// <remarks>
    /// The rule is the one saves use: the compared tree is within this context when reducing it against this context
    /// (<see cref="AccessTreeGenerator.GenerateAccessTree"/>) drops nothing it grants. What the reduction would drop is reported
    /// per action: tick lists, wildcards, rows (the self and empty-or-null rows included) and text values of every combine rule.
    /// A context built from several trees, such as a user's own tree and assigned trees, is within this context when each
    /// of its trees is, as each one's assignment is checked. Each tree is reduced on its own: one tree can't hold what
    /// several grant together (rows beyond a dynamic action's wildcard, grants beneath a group wildcard that go further),
    /// so reducing them as one would drop those and report them as missing. What the trees lose is merged per action.
    /// </remarks>
    public static Dictionary<ActionBase, string> FindInAccessibleActionsOn(this TypeAuthContext typeAuthContext, TypeAuthContext typeAuthContextToCompare)
    {
        var trees = typeAuthContextToCompare.AccessTreeJsonStrings;
        var reducedTrees = new List<(TypeAuthContext From, TypeAuthContext To)>();

        foreach (var json in trees)
        {
            //One tree is reduced as the context holds it; each of several is built on its own.
            var tree = trees.Count == 1 ? typeAuthContextToCompare : new TypeAuthContext(json, typeAuthContextToCompare.ActionTrees);
            var reduced = tree.GenerateAccessTree(typeAuthContext);

            if (Equivalent(json, reduced))
                continue;

            //Written differently is not always less (a pick value 090 is written 90), so what the tree grants is compared with
            //what the reduced tree grants.
            reducedTrees.Add((tree, new TypeAuthContext(reduced, typeAuthContextToCompare.ActionTrees)));
        }

        return Changes(typeAuthContext, reducedTrees, null, Wording.Grant).ToDictionary(x => x.Action!, x => x.Description);
    }

    internal enum Wording
    {
        //As what the saver can't grant: "Write, Delete", "Maximum allowed value is 20. You can't grant 90", "You can't grant /z".
        Grant,

        //As what the saver can't remove: "Write, Delete", "You can't lower 90", "You can't remove /z".
        Remove,

        //As it is: "Write, Delete", "90", "/z".
        Plain,
    }

    //What `from` grants that `to` doesn't, other than what `excepted` grants and, when given, only what `within` grants,
    //per action, worded for the saver.
    internal static List<AccessTreeChange> Changes(TypeAuthContext saver, TypeAuthContext from, TypeAuthContext to, TypeAuthContext? excepted, Wording wording, string suffix = "", TypeAuthContext? within = null)
        => Changes(saver, new[] { (from, to) }, excepted, wording, suffix, within);

    //The same over several trees, each with the tree it is reduced to: what any tree grants that its own reduced tree
    //doesn't, a row only in the trees that list it, merged per action.
    internal static List<AccessTreeChange> Changes(TypeAuthContext saver, IReadOnlyList<(TypeAuthContext From, TypeAuthContext To)> trees, TypeAuthContext? excepted, Wording wording, string suffix = "", TypeAuthContext? within = null)
    {
        var changes = new List<AccessTreeChange>();

        if (trees.Count == 0)
            return changes;

        foreach (var action in Actions(trees[0].From.ActionTree))
        {
            var parts = action is ITextAccessProperties textProps
                ? MissingValues(saver, trees, excepted, within, action, textProps, wording)
                : MissingAccesses(saver, trees, excepted, within, action, wording);

            if (parts.Count > 0)
                changes.Add(new AccessTreeChange(action.Name ?? action.Path ?? "", action.Path ?? "", action, string.Join("; ", parts) + suffix));
        }

        return changes;
    }

    //An access the saver holds on the action itself is missing only when it comes from a group wildcard: a wildcard is
    //given only by the same wildcard.
    private const string FromGroupWildcard = " (from a group wildcard you don't hold)";

    private static List<string> MissingAccesses(TypeAuthContext saver, IReadOnlyList<(TypeAuthContext From, TypeAuthContext To)> trees, TypeAuthContext? excepted, TypeAuthContext? within, ActionBase action, Wording wording)
    {
        var missing = new List<string>();
        var accesses = Grantable(action).ToList();

        var wildCard = accesses.Where(access => trees.Any(x => x.From.Can(action, access) && !x.To.Can(action, access)) && excepted?.Can(action, access) != true && within?.Can(action, access) != false).ToList();

        if (wildCard.Count > 0)
            missing.Add(Describe(wildCard, access => wording != Wording.Plain && saver.Can(action, access)));

        if (action is DynamicAction dynamicAction)
        {
            var rows = trees.Select(x => RowIds(x.From, dynamicAction).ToList()).ToList();

            foreach (var id in rows.SelectMany(x => x).Distinct().OrderBy(x => x, StringComparer.Ordinal))
            {
                var listing = trees.Where((_, i) => rows[i].Contains(id)).ToList();
                var row = accesses.Where(access => listing.Any(x => x.From.Can(action, access, id) && !x.To.Can(action, access, id)) && excepted?.Can(action, access, id) != true && within?.Can(action, access, id) != false).ToList();

                if (row.Count > 0)
                    missing.Add($"{RowName(dynamicAction, id)}: {Describe(row, access => wording != Wording.Plain && saver.Can(action, access, id))}");
            }
        }

        return missing;
    }

    private static string Describe(List<Access> accesses, Func<Access, bool> heldOnTheAction)
    {
        var parts = new List<string>();
        var notHeld = accesses.Where(access => !heldOnTheAction(access)).ToList();
        var throughWildcard = accesses.Where(heldOnTheAction).ToList();

        if (notHeld.Count > 0)
            parts.Add(string.Join(", ", notHeld));

        if (throughWildcard.Count > 0)
            parts.Add(string.Join(", ", throughWildcard) + FromGroupWildcard);

        return string.Join("; ", parts);
    }

    private static List<string> MissingValues(TypeAuthContext saver, IReadOnlyList<(TypeAuthContext From, TypeAuthContext To)> trees, TypeAuthContext? excepted, TypeAuthContext? within, ActionBase action, ITextAccessProperties textProps, Wording wording)
    {
        var missing = new List<string>();
        var combine = textProps.Combine;
        var minimum = textProps.MinimumAccess;
        var maximum = textProps.MaximumAccess;

        var ids = new List<string?> { null };
        var rows = new List<List<string>>();

        if (action is DynamicAction dynamicAction)
        {
            rows = trees.Select(x => RowIds(x.From, dynamicAction).ToList()).ToList();
            ids.AddRange(rows.SelectMany(x => x).Distinct().OrderBy(x => x, StringComparer.Ordinal));
        }

        foreach (var id in ids)
        {
            //The value of each tree that gives it (every tree the action's own value, the trees that list a row the row's),
            //where its reduced tree doesn't keep it.
            var lost = trees.Where((_, i) => id == null || rows[i].Contains(id))
                .Select(x => (Value: x.From.GetTextAccessValue(action, textProps, id), Kept: x.To.GetTextAccessValue(action, textProps, id)))
                .Where(x => !combine.Within(x.Kept, x.Value, minimum, maximum)).ToList();

            if (lost.Count == 0)
                continue;

            //The value the trees give together, as the context combines them.
            var value = combine.Fold(lost.Select(x => x.Value), minimum, maximum);
            var other = excepted?.GetTextAccessValue(action, textProps, id);
            var inside = within?.GetTextAccessValue(action, textProps, id);
            var allowed = saver.GetTextAccessValue(action, textProps, id);
            string? message;

            if (combine.IsList)
            {
                //Each entry once, in ordinal order when several trees give them, as a combined list is.
                var notCovered = lost.Count == 1
                    ? combine.NotCovered(lost[0].Kept, lost[0].Value, minimum, maximum)
                    : lost.SelectMany(x => combine.NotCovered(x.Kept, x.Value, minimum, maximum)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();

                var entries = notCovered
                    .Where(entry => excepted == null || !combine.Within(other, entry, minimum, maximum))
                    .Where(entry => within == null || combine.Within(inside, entry, minimum, maximum)).ToList();
                var notHeld = entries.Where(entry => wording == Wording.Plain || !combine.Within(allowed, entry, minimum, maximum)).ToList();
                var throughWildcard = entries.Except(notHeld).ToList();
                var verb = wording == Wording.Remove ? "You can't remove " : wording == Wording.Grant ? "You can't grant " : "";

                message = entries.Count == 0 ? null : string.Join("; ", new[]
                {
                    notHeld.Count > 0 ? verb + string.Join(", ", notHeld) : null,
                    throughWildcard.Count > 0 ? verb + string.Join(", ", throughWildcard) + FromGroupWildcard : null,
                }.Where(x => x != null));
            }
            else if (excepted != null && combine.Within(other, value, minimum, maximum))
                message = null;
            else if (within != null && !combine.Within(inside, value, minimum, maximum))
                message = null;
            else if (wording == Wording.Plain)
                message = value;
            else if (combine.Within(allowed, value, minimum, maximum))
                message = (wording == Wording.Remove ? "You can't lower " : "You can't grant ") + value + FromGroupWildcard;
            else if (wording == Wording.Remove)
                message = $"You can't lower {value}";
            else
                message = allowed == null ? $"You can't grant {value}" : $"Maximum allowed value is {allowed}. You can't grant {value}";

            if (message != null)
                missing.Add(id == null ? message : $"{RowName((DynamicAction)action, id)}: {message}");
        }

        return missing;
    }

    //A group's wildcard that `from` has and `to` doesn't, where no action beneath it already says so: written for the
    //topmost group only.
    internal static List<AccessTreeChange> GroupChanges(TypeAuthContext saver, TypeAuthContext from, TypeAuthContext to, List<AccessTreeChange> actionChanges, string notHeldSuffix, string heldSuffix)
    {
        var changes = new List<AccessTreeChange>();
        var toNodes = new List<ActionTreeNode>();
        var saverNodes = new List<ActionTreeNode>();

        AccessTreeTraverser.FlattenActionTree(toNodes, to.ActionTree);
        AccessTreeTraverser.FlattenActionTree(saverNodes, saver.ActionTree);

        void Walk(ActionTreeNode node, List<Access> reported)
        {
            foreach (var child in node.ActionTreeItems.Where(x => !x.IsADynamicSubItem && x.Action == null))
            {
                var other = toNodes.FirstOrDefault(x => x.Path == child.Path)?.WildCardAccess ?? new List<Access>();
                var held = saverNodes.FirstOrDefault(x => x.Path == child.Path)?.WildCardAccess ?? new List<Access>();
                var missing = child.WildCardAccess.Where(x => !other.Contains(x) && !reported.Contains(x)).ToList();

                //Said already by an action beneath it.
                if (actionChanges.Any(x => x.Path.StartsWith(child.Path + ".", StringComparison.Ordinal)))
                    missing.Clear();

                missing = Enum.GetValues(typeof(Access)).Cast<Access>().Where(missing.Contains).ToList();

                if (missing.Count > 0)
                {
                    var notHeld = missing.Where(x => !held.Contains(x)).ToList();
                    var inReach = missing.Where(held.Contains).ToList();

                    var parts = new List<string>();

                    if (notHeld.Count > 0)
                        parts.Add(string.Join(", ", notHeld) + notHeldSuffix);

                    if (inReach.Count > 0)
                        parts.Add(string.Join(", ", inReach) + heldSuffix);

                    changes.Add(new AccessTreeChange(child.DisplayName ?? child.ID, child.Path ?? "", null, string.Join("; ", parts)));
                }

                Walk(child, reported.Concat(missing).ToList());
            }
        }

        Walk(from.ActionTree, new List<Access>());

        return changes;
    }

    //The accesses checked for an action, as before 2.0: a boolean, read, read-write or read-write-delete action is
    //checked for the accesses it can be given; other actions for every access.
    internal static IEnumerable<Access> Grantable(ActionBase action)
    {
        if (action is BooleanAction)
            return new[] { Access.Maximum };

        if (action is ReadAction)
            return new[] { Access.Read };

        if (action is ReadWriteAction)
            return new[] { Access.Read, Access.Write };

        if (action is ReadWriteDeleteAction)
            return new[] { Access.Read, Access.Write, Access.Delete };

        return Enum.GetValues(typeof(Access)).Cast<Access>();
    }

    private static IEnumerable<string> RowIds(TypeAuthContext context, DynamicAction action)
    {
        return context.TypeAuthContextHelper.LocateActionInBank(action)
            .SelectMany(x => x.SubActionBankItems)
            .Select(x => (x.Action as DynamicAction)!.Id!)
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal);
    }

    private static string RowName(DynamicAction action, string id)
    {
        var item = action.Items.FirstOrDefault(x => x.Key == id);

        if (item.Value != null)
            return item.Value;

        if (id == TypeAuthContext.SelfReferenceKey)
            return "Self";

        if (id == TypeAuthContext.EmptyOrNullKey)
            return "Unassigned";

        return id;
    }

    private static IEnumerable<ActionBase> Actions(ActionTreeNode node)
    {
        foreach (var child in node.ActionTreeItems)
        {
            if (child.IsADynamicSubItem)
                continue;

            if (child.Action != null)
                yield return child.Action;

            foreach (var action in Actions(child))
                yield return action;
        }
    }
}
