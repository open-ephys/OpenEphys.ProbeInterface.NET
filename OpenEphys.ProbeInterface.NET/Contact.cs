namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Struct that extends the Probeinterface specification by encapsulating all values for a single contact.
    /// </summary>
    public readonly struct Contact
    {
        /// <summary>
        /// Gets the x-position of the contact.
        /// </summary>
        public float PosX { get; }

        /// <summary>
        /// Gets the y-position of the contact.
        /// </summary>
        public float PosY { get; }

        /// <summary>
        /// Gets the <see cref="ContactShape"/> of the contact.
        /// </summary>
        public ContactShape Shape { get; }

        /// <summary>
        /// Gets the <see cref="ContactShapeParam"/>'s of the contact.
        /// </summary>
        public ContactShapeParam ShapeParams { get; }

        /// <summary>
        /// Gets the device ID of the contact.
        /// </summary>
        public int DeviceId { get; }

        /// <summary>
        /// Gets the contact ID of the contact.
        /// </summary>
        public string ContactId { get; }

        /// <summary>
        /// Gets the shank ID of the contact.
        /// </summary>
        public string ShankId { get; }

        /// <summary>
        /// Gets the index of the contact within the <see cref="Probe"/> object.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contact"/> struct.
        /// </summary>
        /// <param name="posX">Center value of the contact on the X-axis.</param>
        /// <param name="posY">Center value of the contact on the Y-axis.</param>
        /// <param name="shape">The <see cref="ContactShape"/> of the contact.</param>
        /// <param name="shapeParam"><see cref="ContactShapeParam"/>'s relevant to the <see cref="ContactShape"/> of the contact.</param>
        /// <param name="deviceId">The device channel index (<see cref="Probe.DeviceChannelIndices"/>) of this contact.</param>
        /// <param name="contactId">The contact ID (<see cref="Probe.ContactIds"/>) of this contact.</param>
        /// <param name="shankId">The shank ID (<see cref="Probe.ShankIds"/>) of this contact.</param>
        /// <param name="index">The index of the contact within the context of the <see cref="Probe"/>.</param>
        public Contact(float posX, float posY, ContactShape shape, ContactShapeParam shapeParam,
            int deviceId, string contactId, string shankId, int index)
        {
            PosX = posX;
            PosY = posY;
            Shape = shape;
            ShapeParams = shapeParam;
            DeviceId = deviceId;
            ContactId = contactId;
            ShankId = shankId;
            Index = index;
        }
    }
}
