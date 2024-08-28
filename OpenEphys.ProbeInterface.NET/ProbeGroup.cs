using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;

namespace OpenEphys.ProbeInterface.NET
{
    /// <summary>
    /// Abstract class that implements the Probeinterface specification in C# for .NET.
    /// </summary>
    public abstract class ProbeGroup
    {
        /// <summary>
        /// Gets the string defining the specification of the file.
        /// </summary>
        /// <remarks>
        /// For Probeinterface files, this value is expected to be "probeinterface".
        /// </remarks>
        [JsonProperty("specification", Required = Required.Always)]
        public string Specification { get; protected set; }

        /// <summary>
        /// Gets the string defining which version of Probeinterface was used.
        /// </summary>
        [JsonProperty("version", Required = Required.Always)]
        public string Version { get; protected set; }

        /// <summary>
        /// Gets an IEnumerable of probes that are present.
        /// </summary>
        /// <remarks>
        /// Each probe can contain multiple shanks, and each probe has a unique
        /// contour that defines the physical representation of the probe. Contacts have several representations
        /// for their channel number, specifically <see cref="Probe.ContactIds"/> (a string that is not guaranteed to be unique) and
        /// <see cref="Probe.DeviceChannelIndices"/> (guaranteed to be unique across all probes). <see cref="Probe.DeviceChannelIndices"/>'s can also be set to -1
        /// to indicate that the channel was not connected or recorded from.
        /// </remarks>
        [XmlIgnore]
        [JsonProperty("probes", Required = Required.Always)]
        public IEnumerable<Probe> Probes { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeGroup"/> class.
        /// </summary>
        /// <param name="specification">String defining the <see cref="Specification"/> parameter.</param>
        /// <param name="version">String defining the <see cref="Version"/> parameter.</param>
        /// <param name="probes">IEnumerable of <see cref="Probe"/> objects.</param>
        public ProbeGroup(string specification, string version, IEnumerable<Probe> probes)
        {
            Specification = specification;
            Version = version;
            Probes = probes;

            Validate();
        }

        /// <summary>
        /// Copy constructor that takes in an existing <see cref="ProbeGroup"/> object and copies the individual fields.
        /// </summary>
        /// <remarks>
        /// After copying the relevant fields, the <see cref="ProbeGroup"/> is validated to ensure that it is compliant
        /// with the Probeinterface specification. See <see cref="Validate"/> for more details on what is checked.
        /// </remarks>
        /// <param name="probeGroup">Existing <see cref="ProbeGroup"/> object.</param>
        protected ProbeGroup(ProbeGroup probeGroup)
        {
            Specification = probeGroup.Specification;
            Version = probeGroup.Version;
            Probes = probeGroup.Probes;

            Validate();
        }

        /// <summary>
        /// Gets the number of contacts across all <see cref="Probe"/> objects.
        /// </summary>
        [JsonIgnore]
        public int NumberOfContacts => Probes.Aggregate(0, (total, next) => total + next.NumberOfContacts);

        /// <summary>
        /// Returns the <see cref="Probe.ContactIds"/>'s of all contacts in all probes.
        /// </summary>
        /// <remarks>
        /// Note that these are not guaranteed to be unique values across probes.
        /// </remarks>
        /// <returns>List of strings containing all contact IDs.</returns>
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
        /// Returns all <see cref="Contact"/> objects in the <see cref="ProbeGroup"/>.
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
        /// Returns all <see cref="Probe.DeviceChannelIndices"/>'s in the <see cref="ProbeGroup"/>.
        /// </summary>
        /// <remarks>
        /// Device channel indices are guaranteed to be unique, unless they are -1. Multiple contacts can be
        /// set to -1 to indicate they are not recorded from.
        /// </remarks>
        /// <returns><see cref="IEnumerable{Int32}"/></returns>
        public IEnumerable<int> GetDeviceChannelIndices()
        {
            List<int> deviceChannelIndices = new();

            foreach (var probe in Probes)
            {
                deviceChannelIndices.AddRange(probe.DeviceChannelIndices.ToList());
            }

            return deviceChannelIndices;
        }

        /// <summary>
        /// Validate that the <see cref="ProbeGroup"/> correctly implements the Probeinterface specification.
        /// </summary>
        /// <remarks>
        /// <para>Check that all necessary fields are populated (<see cref="Specification"/>,
        /// <see cref="Version"/>, <see cref="Probes"/>).</para>
        /// <para>Check that there is at least one <see cref="Probe"/> defined.</para>
        /// <para>Check that all variables in each <see cref="Probe"/> have the same length.</para>
        /// <para>Check if <see cref="Probe.ContactIds"/> are present, and generate default values
        /// based on the index if there are no values defined.</para>
        /// <para>Check if <see cref="Probe.ContactIds"/> are zero-indexed, and convert to
        /// zero-indexed if possible.</para>
        /// <para>Check if <see cref="Probe.ShankIds"/> are defined, and initialize empty strings 
        /// if they are not defined.</para>
        /// <para>Check if <see cref="Probe.DeviceChannelIndices"/> are defined, and initialize default
        /// values (using the <see cref="Probe.ContactIds"/> value as the new <see cref="Probe.DeviceChannelIndices"/>).</para>
        /// <para>Check that all <see cref="Probe.DeviceChannelIndices"/> are unique across all <see cref="Probe"/>'s,
        /// unless the value is -1; multiple contacts can be set to -1.</para>
        /// </remarks>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Specification))
            {
                throw new InvalidOperationException("Specification string must be defined.");
            }

            if (string.IsNullOrEmpty(Version))
            {
                throw new InvalidOperationException("Version string must be defined.");
            }

            if (Probes == null || Probes.Count() == 0)
            {
                throw new InvalidOperationException("No probes are listed, probes must be added during construction");
            }

            ValidateVariableLength();

            SetDefaultContactIdsIfMissing();
            ForceContactIdsToZeroIndexed();
            SetEmptyShankIdsIfMissing();
            SetDefaultDeviceChannelIndicesIfMissing();

            if (!ValidateDeviceChannelIndices())
            {
                throw new Exception("Device channel indices are not unique across all probes.");
            }
        }

        private void ValidateVariableLength()
        {
            for (int i = 0; i < Probes.Count(); i++)
            {
                if (Probes.ElementAt(i).NumberOfContacts != Probes.ElementAt(i).ContactPositions.Count() ||
                    Probes.ElementAt(i).NumberOfContacts != Probes.ElementAt(i).ContactPlaneAxes.Count() ||
                    Probes.ElementAt(i).NumberOfContacts != Probes.ElementAt(i).ContactShapeParams.Count() ||
                    Probes.ElementAt(i).NumberOfContacts != Probes.ElementAt(i).ContactShapes.Count())
                {
                    throw new InvalidOperationException($"Required contact parameters are not the same length in probe {i}. " +
                             "Check positions / plane axes / shapes / shape parameters for lengths.");
                }

                if (Probes.ElementAt(i).ContactIds != null &&
                    Probes.ElementAt(i).ContactIds.Count() != Probes.ElementAt(i).NumberOfContacts)
                {
                    throw new InvalidOperationException($"Contact IDs does not have the correct number of channels for probe {i}");
                }

                if (Probes.ElementAt(i).ShankIds != null &&
                    Probes.ElementAt(i).ShankIds.Count() != Probes.ElementAt(i).NumberOfContacts)
                {
                    throw new InvalidOperationException($"Shank IDs does not have the correct number of channels for probe {i}");
                }

                if (Probes.ElementAt(i).DeviceChannelIndices != null &&
                    Probes.ElementAt(i).DeviceChannelIndices.Count() != Probes.ElementAt(i).NumberOfContacts)
                {
                    throw new InvalidOperationException($"Device Channel Indices does not have the correct number of channels for probe {i}");
                }
            }
        }

        private void SetDefaultContactIdsIfMissing()
        {
            for (int i = 0; i < Probes.Count(); i++)
            {
                if (Probes.ElementAt(i).ContactIds == null)
                {
                    Probes.ElementAt(i).ContactIds = Probe.DefaultContactIds(Probes.ElementAt(i).NumberOfContacts);
                }
            }
        }

        private void ForceContactIdsToZeroIndexed()
        {
            var contactIds = GetContactIds();
            var numericIds = contactIds.Select(c => { return int.Parse(c); })
                                       .ToList();

            var min = numericIds.Min();
            var max = numericIds.Max();

            if (min == 1 && max == NumberOfContacts && numericIds.Count == numericIds.Distinct().Count())
            {
                for (int i = 0; i < Probes.Count(); i++)
                {
                    var probe = Probes.ElementAt(i);
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
            for (int i = 0; i < Probes.Count(); i++)
            {
                if (Probes.ElementAt(i).ShankIds == null)
                {
                    Probes.ElementAt(i).ShankIds = Probe.DefaultShankIds(Probes.ElementAt(i).NumberOfContacts);
                }
            }
        }

        private void SetDefaultDeviceChannelIndicesIfMissing()
        {
            for (int i = 0; i < Probes.Count(); i++)
            {
                if (Probes.ElementAt(i).DeviceChannelIndices == null)
                {
                    Probes.ElementAt(i).DeviceChannelIndices = new int[Probes.ElementAt(i).NumberOfContacts];

                    for (int j = 0; j < Probes.ElementAt(i).NumberOfContacts; j++)
                    {
                        if (int.TryParse(Probes.ElementAt(i).ContactIds[j], out int result))
                        {
                            Probes.ElementAt(i).DeviceChannelIndices[j] = result;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate the uniqueness of all <see cref="Probe.DeviceChannelIndices"/>'s across all <see cref="Probe"/>'s.
        /// </summary>
        /// <remarks>
        /// All indices that are greater than or equal to 0 must be unique,
        /// but there can be as many values equal to -1 as there are contacts. A value of -1 indicates that this contact is 
        /// not being recorded.
        /// </remarks>
        /// <returns>True if all values not equal to -1 are unique, False if there are duplicates.</returns>
        public bool ValidateDeviceChannelIndices()
        {
            var activeChannels = GetDeviceChannelIndices().Where(index => index != -1);
            return activeChannels.Count() == activeChannels.Distinct().Count();
        }

        /// <summary>
        /// Update the <see cref="Probe.DeviceChannelIndices"/> at the given probe index.
        /// </summary>
        /// <remarks>
        /// Device channel indices can be updated as contacts are being enabled or disabled. This is done on a 
        /// per-probe basis, where the incoming array of indices must be the same size as the original probe, 
        /// and must follow the standard for uniqueness found in <see cref="Probe.DeviceChannelIndices"/>.
        /// </remarks>
        /// <param name="probeIndex">Zero-based index of the probe to update.</param>
        /// <param name="deviceChannelIndices">Array of <see cref="Probe.DeviceChannelIndices"/>.</param>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateDeviceChannelIndices(int probeIndex, int[] deviceChannelIndices)
        {
            if (Probes.ElementAt(probeIndex).DeviceChannelIndices.Length != deviceChannelIndices.Length)
            {
                throw new ArgumentException($"Incoming device channel indices have {deviceChannelIndices.Length} contacts, " +
                    $"but the existing probe {probeIndex} has {Probes.ElementAt(probeIndex).DeviceChannelIndices.Length} contacts");
            }    

            Probes.ElementAt(probeIndex).DeviceChannelIndices = deviceChannelIndices;

            if (!ValidateDeviceChannelIndices())
            {
                throw new ArgumentException("Device channel indices are not valid. Ensure that all values are either -1 or are unique.");
            }
        }
    }
}
