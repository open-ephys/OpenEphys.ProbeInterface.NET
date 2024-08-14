using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface.NET
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
        /// <param name="contactAnnotations">Array of strings containing annotations for each contact. Size of the array should match the number of contacts, but they can be empty strings.</param>
        [JsonConstructor]
        public ContactAnnotations(string[] contactAnnotations)
        {
            Annotations = contactAnnotations;
        }
    }
}
