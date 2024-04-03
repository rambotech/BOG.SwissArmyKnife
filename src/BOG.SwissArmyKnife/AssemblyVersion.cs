using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Text;

namespace BOG.SwissArmyKnife
{
    public class AssemblyVersion
    {
        public enum AssemblySource : int
        {
            Entry = 1,
            Calling = 2,
            Executing = 3
        }

        public string Filename { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string Version { get; private set; } = string.Empty;
        public DateTime BuildDate { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// Get info for the entry assembly
        /// </summary>
        public AssemblyVersion()
        {
            Initialize(AssemblySource.Entry);
        }

        /// <summary>
        /// Get info for the sepcified assembly
        /// </summary>
        public AssemblyVersion(AssemblySource source)
        {
            Initialize(source);
        }

        public void Initialize(AssemblySource source)
        {
            Assembly sourceAssembly;
            switch (source)
            {
                case AssemblySource.Entry:
                    sourceAssembly = Assembly.GetEntryAssembly();
                    break;
                case AssemblySource.Calling:
                    sourceAssembly = Assembly.GetCallingAssembly();
                    break;
                case AssemblySource.Executing:
                    sourceAssembly = Assembly.GetExecutingAssembly();
                    break;
                default:
                    throw new Exception($"Unknown AssemblySource: {source}");
            }
            var av = sourceAssembly.GetName();
            Name = av.Name;
            Filename = sourceAssembly.Location;
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
