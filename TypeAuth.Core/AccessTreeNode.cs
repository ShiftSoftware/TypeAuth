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
            if (accessCursor == null)
                return;

            if (accessCursor.GetType() == typeof(JValue))
            {
                AccessValue = accessCursor.ToString();
            }
            else if (accessCursor.GetType() == typeof(JArray))
            {
                var theArray = ((JArray)accessCursor).Select(x => x.ToObject<Access>()).ToList();

                AccessArray.AddRange(theArray);
            }
            else if (accessCursor.GetType() == typeof(JObject))
            {
                AccessObject = (JObject)accessCursor;
            }
        }
    }
}
