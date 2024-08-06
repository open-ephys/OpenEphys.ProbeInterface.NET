using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace OpenEphys.ProbeInterface;

/// <summary>
/// Abstract class that implements the Probe Interface specification in C#. Allows for JSON files to be loaded in as 
/// <see cref="ProbeGroup"/> objects.
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public abstract class ProbeGroup
{
    private string _specification;
    private string _version;
    private IEnumerable<Probe> _probes;

    /// <summary>
    /// String defining the specification of the file. This is expected to be "probeinterface".
    /// </summary>
    [JsonProperty("specification", Required = Required.Always)]
    public string Specification
    {
        get { return _specification; }
        protected set { _specification = value; }
    }

    /// <summary>
    /// String defining which version of Probe Interface was used.
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
    [XmlIgnore]
    [JsonProperty("probes", Required = Required.Always)]
    public IEnumerable<Probe> Probes
    {
        get { return _probes; }
        protected set { _probes = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProbeGroup"/> class.
    /// </summary>
    /// <param name="specification">Defines the <see cref="Specification"/> parameter</param>
    /// <param name="version">Defines the <see cref="Version"/> parameter</param>
    /// <param name="probes">Defines a list of <see cref="Probe"/> objects</param>
    public ProbeGroup(string specification, string version, IEnumerable<Probe> probes)
    {
        _specification = specification;
        _version = version;
        _probes = probes;

        Validate();
    }

    /// <summary>
    /// Copy constructor that takes in an existing <see cref="ProbeGroup"/> object and copies the individual fields.
    /// </summary>
    /// <param name="probeGroup"></param>
    protected ProbeGroup(ProbeGroup probeGroup)
    {
        _specification = probeGroup._specification;
        _version = probeGroup._version;
        _probes = probeGroup._probes;

        Validate();
    }

    /// <summary>
    /// Number of contacts across all <see cref="Probe"/> objects.
    /// </summary>
    public int NumberOfContacts
    {
        get
        {
            int numContacts = 0;

            foreach (var probe in _probes)
            {
                numContacts += probe.NumberOfContacts;
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
    /// Creates <see cref="Contact"/> objects for every contact in the <see cref="ProbeGroup"/>.
    /// </summary>
    /// <returns><see cref="List{Contact}"/></returns>
    public List<Contact> GetContacts()
    {
        List<Contact> contacts = new();

        foreach (var p in Probes)
        {
            for (int i = 0; i < p.NumberOfContacts; i++)
            {
                contacts.Add(p.GetContact(i));
            }
        }

        return contacts;
    }

    /// <summary>
    /// Returns device channel indices of all contacts in all probe. Device channel indices are guaranteed to be
    /// unique, unless they are -1.
    /// </summary>
    /// <returns><see cref="IEnumerable{Int32}"/></returns>
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
    /// Check that the probe group is consistent in variable lengths, and that it contains minimally necessary fields.
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
        ValidateContactIds();
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
            if (_probes.ElementAt(i).NumberOfContacts != _probes.ElementAt(i).ContactPositions.Count() ||
                _probes.ElementAt(i).NumberOfContacts != _probes.ElementAt(i).ContactPlaneAxes.Count() ||
                _probes.ElementAt(i).NumberOfContacts != _probes.ElementAt(i).ContactShapeParams.Count() ||
                _probes.ElementAt(i).NumberOfContacts != _probes.ElementAt(i).ContactShapes.Count())
            {
                result = $"Required contact parameters are not the same length in probe {i}. " +
                         "Check positions / plane axes / shapes / shape parameters for lengths.";
                return false;
            }

            if (_probes.ElementAt(i).ContactIds != null &&
                _probes.ElementAt(i).ContactIds.Count() != _probes.ElementAt(i).NumberOfContacts)
            {
                result = $"Contact IDs does not have the correct number of channels for probe {i}";
                return false;
            }

            if (_probes.ElementAt(i).ShankIds != null &&
                _probes.ElementAt(i).ShankIds.Count() != _probes.ElementAt(i).NumberOfContacts)
            {
                result = $"Shank IDs does not have the correct number of channels for probe {i}";
                return false;
            }

            if (_probes.ElementAt(i).DeviceChannelIndices != null &&
                _probes.ElementAt(i).DeviceChannelIndices.Count() != _probes.ElementAt(i).NumberOfContacts)
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
                _probes.ElementAt(i).ContactIds = Probe.DefaultContactIds(_probes.ElementAt(i).NumberOfContacts);
            }
            else
                contactNum += _probes.ElementAt(i).NumberOfContacts;
        }
    }

    private void ValidateContactIds()
    {
        CheckIfContactIdsAreZeroIndexed();
    }

    private void CheckIfContactIdsAreZeroIndexed()
    {
        var contactIds = GetContactIds();

        var numericIds = contactIds.Select(c => { return int.Parse(c); })
                                   .ToList();

        var min = numericIds.Min();

        var max = numericIds.Max();

        if (min == 1 && max == NumberOfContacts && numericIds.Count == numericIds.Distinct().Count())
        {
            for (int i = 0; i < _probes.Count(); i++)
            {
                var probe = _probes.ElementAt(i);
                var newContactIds = probe.ContactIds.Select(c => { return (int.Parse(c) - 1).ToString(); });

                for (int j = 0; j < probe.NumberOfContacts; j++)
                {
                    probe.ContactIds.SetValue(newContactIds.ElementAt(j), j);
                }
            }
        }
    }

    private void SetEmptyShankIdsIfMissing()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).ShankIds == null)
            {
                _probes.ElementAt(i).ShankIds = Probe.DefaultShankIds(_probes.ElementAt(i).NumberOfContacts);
            }
        }
    }

    private void SetDefaultDeviceChannelIndicesIfMissing()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).DeviceChannelIndices == null)
            {
                _probes.ElementAt(i).DeviceChannelIndices = new int[_probes.ElementAt(i).NumberOfContacts];

                for (int j = 0; j < _probes.ElementAt(i).NumberOfContacts; j++)
                {
                    if (int.TryParse(_probes.ElementAt(i).ContactIds[j], out int result))
                    {
                        _probes.ElementAt(i).DeviceChannelIndices[j] = result;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Validate the uniqueness of device channel indices; all indices greater than or equal to 0 must be unique,
    /// but there can be as many values equal to -1 as there are contacts. A value of -1 indicates that this contact is 
    /// not being recorded.
    /// </summary>
    /// <returns></returns>
    public bool ValidateDeviceChannelIndices()
    {
        var activeChannels = GetDeviceChannelIndices()
                             .Where(index => index != -1);

        if (activeChannels.Count() != activeChannels.Distinct().Count())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Device channel indices can be updated as contacts are being enabled or disabled. This is done on a 
    /// per-probe basis, where the incoming array of indices must be the same size as the original probe, 
    /// and must follow the standard for uniqueness found in <see cref="Probe.DeviceChannelIndices"/>.
    /// </summary>
    /// <param name="probeIndex"></param>
    /// <param name="deviceChannelIndices"></param>
    /// <exception cref="ArgumentException"></exception>
    public void UpdateDeviceChannelIndices(int probeIndex, int[] deviceChannelIndices)
    {
        if (_probes.ElementAt(probeIndex).DeviceChannelIndices.Length != deviceChannelIndices.Length)
        {
            throw new ArgumentException($"Incoming device channel indices have {deviceChannelIndices.Length} contacts, " +
                $"but the existing probe {probeIndex} has {_probes.ElementAt(probeIndex).DeviceChannelIndices.Length} contacts");
        }    

        _probes.ElementAt(probeIndex).DeviceChannelIndices = deviceChannelIndices;

        if (!ValidateDeviceChannelIndices())
        {
            throw new ArgumentException("Device channel indices are not valid. Ensure that all values are either -1 or are unique.");
        }
    }
}

/// <summary>
/// Class that implements the Probe Interface specification for a Probe.
/// </summary>
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

    /// <summary>
    /// Number of dimensions to use while plotting the probe. Options are 
    /// <see cref="ProbeNdim.Two"/> and <see cref="ProbeNdim.Three"/>.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("ndim", Required = Required.Always)]
    public ProbeNdim NumDimensions
    {
        get { return _numDimensions; }
        protected set { _numDimensions = value; }
    }

    /// <summary>
    /// Units to use while plotting the probe and all contacts. Options are 
    /// <see cref="ProbeSiUnits.um"/> and <see cref="ProbeSiUnits.mm"/>.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("si_units", Required = Required.Always)]
    public ProbeSiUnits SiUnits
    {
        get { return _siUnits; }
        protected set { _siUnits = value; }
    }

    /// <summary>
    /// Annotations related to the probe itself. Used to specify the name of the probe, and the manufacturer.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("annotations", Required = Required.Always)]
    public ProbeAnnotations Annotations
    {
        get { return _annotations; }
        protected set { _annotations = value; }
    }

    /// <summary>
    /// Annotations related to the contacts, noting things like where it physically is within a specimen, or if it
    /// is no longer functioning correctly.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_annotations")]
    public ContactAnnotations ContactAnnotations
    {
        get { return _contactAnnotations; }
        protected set { _contactAnnotations = value; }
    }

    /// <summary>
    /// Contact positions, specifically the center point of every contact. This is a two-dimensional array of
    /// floats; the first index is the index of the contact, and the second index is the X and Y value, respectively.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_positions", Required = Required.Always)]
    public float[][] ContactPositions
    {
        get { return _contactPositions; }
        protected set { _contactPositions = value; }
    }

    /// <summary>
    /// Defines if/how contacts are rotated.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_plane_axes")]
    public float[][][] ContactPlaneAxes
    {
        get { return _contactPlaneAxes; }
        protected set { _contactPlaneAxes = value; }
    }

    /// <summary>
    /// Defines the shape for each contact. Possible options are <see cref="ContactShape.Circle"/>,
    /// <see cref="ContactShape.Rect"/>, and <see cref="ContactShape.Square"/>
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_shapes", Required = Required.Always)]
    public ContactShape[] ContactShapes
    {
        get { return _contactShapes; }
        protected set { _contactShapes = value; }
    }

    /// <summary>
    /// Defines the parameters of the shape for each contact. Depending on which <see cref="ContactShape"/>
    /// is selected, not all parameters are needed; for instance, <see cref="ContactShape.Circle"/> only uses
    /// <see cref="ContactShapeParam.Radius"/>, while <see cref="ContactShape.Square"/> just uses
    /// <see cref="ContactShapeParam.Width"/>.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_shape_params", Required = Required.Always)]
    public ContactShapeParam[] ContactShapeParams
    {
        get { return _contactShapeParams; }
        protected set { _contactShapeParams = value; }
    }

    /// <summary>
    /// Defines the outline of the probe that represents the physical shape. This should fully encapsulate all contacts.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("probe_planar_contour")]
    public float[][] ProbePlanarContour
    {
        get { return _probePlanarContour; }
        protected set { _probePlanarContour = value; }
    }

    /// <summary>
    /// Indices of each channel defining their recording channel number. Must be unique, except for contacts
    /// that are set to -1 if they disabled.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("device_channel_indices")]
    public int[] DeviceChannelIndices
    {
        get { return _deviceChannelIndices; }
        internal set { _deviceChannelIndices = value; }
    }

    /// <summary>
    /// Contact IDs for each channel. These do not have to be unique.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("contact_ids")]
    public string[] ContactIds
    {
        get { return _contactIds; }
        internal set { _contactIds = value; }
    }

    /// <summary>
    /// Defines the shank ID that each contact belongs to.
    /// </summary>
    [XmlIgnore]
    [JsonProperty("shank_ids")]
    public string[] ShankIds
    {
        get { return _shankIds; }
        internal set { _shankIds = value; }
    }

    /// <summary>
    /// Public constructor, defined as the default Json constructor.
    /// </summary>
    /// <param name="ndim"></param>
    /// <param name="si_units"></param>
    /// <param name="annotations"></param>
    /// <param name="contact_annotations"></param>
    /// <param name="contact_positions"></param>
    /// <param name="contact_plane_axes"></param>
    /// <param name="contact_shapes"></param>
    /// <param name="contact_shape_params"></param>
    /// <param name="probe_planar_contour"></param>
    /// <param name="device_channel_indices"></param>
    /// <param name="contact_ids"></param>
    /// <param name="shank_ids"></param>
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

    /// <summary>
    /// Copy constructor given an existing <see cref="Probe"/> object.
    /// </summary>
    /// <param name="probe"></param>
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
            contactShapeParams[i] = new ContactShapeParam(radius: radius);
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
            contactShapeParams[i] = new ContactShapeParam(width: width);
        }

        return contactShapeParams;
    }

    /// <summary>
    /// Generates an array of contact shape parameters for the rectangle, based on the given number of contacts and the given width / height
    /// </summary>
    /// <param name="numberOfContacts">Number of contacts in a single probe</param>
    /// <param name="width">Width of the contact, in the current probe units</param>
    /// <param name="height">Height of the contact, in the current probe units</param>
    /// <returns></returns>
    public static ContactShapeParam[] DefaultRectParams(int numberOfContacts, float width, float height)
    {
        ContactShapeParam[] contactShapeParams = new ContactShapeParam[numberOfContacts];

        for (int i = 0; i < numberOfContacts; i++)
        {
            contactShapeParams[i] = new ContactShapeParam(height: height);
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
    /// Returns a <see cref="Contact"/> object that contains the position, shape, shape params, and IDs (device / contact / shank)
    /// for a single contact at the given index
    /// </summary>
    /// <param name="index">Relative index of the contact in this Probe</param>
    /// <returns></returns>
    public Contact GetContact(int index)
    {
        return new Contact(ContactPositions[index][0], ContactPositions[index][1], ContactShapes[index], ContactShapeParams[index],
            DeviceChannelIndices[index], ContactIds[index], ShankIds[index], index);
    }

    /// <summary>
    /// Number of contacts within this <see cref="Probe"/>.
    /// </summary>
    public int NumberOfContacts => ContactPositions.Length;
}

/// <summary>
/// Number of dimensions to use while plotting a <see cref="Probe"/>.
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public enum ProbeNdim
{
    /// <summary>
    /// Two-dimensions
    /// </summary>
    [EnumMemberAttribute(Value = "2")]
    Two = 2,

    /// <summary>
    /// Three-dimensions
    /// </summary>
    [EnumMemberAttribute(Value = "3")]
    Three = 3,
}

/// <summary>
/// SI units for all values relating to location and position.
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ProbeSiUnits
{
    /// <summary>
    /// Millimeters [mm]
    /// </summary>
    [EnumMemberAttribute(Value = "mm")]
    mm = 0,

    /// <summary>
    /// Micrometers [um]
    /// </summary>
    [EnumMemberAttribute(Value = "um")]
    um = 1,
}

/// <summary>
/// Struct that extends the Probe Interface specification by encapsulating all values for a single contact within
/// a single struct.
/// </summary>
public readonly struct Contact
{
    /// <summary>
    /// X-position of the contact
    /// </summary>
    public float PosX { get; }
    /// <summary>
    /// Y-position of the contact
    /// </summary>
    public float PosY { get; }
    /// <summary>
    /// Shape of the contact
    /// </summary>
    public ContactShape Shape { get; }
    /// <summary>
    /// Parameters of the shape of the contact
    /// </summary>
    public ContactShapeParam ShapeParams { get; }
    /// <summary>
    /// Device channel index of the contact
    /// </summary>
    public int DeviceId { get; }
    /// <summary>
    /// Contact ID of the contact
    /// </summary>
    public string ContactId { get; }
    /// <summary>
    /// Shank ID of the contact
    /// </summary>
    public string ShankId { get; }
    /// <summary>
    /// Index of the contact within the <see cref="Probe"/> object variables.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// <see cref="Contact"/> constructor that initializes all data.
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

/// <summary>
/// Parameters used to draw the contact. Fields are nullable, since not all fields are required depending on the shape selected.
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class ContactShapeParam
{
    private float? _radius;
    private float? _width;
    private float? _height;

    /// <summary>
    /// Radius of the contact, only used to draw <see cref="ContactShape.Circle"/> contacts.
    /// </summary>
    public float? Radius
    {
        get { return _radius; }
        protected set { _radius = value; }
    }

    /// <summary>
    /// Width of the contact, used to draw <see cref="ContactShape.Square"/> or
    /// <see cref="ContactShape.Rect"/> contacts.
    /// </summary>
    public float? Width
    {
        get { return _width; }
        protected set { _width = value; }
    }

    /// <summary>
    /// Height of the contact, used to draw <see cref="ContactShape.Rect"/> contacts.
    /// </summary>
    public float? Height
    {
        get { return _height; }
        protected set { _height = value; }
    }

    /// <summary>
    /// Constructor, where all inputs are null by default. Inputs can be specified using <c>new ContactShapeParams(width: Width)</c>.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    [JsonConstructor]
    public ContactShapeParam(float? radius = null, float? width = null, float? height = null)
    {
        _radius = radius;
        _width = width;
        _height = height;
    }

    /// <summary>
    /// Copy constructor given an existing <see cref="ContactShapeParam"/> object.
    /// </summary>
    /// <param name="shape"></param>
    protected ContactShapeParam(ContactShapeParam shape)
    {
        _radius = shape._radius;
        _width = shape._width;
        _height = shape._height;
    }
}

/// <summary>
/// Shape of the contact
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
[JsonConverter(typeof(StringEnumConverter))]
public enum ContactShape
{
    /// <summary>
    /// Circle
    /// </summary>
    [EnumMemberAttribute(Value = "circle")]
    Circle = 0,

    /// <summary>
    /// Rectangle
    /// </summary>
    [EnumMemberAttribute(Value = "rect")]
    Rect = 1,

    /// <summary>
    /// Square
    /// </summary>
    [EnumMemberAttribute(Value = "square")]
    Square = 2,
}

/// <summary>
/// Class holding the Probe annotations. Name and manufacturer of the probe are defined here.
/// </summary>
[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class ProbeAnnotations
{
    private string _name;
    private string _manufacturer;

    /// <summary>
    /// Name of the probe as defined by the manufacturer, or a descriptive name such as the neurological target.
    /// </summary>
    [JsonProperty("name")]
    public string Name
    {
        get { return _name; }
        protected set { _name = value; }
    }

    /// <summary>
    /// Name of the manufacturer who created the probe.
    /// </summary>
    [JsonProperty("manufacturer")]
    public string Manufacturer
    {
        get { return _manufacturer; }
        protected set { _manufacturer = value; }
    }

    /// <summary>
    /// Constructor that instantiates with the name and manufacturer for the probe.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="manufacturer"></param>
    [JsonConstructor]
    public ProbeAnnotations(string name, string manufacturer)
    {
        _name = name;
        _manufacturer = manufacturer;
    }

    /// <summary>
    /// Copy constructor that copies data from an existing <see cref="ProbeAnnotations"/> object.
    /// </summary>
    /// <param name="probeAnnotations"></param>
    protected ProbeAnnotations(ProbeAnnotations probeAnnotations)
    {
        _name = probeAnnotations._name;
        _manufacturer = probeAnnotations._manufacturer;
    }
}

/// <summary>
/// Class holding all of the annotations for each contact.
/// </summary>
public class ContactAnnotations
{
    private string[] _contactAnnotations;

    /// <summary>
    /// Array of strings holding annotations for each contact. Not all indices must have annotations.
    /// </summary>
    public string[] Annotations
    {
        get { return _contactAnnotations; }
        protected set { _contactAnnotations = value; }
    }

    /// <summary>
    /// Constructor that initializes the contact annotations class.
    /// </summary>
    /// <param name="contactAnnotations"></param>
    [JsonConstructor]
    public ContactAnnotations(string[] contactAnnotations)
    {
        _contactAnnotations = contactAnnotations;
    }
}
