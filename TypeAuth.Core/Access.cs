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
        /// <summary>
        /// Read-only access.
        /// </summary>
        [EnumMember(Value = "r")]
        Read = 1,

        /// <summary>
        /// Write (create/update) access.
        /// </summary>
        [EnumMember(Value = "w")]
        Write = 2,

        /// <summary>
        /// Delete access.
        /// </summary>
        [EnumMember(Value = "d")]
        Delete = 3,

        /// <summary>
        /// Full/maximum access. Used for boolean actions and wildcard grants.
        /// </summary>
        [EnumMember(Value = "m")]
        Maximum = 4,
    }
}
