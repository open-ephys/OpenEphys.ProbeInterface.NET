using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Class that implements the Probe Interface specification for a Probe.
    /// </summary>
    public class Probe
    {
        /// <summary>
        /// Gets the <see cref="ProbeNdim"/> to use while plotting the <see cref="Probe"/>.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("ndim", Required = Required.Always)]
        public ProbeNdim NumDimensions {  get; protected set; }

        /// <summary>
        /// Gets the <see cref="ProbeSiUnits"/> to use while plotting the <see cref="Probe"/>.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("si_units", Required = Required.Always)]
        public ProbeSiUnits SiUnits { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ProbeAnnotations"/> for the <see cref="Probe"/>.
        /// </summary>
        /// <remarks>
        /// Used to specify the name of the probe, and the manufacturer.
        /// </remarks>
        [XmlIgnore]
        [JsonProperty("annotations", Required = Required.Always)]
        public ProbeAnnotations Annotations { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ContactAnnotations.ContactAnnotations"/> for the <see cref="Probe"/>.
        /// </summary>
        /// <remarks>
        /// This field can be used for noting things like where it physically is within a specimen, or if it
        /// is no longer functioning correctly.
        /// </remarks>
        [XmlIgnore]
        [JsonProperty("contact_annotations")]
        public ContactAnnotations ContactAnnotations { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Contact"/> positions, specifically the center point of every contact.
        /// </summary>
        /// <remarks>
        /// This is a two-dimensional array of floats; the first index is the index of the contact, and
        /// the second index is the X and Y value, respectively.
        /// </remarks>
        [XmlIgnore]
        [JsonProperty("contact_positions", Required = Required.Always)]
        public float[][] ContactPositions { get; protected set; }

        /// <summary>
        /// Gets the plane axes for the contacts.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("contact_plane_axes")]
        public float[][][] ContactPlaneAxes { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ContactShape"/> for each contact.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("contact_shapes", Required = Required.Always)]
        public ContactShape[] ContactShapes { get; protected set; }

        /// <summary>
        /// Gets the parameters of the shape for each contact.
        /// </summary>
        /// <remarks>
        /// Depending on which <see cref="ContactShape"/>
        /// is selected, not all parameters are needed; for instance, <see cref="ContactShape.Circle"/> only uses
        /// <see cref="ContactShapeParam.Radius"/>, while <see cref="ContactShape.Square"/> just uses
        /// <see cref="ContactShapeParam.Width"/>.
        /// </remarks>
        [XmlIgnore]
        [JsonProperty("contact_shape_params", Required = Required.Always)]
        public ContactShapeParam[] ContactShapeParams { get; protected set; }

        /// <summary>
        /// Gets the outline of the probe that represents the physical shape.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("probe_planar_contour")]
        public float[][] ProbePlanarContour { get; protected set; }

        /// <summary>
        /// Gets the indices of each channel defining their recording channel number. Must be unique, except for contacts
        /// that are set to -1 if they disabled.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("device_channel_indices")]
        public int[] DeviceChannelIndices { get; internal set; }

        /// <summary>
        /// Gets the contact IDs for each channel. These do not have to be unique.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("contact_ids")]
        public string[] ContactIds { get; internal set; }

        /// <summary>
        /// Gets the shank that each contact belongs to.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("shank_ids")]
        public string[] ShankIds { get; internal set; }

        /// <summary>
        /// Public constructor, defined as the default Json constructor.
        /// </summary>
        /// <param name="ndim">Number of dimensions to use while plotting the contacts [<see cref="ProbeNdim.Two"/> or <see cref="ProbeNdim.Three"/>].</param>
        /// <param name="si_units">Real-world units to use while plotting the contacts [<see cref="ProbeSiUnits.mm"/> or <see cref="ProbeSiUnits.um"/>].</param>
        /// <param name="annotations">Annotations for the probe.</param>
        /// <param name="contact_annotations">Annotations for each contact as an array of strings.</param>
        /// <param name="contact_positions">Center position of each contact in a two-dimensional array of floats. For more info, see <see cref="ContactPositions"/>.</param>
        /// <param name="contact_plane_axes">Plane axes of each contact in a three-dimensional array of floats. For more info, see <see cref="ContactPlaneAxes"/>.</param>
        /// <param name="contact_shapes">Array of shapes for each contact.</param>
        /// <param name="contact_shape_params">Array of shape parameters for the each contact.</param>
        /// <param name="probe_planar_contour">Two-dimensional array of floats (X and Y positions) defining a closed shape for a probe contour.</param>
        /// <param name="device_channel_indices">Array of integers containing the device channel indices for each contact. For more info, see <see cref="DeviceChannelIndices"/>.</param>
        /// <param name="contact_ids">Array of strings containing the contact ID for each contact. For more info, see <see cref="ContactIds"/>.</param>
        /// <param name="shank_ids">Array of strings containing the shank ID for each contact. For more info, see <see cref="ShankIds"/>.</param>
        [JsonConstructor]
        public Probe(ProbeNdim ndim, ProbeSiUnits si_units, ProbeAnnotations annotations, ContactAnnotations contact_annotations,
            float[][] contact_positions, float[][][] contact_plane_axes, ContactShape[] contact_shapes,
            ContactShapeParam[] contact_shape_params, float[][] probe_planar_contour, int[] device_channel_indices,
            string[] contact_ids, string[] shank_ids)
        {
            NumDimensions = ndim;
            SiUnits = si_units;
            Annotations = annotations;
            ContactAnnotations = contact_annotations;
            ContactPositions = contact_positions;
            ContactPlaneAxes = contact_plane_axes;
            ContactShapes = contact_shapes;
            ContactShapeParams = contact_shape_params;
            ProbePlanarContour = probe_planar_contour;
            DeviceChannelIndices = device_channel_indices;
            ContactIds = contact_ids;
            ShankIds = shank_ids;
        }

        /// <summary>
        /// Copy constructor given an existing <see cref="Probe"/> object.
        /// </summary>
        /// <param name="probe">Existing <see cref="Probe"/> object to be copied.</param>
        protected Probe(Probe probe)
        {
            NumDimensions = probe.NumDimensions;
            SiUnits = probe.SiUnits;
            Annotations = probe.Annotations;
            ContactAnnotations = probe.ContactAnnotations;
            ContactPositions = probe.ContactPositions;
            ContactPlaneAxes = probe.ContactPlaneAxes;
            ContactShapes = probe.ContactShapes;
            ContactShapeParams = probe.ContactShapeParams;
            ProbePlanarContour = probe.ProbePlanarContour;
            DeviceChannelIndices = probe.DeviceChannelIndices;
            ContactIds = probe.ContactIds;
            ShankIds = probe.ShankIds;
        }

        /// <summary>
        /// Returns default <see cref="ContactShape"/> array that contains the given number of channels and the corresponding shape.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <param name="contactShape">The <see cref="ContactShape"/> to apply to each contact.</param>
        /// <returns><see cref="ContactShape"/> array.</returns>
        public static ContactShape[] DefaultContactShapes(int numberOfContacts, ContactShape contactShape)
        {
            ContactShape[] contactShapes = new ContactShape[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactShapes[i] = contactShape;
            }

            return contactShapes;
        }

        /// <summary>
        /// Returns a default contactPlaneAxes array, with each contact given the same axis; { { 1, 0 }, { 0, 1 } }
        /// </summary>
        /// <remarks>
        /// See Probeinterface documentation for more info.
        /// </remarks>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <returns>Three-dimensional array of <see cref="float"/>s.</returns>
        public static float[][][] DefaultContactPlaneAxes(int numberOfContacts)
        {
            float[][][] contactPlaneAxes = new float[numberOfContacts][][];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactPlaneAxes[i] = new float[2][] { new float[2] { 1.0f, 0.0f }, new float[2] { 0.0f, 1.0f } };
            }

            return contactPlaneAxes;
        }

        /// <summary>
        /// Returns an array of <see cref="ContactShapeParam"/>s for a <see cref="ContactShape.Circle"/>.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <param name="radius">Radius of the contact, in units of <see cref="ProbeSiUnits"/>.</param>
        /// <returns><see cref="ContactShapeParam"/> array.</returns>
        public static ContactShapeParam[] DefaultCircleParams(int numberOfContacts, float radius)
        {
            ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactShapeParams[i] = new ContactShapeParam(radius: radius);
            }

            return contactShapeParams;
        }

        /// <summary>
        /// Returns an array of <see cref="ContactShapeParam"/>s for a <see cref="ContactShape.Square"/>.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <param name="width">Width of the contact, in units of <see cref="ProbeSiUnits"/>.</param>
        /// <returns><see cref="ContactShapeParam"/> array.</returns>
        public static ContactShapeParam[] DefaultSquareParams(int numberOfContacts, float width)
        {
            ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactShapeParams[i] = new ContactShapeParam(width: width);
            }

            return contactShapeParams;
        }

        /// <summary>
        /// Returns an array of <see cref="ContactShapeParam"/>s for a <see cref="ContactShape.Rect"/>.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <param name="width">Width of the contact, in units of <see cref="ProbeSiUnits"/>.</param>
        /// <param name="height">Height of the contact, in units of <see cref="ProbeSiUnits"/>.</param>
        /// <returns><see cref="ContactShapeParam"/> array.</returns>
        public static ContactShapeParam[] DefaultRectParams(int numberOfContacts, float width, float height)
        {
            ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactShapeParams[i] = new ContactShapeParam(width: width, height: height);
            }

            return contactShapeParams;
        }

        /// <summary>
        /// Returns a default array of sequential <see cref="DeviceChannelIndices"/>.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <param name="offset">The first value of the <see cref="DeviceChannelIndices"/>.</param>
        /// <returns>A serially increasing array of <see cref="DeviceChannelIndices"/>.</returns>
        public static int[] DefaultDeviceChannelIndices(int numberOfContacts, int offset)
        {
            int[] deviceChannelIndices = new int[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                deviceChannelIndices[i] = i + offset;
            }

            return deviceChannelIndices;
        }

        /// <summary>
        /// Returns a sequential array of <see cref="ContactIds"/>.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <returns>Array of strings defining the <see cref="ContactIds"/>.</returns>
        public static string[] DefaultContactIds(int numberOfContacts)
        {
            string[] contactIds = new string[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactIds[i] = i.ToString();
            }

            return contactIds;
        }

        /// <summary>
        /// Returns an array of empty strings as the default shank ID.
        /// </summary>
        /// <param name="numberOfContacts">Number of contacts in a single <see cref="Probe"/>.</param>
        /// <returns>Array of empty strings.</returns>
        public static string[] DefaultShankIds(int numberOfContacts)
        {
            string[] contactIds = new string[numberOfContacts];

            for (int i = 0; i < numberOfContacts; i++)
            {
                contactIds[i] = "";
            }

            return contactIds;
        }

        /// <summary>
        /// Returns a <see cref="Contact"/> object.
        /// </summary>
        /// <param name="index">Relative index of the contact in this <see cref="Probe"/>.</param>
        /// <returns><see cref="Contact"/>.</returns>
        public Contact GetContact(int index)
        {
            return new Contact(ContactPositions[index][0], ContactPositions[index][1], ContactShapes[index], ContactShapeParams[index],
                DeviceChannelIndices[index], ContactIds[index], ShankIds[index], index);
        }

        /// <summary>
        /// Gets the number of contacts within this <see cref="Probe"/>.
        /// </summary>
        public int NumberOfContacts => ContactPositions.Length;
    }
}
