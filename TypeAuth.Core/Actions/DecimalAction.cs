namespace ShiftSoftware.TypeAuth.Core.Actions;

public class DecimalAction : TextAction
{
    public DecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            description,
            minimumAccess?.ToString() ?? null,
            maximumAccess?.ToString() ?? null,
            (a, b) =>
            {
                var numbers = new List<decimal>();

                if (a != null)
                    numbers.Add(decimal.Parse(a));
                if (b != null)
                    numbers.Add(decimal.Parse(b));

                if (numbers.Count > 0)
                    return numbers.Max().ToString();

                return null;
            }
        )
    {

    }
}

public class DynamicDecimalAction : DynamicTextAction
{
    public DynamicDecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            description,
            minimumAccess?.ToString() ?? null,
            maximumAccess?.ToString() ?? null,
            (a, b) =>
            {
                var numbers = new List<decimal>();

                if (a != null)
                    numbers.Add(decimal.Parse(a));
                if (b != null)
                    numbers.Add(decimal.Parse(b));

                if (numbers.Count > 0)
                    return numbers.Max().ToString();

                return null;
            }
        )
    {

    }
}