using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Class holding parameters used to draw the contact.
    /// </summary>
    /// <remarks>
    /// Fields are nullable, since not all fields are required depending on the shape selected.
    /// </remarks>
    public class ContactShapeParam
    {
        /// <summary>
        /// Gets the radius of the contact.
        /// </summary>
        /// <remarks>
        /// This is only used to draw <see cref="ContactShape.Circle"/> contacts. Field can be null.
        /// </remarks>
        [JsonProperty("radius")]
        public float? Radius { get; protected set; }

        /// <summary>
        /// Gets the width of the contact.
        /// </summary>
        /// <remarks>
        /// This is used to draw <see cref="ContactShape.Square"/> or <see cref="ContactShape.Rect"/> contacts.
        /// Field can be null.
        /// </remarks>
        [JsonProperty("width")]
        public float? Width { get; protected set; }

        /// <summary>
        /// Gets the height of the contact.
        /// </summary>
        /// <remarks>
        /// This is only used to draw <see cref="ContactShape.Rect"/> contacts. Field can be null.
        /// </remarks>
        [JsonProperty("height")]
        public float? Height { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactShapeParam"/> class.
        /// </summary>
        /// <param name="radius">Radius. Can be null.</param>
        /// <param name="width">Width. Can be null.</param>
        /// <param name="height">Height. Can be null.</param>
        [JsonConstructor]
        public ContactShapeParam(float? radius = null, float? width = null, float? height = null)
        {
            Radius = radius;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Copy constructor given an existing <see cref="ContactShapeParam"/> object.
        /// </summary>
        /// <param name="shape">Existing <see cref="ContactShapeParam"/> object to be copied.</param>
        protected ContactShapeParam(ContactShapeParam shape)
        {
            Radius = shape.Radius;
            Width = shape.Width;
            Height = shape.Height;
        }
    }
}
