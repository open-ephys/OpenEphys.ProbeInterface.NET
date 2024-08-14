using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface
{
    /// <summary>
    /// Class holding all of the annotations for each contact.
    /// </summary>
    public class ContactAnnotations
    {
        /// <summary>
        /// Gets the array of strings holding annotations for each contact. Not all indices must have annotations.
        /// </summary>
        public string[] Annotations { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactAnnotations"/> class.
        /// </summary>
        /// <param name="contactAnnotations"></param>
        [JsonConstructor]
        public ContactAnnotations(string[] contactAnnotations)
        {
            Annotations = contactAnnotations;
        }
    }
}
