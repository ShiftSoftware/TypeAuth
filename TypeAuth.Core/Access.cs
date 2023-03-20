using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ShiftSoftware.TypeAuth.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    /// <summary>
    /// When an Action is evaluated, the Access could be one or more of the Values defined here.
    /// </summary>
    public enum Access
    {
        [EnumMember(Value = "r")]
        Read = 1,

        [EnumMember(Value = "w")]
        Write = 2,

        [EnumMember(Value = "d")]
        Delete = 3,

        [EnumMember(Value = "m")]
        Maximum = 4,
    }
}
