using System.Runtime.Serialization;

namespace OpenEphys.ProbeInterface
{
    /// <summary>
    /// Number of dimensions to use while plotting a <see cref="Probe"/>.
    /// </summary>
    public enum ProbeNdim
    {
        /// <summary>
        /// Two-dimensions.
        /// </summary>
        [EnumMember(Value = "2")]
        Two = 2,

        /// <summary>
        /// Three-dimensions.
        /// </summary>
        [EnumMember(Value = "3")]
        Three = 3,
    }
}
