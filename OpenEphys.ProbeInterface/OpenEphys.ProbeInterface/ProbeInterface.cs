using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace OpenEphys.ProbeInterface;

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public abstract class ProbeGroup
{
    private string _specification;
    private string _version;
    private IEnumerable<Probe> _probes;

    /// <summary>
    /// String defining the specification of the file. For Probe Interface files, this is expected to be "probeinterface"
    /// </summary>
    [JsonProperty("specification", Required = Required.Always)]
    public string Specification
    {
        get { return _specification; }
        protected set { _specification = value; }
    }

    /// <summary>
    /// String defining which version of Probe Interface was used
    /// </summary>
    [JsonProperty("version", Required = Required.Always)]
    public string Version
    {
        get { return _version; }
        protected set { _version = value; }
    }

    /// <summary>
    /// IEnumerable of probes that are present. Each probe can contain multiple shanks, and each probe has a unique
    /// contour that defines the physical representation of the probe. Contacts have several representations
    /// for their channel number, specifically ContactIds (a string that is not guaranteed to be unique) and
    /// DeviceChannelIds (guaranteed to be unique across all probes). DeviceChannelIds can also be set to -1
    /// to indicate that the channel was not connected or recorded from.
    /// </summary>
    [XmlIgnore()]
    [JsonProperty("probes", Required = Required.Always)]
    public IEnumerable<Probe> Probes
    {
        get { return _probes; }
        protected set { _probes = value; }
    }

    public ProbeGroup(string specification, string version, Probe[] probes)
    {
        _specification = specification;
        _version = version;
        _probes = probes.ToList();

        Validate();
    }

    protected ProbeGroup(ProbeGroup probeGroup)
    {
        _specification = probeGroup._specification;
        _version = probeGroup._version;
        _probes = probeGroup._probes;

        Validate();
    }

    public int NumContacts
    {
        get
        {
            int numContacts = 0;

            foreach (var probe in _probes)
            {
                numContacts += probe.NumberOfChannels;
            }

            return numContacts;
        }
    }

    /// <summary>
    /// Returns the contact IDs of all contacts in all probes. Note that these are not guaranteed to be unique values across probes.
    /// </summary>
    /// <returns>List of strings containing all contact IDs</returns>
    public IEnumerable<string> GetContactIds()
    {
        List<string> contactIds = new();

        foreach (var probe in _probes)
        {
            contactIds.AddRange(probe.ContactIds.ToList());
        }

        return contactIds;
    }

    /// <summary>
    /// Returns device channel indices of all contacts in all probe. Device channel indices are guaranteed to be
    /// unique, unless they are -1
    /// </summary>
    /// <returns></returns>
    public IEnumerable<int> GetDeviceChannelIndices()
    {
        List<int> deviceChannelIndices = new();

        foreach (var probe in _probes)
        {
            deviceChannelIndices.AddRange(probe.DeviceChannelIndices.ToList());
        }

        return deviceChannelIndices;
    }

    /// <summary>
    /// Check that the probe group is consistent in variable lengths, and contains minimally necessary fields
    /// </summary>
    public void Validate()
    {
        if (_specification == null || _version == null || _probes == null)
        {
            throw new Exception("Necessary fields are null, unable to validate properly");
        }

        if (_probes.Count() == 0)
        {
            throw new Exception("No probes are listed, probes must be added during construction");
        }

        if (!ValidateVariableLength(out string result))
        {
            throw new Exception(result);
        }

        SetDefaultContactIdsIfMissing();
        SetEmptyShankIdsIfMissing();
        SetDefaultDeviceChannelIndicesIfMissing();

        if (!ValidateDeviceChannelIndices())
        {
            throw new Exception("Device channel indices are not unique across all probes.");
        }
    }

    private bool ValidateVariableLength(out string result)
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).NumberOfChannels != _probes.ElementAt(i).ContactPositions.Count() ||
                _probes.ElementAt(i).NumberOfChannels != _probes.ElementAt(i).ContactPlaneAxes.Count() ||
                _probes.ElementAt(i).NumberOfChannels != _probes.ElementAt(i).ContactShapeParams.Count() ||
                _probes.ElementAt(i).NumberOfChannels != _probes.ElementAt(i).ContactShapes.Count())
            {
                result = $"Required contact parameters are not the same length in probe {i}. " +
                         "Check positions / plane axes / shapes / shape parameters for lengths.";
                return false;
            }

            if (_probes.ElementAt(i).ContactIds != null &&
                _probes.ElementAt(i).ContactIds.Count() != _probes.ElementAt(i).NumberOfChannels)
            {
                result = $"Contact IDs does not have the correct number of channels for probe {i}";
                return false;
            }

            if (_probes.ElementAt(i).ShankIds != null &&
                _probes.ElementAt(i).ShankIds.Count() != _probes.ElementAt(i).NumberOfChannels)
            {
                result = $"Shank IDs does not have the correct number of channels for probe {i}";
                return false;
            }

            if (_probes.ElementAt(i).DeviceChannelIndices != null &&
                _probes.ElementAt(i).DeviceChannelIndices.Count() != _probes.ElementAt(i).NumberOfChannels)
            {
                result = $"Device Channel Indices does not have the correct number of channels for probe {i}";
                return false;
            }
        }

        result = "";
        return true;
    }

    private void SetDefaultContactIdsIfMissing()
    {
        int contactNum = 0;

        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).ContactIds == null)
            {
                _probes.ElementAt(i).ContactIds = Probe.DefaultContactIds(_probes.ElementAt(i).NumberOfChannels);
            }
            else
                contactNum += _probes.ElementAt(i).NumberOfChannels;
        }
    }

    private void SetEmptyShankIdsIfMissing()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).ShankIds == null)
            {
                _probes.ElementAt(i).ShankIds = Probe.DefaultShankIds(_probes.ElementAt(i).NumberOfChannels);
            }
        }
    }

    private void SetDefaultDeviceChannelIndicesIfMissing()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).DeviceChannelIndices == null)
            {
                _probes.ElementAt(i).DeviceChannelIndices = new int[_probes.ElementAt(i).NumberOfChannels];

                for (int j = 0; j < _probes.ElementAt(i).NumberOfChannels; j++)
                {
                    if (int.TryParse(_probes.ElementAt(i).ContactIds[j], out int result))
                    {
                        _probes.ElementAt(i).DeviceChannelIndices[j] = result;
                    }
                }
            }
        }
    }

    private bool ValidateDeviceChannelIndices()
    {
        var activeChannels = GetDeviceChannelIndices()
                             .Where(index => index != -1);

        if (activeChannels.Count() != activeChannels.Distinct().Count())
        {
            return false;
        }

        return true;
    }
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class Probe
{
    private ProbeNdim _numDimensions;
    private ProbeSiUnits _siUnits;
    private ProbeAnnotations _annotations;
    private ContactAnnotations _contactAnnotations;
    private float[][] _contactPositions;
    private float[][][] _contactPlaneAxes;
    private ContactShape[] _contactShapes;
    private ContactShapeParam[] _contactShapeParams;
    private float[][] _probePlanarContour;
    private int[] _deviceChannelIndices;
    private string[] _contactIds;
    private string[] _shankIds;

    [XmlIgnore()]
    [JsonProperty("ndim", Required = Required.Always)]
    public ProbeNdim NumDimensions
    {
        get { return _numDimensions; }
        protected set { _numDimensions = value; }
    }

    [XmlIgnore()]
    [JsonProperty("si_units", Required = Required.Always)]
    public ProbeSiUnits SiUnits
    {
        get { return _siUnits; }
        protected set { _siUnits = value; }
    }

    [XmlIgnore()]
    [JsonProperty("annotations", Required = Required.Always)]
    public ProbeAnnotations Annotations
    {
        get { return _annotations; }
        protected set { _annotations = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_annotations")]
    public ContactAnnotations ContactAnnotations
    {
        get { return _contactAnnotations; }
        protected set { _contactAnnotations = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_positions", Required = Required.Always)]
    public float[][] ContactPositions
    {
        get { return _contactPositions; }
        protected set { _contactPositions = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_plane_axes")]
    public float[][][] ContactPlaneAxes
    {
        get { return _contactPlaneAxes; }
        protected set { _contactPlaneAxes = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_shapes", Required = Required.Always)]
    public ContactShape[] ContactShapes
    {
        get { return _contactShapes; }
        protected set { _contactShapes = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_shape_params", Required = Required.Always)]
    public ContactShapeParam[] ContactShapeParams
    {
        get { return _contactShapeParams; }
        protected set { _contactShapeParams = value; }
    }

    [XmlIgnore()]
    [JsonProperty("probe_planar_contour")]
    public float[][] ProbePlanarContour
    {
        get { return _probePlanarContour; }
        protected set { _probePlanarContour = value; }
    }

    [XmlIgnore()]
    [JsonProperty("device_channel_indices")]
    public int[] DeviceChannelIndices
    {
        get { return _deviceChannelIndices; }
        internal set { _deviceChannelIndices = value; }
    }

    [XmlIgnore()]
    [JsonProperty("contact_ids")]
    public string[] ContactIds
    {
        get { return _contactIds; }
        internal set { _contactIds = value; }
    }

    [XmlIgnore()]
    [JsonProperty("shank_ids")]
    public string[] ShankIds
    {
        get { return _shankIds; }
        internal set { _shankIds = value; }
    }

    [JsonConstructor]
    public Probe(ProbeNdim ndim, ProbeSiUnits si_units, ProbeAnnotations annotations, ContactAnnotations contact_annotations,
        float[][] contact_positions, float[][][] contact_plane_axes, ContactShape[] contact_shapes,
        ContactShapeParam[] contact_shape_params, float[][] probe_planar_contour, int[] device_channel_indices,
        string[] contact_ids, string[] shank_ids)
    {
        _numDimensions = ndim;
        _siUnits = si_units;
        _annotations = annotations;
        _contactAnnotations = contact_annotations;
        _contactPositions = contact_positions;
        _contactPlaneAxes = contact_plane_axes;
        _contactShapes = contact_shapes;
        _contactShapeParams = contact_shape_params;
        _probePlanarContour = probe_planar_contour;
        _deviceChannelIndices = device_channel_indices;
        _contactIds = contact_ids;
        _shankIds = shank_ids;
    }

    protected Probe(Probe probe)
    {
        _numDimensions = probe._numDimensions;
        _siUnits = probe._siUnits;
        _annotations = probe._annotations;
        _contactAnnotations = probe._contactAnnotations;
        _contactPositions = probe._contactPositions;
        _contactPlaneAxes = probe._contactPlaneAxes;
        _contactShapes = probe._contactShapes;
        _contactShapeParams = probe._contactShapeParams;
        _probePlanarContour = probe._probePlanarContour;
        _deviceChannelIndices = probe._deviceChannelIndices;
        _contactIds = probe._contactIds;
        _shankIds = probe._shankIds;
    }

    /// <summary>
    /// Generates a default ContactShape array that contains the given number of channels and the corresponding shape
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <param name="contactShape">Enumeration of the chosen shape for a contact</param>
    /// <returns></returns>
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
    /// Generates a default contactPlaneAxes array, with each contact given the same axis; { { 1, 0 }, { 0, 1 } }
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <returns></returns>
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
    /// Generates an array of contact shape parameters for the circle, based on the given number of contacts and the given radius
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <param name="radius">Radius of the contact, in the current probe units</param>
    /// <returns></returns>
    public static ContactShapeParam[] DefaultCircleParams(int numberOfContacts, float radius)
    {
        ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

        for (int i = 0; i < numberOfContacts; i++)
        {
            contactShapeParams[i] = new ContactShapeParam(radius:radius, width:null);
        }

        return contactShapeParams;
    }

    /// <summary>
    /// Generates an array of contact shape parameters for the square, based on the given number of contacts and the given width
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <param name="width">Width of the contact, in the current probe units</param>
    /// <returns></returns>
    public static ContactShapeParam[] DefaultSquareParams(int numberOfContacts, float width)
    {
        ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

        for (int i = 0; i < numberOfContacts; i++)
        {
            contactShapeParams[i] = new ContactShapeParam(width:width, radius:null);
        }

        return contactShapeParams;
    }

    /// <summary>
    /// Generates an array of sequential device channel indices, based on the number of contacts and the offset given.
    /// Note that device channel indices must be unique across all probes.
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <param name="offset">The first value of the sequential device channel indices</param>
    /// <returns></returns>
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
    /// Generates a sequential array of contact IDs based on the number of contacts.
    /// Note that contact IDs do not need to be unique across probes.
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <returns></returns>
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
    /// Generates an array of empty strings as the default shank ID
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <returns></returns>
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
    /// Returns a Contact object that contains the position, shape, shape params, and IDs (device / contact / shank)
    /// for a single contact at the given index
    /// </summary>
    /// <param name="index">Relative index of the contact in this Probe</param>
    /// <returns></returns>
    public Contact GetContact(int index)
    {
        return new Contact(ContactPositions[index][0], ContactPositions[index][1], ContactShapes[index], ContactShapeParams[index],
            DeviceChannelIndices[index], ContactIds[index], ShankIds[index], index);
    }

    public int NumberOfChannels => ContactPositions.Length;
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public enum ProbeNdim
{
    [EnumMemberAttribute(Value = "2")]
    _2 = 2,

    [EnumMemberAttribute(Value = "3")]
    _3 = 3,
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ProbeSiUnits
{
    [System.Runtime.Serialization.EnumMemberAttribute(Value = "mm")]
    Mm = 0,

    [System.Runtime.Serialization.EnumMemberAttribute(Value = "um")]
    Um = 1,
}

public readonly struct Contact
{
    public float PosX { get; }
    public float PosY { get; }
    public ContactShape Shape { get; }
    public ContactShapeParam ShapeParams { get; }
    public int DeviceId { get; }
    public string ContactId { get; }
    public string ShankId { get; }
    public int Index { get; }

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

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class ContactShapeParam
{
    private float? _radius;
    private float? _width;

    public float? Radius
    {
        get { return _radius; }
        protected set { _radius = value; }
    }

    public float? Width
    {
        get { return _width; }
        protected set { _width = value; }
    }

    [JsonConstructor]
    public ContactShapeParam(float? radius, float? width)
    {
        _radius = radius;
        _width = width;
    }

    protected ContactShapeParam(ContactShapeParam shape)
    {
        _radius = shape._radius;
        _width = shape._width;
    }
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ContactShape
{
    [EnumMemberAttribute(Value = "circle")]
    Circle = 0,

    [EnumMemberAttribute(Value = "rect")]
    Rect = 1,

    [EnumMemberAttribute(Value = "square")]
    Square = 2,
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class ProbeAnnotations
{
    private string _name;
    private string _manufacturer;

    [JsonProperty("name")]
    public string Name
    {
        get { return _name; }
        protected set { _name = value; }
    }

    [JsonProperty("manufacturer")]
    public string Manufacturer
    {
        get { return _manufacturer; }
        protected set { _manufacturer = value; }
    }

    [JsonConstructor]
    public ProbeAnnotations(string name, string manufacturer)
    {
        _name = name;
        _manufacturer = manufacturer;
    }

    protected ProbeAnnotations(ProbeAnnotations probeAnnotations)
    {
        _name = probeAnnotations._name;
        _manufacturer = probeAnnotations._manufacturer;
    }
}

public class ContactAnnotations
{
    private string[] _contactAnnotations;

    public string[] Annotations
    {
        get { return _contactAnnotations; }
        protected set { _contactAnnotations = value; }
    }

    [JsonConstructor]
    public ContactAnnotations(string[] contactAnnotations)
    {
        _contactAnnotations = contactAnnotations;
    }
}
