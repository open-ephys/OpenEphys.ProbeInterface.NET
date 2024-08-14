namespace OpenEphys.ProbeInterface
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
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="shape"></param>
        /// <param name="shapeParam"></param>
        /// <param name="device_id"></param>
        /// <param name="contact_id"></param>
        /// <param name="shank_id"></param>
        /// <param name="index"></param>
        public Contact(float posX, float posY, ContactShape shape, ContactShapeParam shapeParam,
            int device_id, string contact_id, string shank_id, int index)
        {
            PosX = posX;
            PosY = posY;
            Shape = shape;
            ShapeParams = shapeParam;
            DeviceId = device_id;
            ContactId = contact_id;
            ShankId = shank_id;
            Index = index;
        }
    }
}
