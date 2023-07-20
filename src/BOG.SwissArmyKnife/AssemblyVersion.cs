using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BOG.SwissArmyKnife
{
    public class AssemblyVersion
    {
        public string Filename { get; private set; } = string.Empty;
        public string Version { get; private set; } = string.Empty;
        public DateTime BuildDate { get; private set; } = DateTime.MaxValue;

        public AssemblyVersion()
        {
            var av = Assembly.GetEntryAssembly().GetName();
            var FullName = av.CodeBase.Replace("file:///", string.Empty);
            Filename = Path.Combine(Path.GetDirectoryName(FullName), Path.GetFileName(FullName));
            Version = av.Version.ToString();
            BuildDate = File.GetCreationTime(Filename);
        }

        public override string ToString()
        {
            return $"{Path.GetFileName(Filename)}, {Version}, {BuildDate.ToString("G")}";
        }
    }
}
