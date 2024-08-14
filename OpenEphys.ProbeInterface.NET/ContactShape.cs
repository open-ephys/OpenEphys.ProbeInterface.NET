using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Shape of the contact.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactShape
    {
        /// <summary>
        /// Circle.
        /// </summary>
        [EnumMember(Value = "circle")]
        Circle = 0,

        /// <summary>
        /// Rectangle.
        /// </summary>
        [EnumMember(Value = "rect")]
        Rect = 1,

        /// <summary>
        /// Square.
        /// </summary>
        [EnumMember(Value = "square")]
        Square = 2,
    }
}
