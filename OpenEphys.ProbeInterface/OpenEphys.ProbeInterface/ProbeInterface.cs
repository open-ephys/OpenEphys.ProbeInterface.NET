using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenEphys.ProbeInterface;

public abstract class ProbeGroup
{
    private string _specification;
    private string _version;
    private IEnumerable<Probe> _probes;

    /// <summary>
    /// String defining the specification of the file. For Probe Interface files, this is expected to be "probeinterface"
    /// </summary>
    public string Specification
    {
        get { return _specification; }
        protected set { _specification = value; }
    }

    /// <summary>
    /// String defining which version of Probe Interface was used
    /// </summary>
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
    public IEnumerable<Probe> Probes
    {
        get { return _probes; }
        protected set { _probes = value; }
    }

    protected ProbeGroup() { }

    public ProbeGroup(string specification, string version, Probe[] probes)
    {
        Specification = specification;
        Version = version;
        Probes = probes.ToList();

        ValidateContactIds();
        ValidateShankIds();
        ValidateDeviceChannelIndices();
    }

    public ProbeGroup(ProbeGroup probeGroup)
    {
        Specification = probeGroup.Specification;
        Version = probeGroup.Version;
        Probes = probeGroup.Probes;
    }

    public int NumContacts
    {
        get
        {
            int numContacts = 0;

            foreach (var probe in Probes)
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

        foreach (var probe in Probes)
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

        foreach (var probe in Probes)
        {
            deviceChannelIndices.AddRange(probe.DeviceChannelIndices.ToList());
        }

        return deviceChannelIndices;
    }

    private void ValidateContactIds()
    {
        int contactNum = 0;

        for (int i = 0; i < Probes.Count(); i++)
        {
            if (Probes.ElementAt(i).ContactIds == null)
            {
                Probes.ElementAt(i).ContactIds = new string[Probes.ElementAt(i).ContactPositions.Length];

                for (int j = 0; j < Probes.ElementAt(i).ContactIds.Length; j++)
                {
                    Probes.ElementAt(i).ContactIds[j] = contactNum.ToString();
                    contactNum++;
                }
            }
            else
                contactNum += Probes.ElementAt(i).ContactIds.Length;
        }
    }

    private void ValidateShankIds()
    {
        for (int i = 0; i < Probes.Count(); i++)
        {
            if (Probes.ElementAt(i).ShankIds == null)
            {
                Probes.ElementAt(i).ShankIds = new string[Probes.ElementAt(i).ContactPositions.Length];
            }
        }
    }

    private void ValidateDeviceChannelIndices()
    {
        for (int i = 0; i < Probes.Count(); i++)
        {
            if (Probes.ElementAt(i).DeviceChannelIndices == null)
            {
                Probes.ElementAt(i).DeviceChannelIndices = new int[Probes.ElementAt(i).ContactIds.Length];

                for (int j = 0; j < Probes.ElementAt(i) .DeviceChannelIndices.Length; j++)
                {
                    if (int.TryParse(Probes.ElementAt(i).ContactIds[j], out int result))
                    {
                        Probes.ElementAt(i).DeviceChannelIndices[j] = result;
                    }
                }
            }
        }
    }
}

public class Probe
{
    private uint _numDimensions;
    private string _siUnits;
    private ProbeAnnotations _annotations;
    private ContactAnnotations _contactAnnotations;
    private float[][] _contactPositions;
    private float[][][] _contactPlaneAxes;
    private string[] _contactShapes;
    private ContactShapeParam[] _contactShapeParams;
    private float[][] _probePlanarContour;
    private int[] _deviceChannelIndices;
    private string[] _contactIds;
    private string[] _shankIds;

    public uint NumDimensions
    {
        get { return _numDimensions; }
        protected set { _numDimensions = value; }
    }
    public string SiUnits
    {
        get { return _siUnits; }
        protected set { _siUnits = value; }
    }
    public ProbeAnnotations Annotations
    {
        get { return _annotations; }
        protected set { _annotations = value; }
    }
    public ContactAnnotations ContactAnnotations
    {
        get { return _contactAnnotations; }
        protected set { _contactAnnotations = value; }
    }
    public float[][] ContactPositions
    {
        get { return _contactPositions; }
        protected set { _contactPositions = value; }
    }
    public float[][][] ContactPlaneAxes
    {
        get { return _contactPlaneAxes; }
        protected set { _contactPlaneAxes = value; }
    }
    public string[] ContactShapes
    {
        get { return _contactShapes; }
        protected set { _contactShapes = value; }
    }
    public ContactShapeParam[] ContactShapeParams
    {
        get { return _contactShapeParams; }
        protected set { _contactShapeParams = value; }
    }
    public float[][] ProbePlanarContour
    {
        get { return _probePlanarContour; }
        protected set { _probePlanarContour = value; }
    }
    public int[] DeviceChannelIndices
    {
        get { return _deviceChannelIndices; }
        internal set { _deviceChannelIndices = value; }
    }
    public string[] ContactIds
    {
        get { return _contactIds; }
        internal set { _contactIds = value; }
    }
    public string[] ShankIds
    {
        get { return _shankIds; }
        internal set { _shankIds = value; }
    }

    public Probe() { }

    [JsonConstructor]
    public Probe(uint ndim, string si_units, ProbeAnnotations annotations, ContactAnnotations contact_annotations,
        float[][] contact_positions, float[][][] contact_plane_axes, string[] contact_shapes,
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

public struct Contact
{
    public float PosX { get; }
    public float PosY { get; }
    public string Shape { get; }
    public ContactShapeParam ShapeParams { get; }
    public int DeviceId { get; }
    public string ContactId { get; }
    public string ShankId { get; }

    public Contact(float posX, float posY, string shape, ContactShapeParam shapeParam, int device_id, string contact_id, string shank_id)
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

public class ContactShapeParam
{
    public float Radius { get; protected set; }

    public ContactShapeParam()
    {
    }

    [JsonConstructor]
    public ContactShapeParam(float radius)
    {
        Radius = radius;
    }
}

public class ProbeAnnotations
{
    public string Name { get; protected set; }
    public string Manufacturer { get; protected set; }

    public ProbeAnnotations()
    {
    }

    [JsonConstructor]
    public ProbeAnnotations(string name, string manufacturer)
    {
        Name = name;
        Manufacturer = manufacturer;
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
