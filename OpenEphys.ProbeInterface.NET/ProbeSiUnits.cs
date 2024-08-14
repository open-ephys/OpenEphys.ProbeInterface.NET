using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// SI units for all values relating to location and position.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProbeSiUnits
    {
        /// <summary>
        /// Millimeters [mm].
        /// </summary>
        [EnumMember(Value = "mm")]
        mm = 0,

        /// <summary>
        /// Micrometers [um].
        /// </summary>
        [EnumMember(Value = "um")]
        um = 1,
    }
}
