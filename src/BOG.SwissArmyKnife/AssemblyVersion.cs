using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Text;

namespace BOG.SwissArmyKnife
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AssemblyVersion
    {
        public enum AssemblySource : int
        {
            Entry = 1,
            Calling = 2,
            Executing = 3
        }

        [JsonProperty(PropertyName = "Filename", Required = Required.Always)]
        public string Filename { get; private set; } = string.Empty;
        [JsonProperty(PropertyName = "Name", Required = Required.Always)]
        public string Name { get; private set; } = string.Empty;
        [JsonProperty(PropertyName = "Version", Required = Required.Always)]
        public string Version { get; private set; } = string.Empty;
        [JsonProperty(PropertyName = "BuildDate", Required = Required.Always)]
        public DateTime BuildDate { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// Get info for the entry assembly
        /// </summary>
        public AssemblyVersion()
        {
            Initialize(Assembly.GetEntryAssembly());
        }

        /// <summary>
        /// Get info for an assembly specified by type.
        /// </summary>
        public AssemblyVersion(Type typeObj)
        {
            Initialize(Assembly.GetAssembly(typeObj));
        }

        /// <summary>
        /// Get info for the specified assembly from the 3 common methods in System.Reflection.
        /// </summary>
        public AssemblyVersion(AssemblySource source)
        {
            switch (source)
            {
                case AssemblySource.Entry:
                    Initialize(Assembly.GetEntryAssembly());
                    break;
                case AssemblySource.Calling:
                    Initialize(Assembly.GetCallingAssembly());
                    break;
                case AssemblySource.Executing:
                    Initialize(Assembly.GetExecutingAssembly());
                    break;
                default:
                    throw new Exception($"Unknown AssemblySource: {source}");
            }
        }

        public void Initialize(Assembly assembly)
        {
            var av = assembly.GetName();
            Name = av.Name;
            Filename = assembly.Location;
            Version = av.Version.ToString();
            BuildDate = File.GetCreationTime(Filename);
        }

        /// <summary>
        /// Generate single line string summary.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Path.GetFileName(Filename)}, {Version}, {BuildDate:G)}";
        }

        /// <summary>
        /// Generate Json object for REST
        /// </summary>
        /// <returns></returns>
        public JObject ToJson()
        {
            return new JObject
            {
                { "file", Path.GetFileName(Filename) },
                { "name", Name },
                { "version", Version },
                { "built", BuildDate.ToString("G") }
            };
        }
    }
}
