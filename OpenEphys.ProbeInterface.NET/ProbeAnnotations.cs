using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Class holding the <see cref="Probe"/> annotations.
    /// </summary>
    public class ProbeAnnotations
    {
        /// <summary>
        /// Gets the name of the probe as defined by the manufacturer, or a descriptive name such as the neurological target.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the name of the manufacturer who created the probe.
        /// </summary>
        [JsonProperty("manufacturer")]
        public string Manufacturer {  get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeAnnotations"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="manufacturer"></param>
        [JsonConstructor]
        public ProbeAnnotations(string name, string manufacturer)
        {
            Name = name;
            Manufacturer = manufacturer;
        }

        /// <summary>
        /// Copy constructor that copies data from an existing <see cref="ProbeAnnotations"/> object.
        /// </summary>
        /// <param name="probeAnnotations"></param>
        protected ProbeAnnotations(ProbeAnnotations probeAnnotations)
        {
            Name = probeAnnotations.Name;
            Manufacturer = probeAnnotations.Manufacturer;
        }
    }
}
