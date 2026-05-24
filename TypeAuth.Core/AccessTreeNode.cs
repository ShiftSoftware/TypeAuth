using System.Globalization;
using Newtonsoft.Json.Linq;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class AccessTreeNode
    {
        public List<Access> AccessArray { get; set; } = new List<Access>();
        public string? AccessValue { get; set; }
        public JObject? AccessObject { get; set; }

        public AccessTreeNode(object? accessCursor)
        {
            if (accessCursor is null)
                return;

            if (accessCursor is JValue jValue)
            {
                if (jValue.Value is not null)
                    AccessValue = Convert.ToString(jValue.Value, CultureInfo.InvariantCulture);
            }
            else if (accessCursor is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    try
                    {
                        AccessArray.Add(item.ToObject<Access>());
                    }
                    catch
                    {
                    }
                }
            }
            else if (accessCursor is JObject jObject)
            {
                AccessObject = jObject;
            }
        }
    }
}
