using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.CodeDom.Compiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    protected ProbeGroup() { }

    public ProbeGroup(string specification, string version, Probe[] probes)
    {
        _specification = specification;
        _version = version;
        _probes = probes.ToList();

        ValidateContactIds();
        ValidateShankIds();
        ValidateDeviceChannelIndices();
    }

    protected ProbeGroup(ProbeGroup probeGroup)
    {
        _specification = probeGroup._specification;
        _version = probeGroup._version;
        _probes = probeGroup._probes;
    }

    public int NumContacts
    {
        get
        {
            int numContacts = 0;

            foreach (var probe in _probes)
            {
                numContacts += probe.NumContacts;
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

    private void ValidateContactIds()
    {
        int contactNum = 0;

        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).ContactIds == null)
            {
                _probes.ElementAt(i).ContactIds = new string[_probes.ElementAt(i).ContactPositions.Length];

                for (int j = 0; j < _probes.ElementAt(i).ContactIds.Length; j++)
                {
                    _probes.ElementAt(i).ContactIds[j] = contactNum.ToString();
                    contactNum++;
                }
            }
            else
                contactNum += _probes.ElementAt(i).ContactIds.Length;
        }
    }

    private void ValidateShankIds()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).ShankIds == null)
            {
                _probes.ElementAt(i).ShankIds = new string[_probes.ElementAt(i).ContactPositions.Length];
            }
        }
    }

    private void ValidateDeviceChannelIndices()
    {
        for (int i = 0; i < _probes.Count(); i++)
        {
            if (_probes.ElementAt(i).DeviceChannelIndices == null)
            {
                _probes.ElementAt(i).DeviceChannelIndices = new int[_probes.ElementAt(i).ContactIds.Length];

                for (int j = 0; j < _probes.ElementAt(i).DeviceChannelIndices.Length; j++)
                {
                    if (int.TryParse(_probes.ElementAt(i).ContactIds[j], out int result))
                    {
                        _probes.ElementAt(i).DeviceChannelIndices[j] = result;
                    }
                }
            }
        }
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

    public Probe() { }

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
    /// Returns a Contact object that contains the position, shape, shape params, and IDs (device / contact / shank)
    /// for a single contact at the given index
    /// </summary>
    /// <param name="index">Relative index of the contact in this Probe</param>
    /// <returns></returns>
    public Contact GetContact(int index)
    {
        return new Contact(ContactPositions[index][0], ContactPositions[index][1], ContactShapes[index], ContactShapeParams[index],
            DeviceChannelIndices[index], ContactIds[index], ShankIds[index]);
    }

    public int NumContacts => ContactPositions.Length;
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

public struct Contact
{
    public float PosX { get; }
    public float PosY { get; }
    public ContactShape Shape { get; }
    public ContactShapeParam ShapeParams { get; }
    public int DeviceId { get; }
    public string ContactId { get; }
    public string ShankId { get; }

    public Contact(float posX, float posY, ContactShape shape, ContactShapeParam shapeParam,
        int device_id, string contact_id, string shank_id)
    {
        PosX = posX;
        PosY = posY;
        Shape = shape;
        ShapeParams = shapeParam;
        DeviceId = device_id;
        ContactId = contact_id;
        ShankId = shank_id;
    }
}

[GeneratedCodeAttribute("Bonsai.Sgen", "0.3.0.0 (Newtonsoft.Json v13.0.0.0)")]
public class ContactShapeParam
{
    private float _radius;

    public float Radius
    {
        get { return _radius; }
        protected set { _radius = value; }
    }

    public ContactShapeParam()
    {
    }

    [JsonConstructor]
    public ContactShapeParam(float radius)
    {
        _radius = radius;
    }

    protected ContactShapeParam(ContactShapeParam shape)
    {
        _radius = shape._radius;
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

    public ProbeAnnotations()
    {
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
    public string[] Contact_Annotations { get; protected set; }

    public ContactAnnotations()
    {
    }

    [JsonConstructor]
    public ContactAnnotations(string[] contact_annotations)
    {
        Contact_Annotations = contact_annotations;
    }
}
