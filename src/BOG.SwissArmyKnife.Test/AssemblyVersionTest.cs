using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using NUnit.Framework.Constraints;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using static BOG.SwissArmyKnife.AssemblyVersion;
using System.Reflection;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class AssemblyVersionTest
    {
        /// <summary>
        /// Validate that fields are present.
        /// </summary>
        [Test]
        public void Assembly_ToJson()
        {
            var o = new AssemblyVersion().ToJson();

            Assert.That(o.Property("file").Any(), $"No value for file property");
            Assert.That(o.Property("name").Any(), $"No value for name property");
            Assert.That(o.Property("version").Any(), $"No value for version property");
            Assert.That(o.Property("built").Any(), $"No value for build property");
        }

        /// <summary>
        /// Validate that fields are present.
        /// </summary>
        [Test]
        public void Assembly_Entry_ToJson()
        {
            var o = new AssemblyVersion(AssemblySource.Entry).ToJson();

            Assert.That(o.Property("file").Any(), $"No value for file property");
            Assert.That(o.Property("name").Any(), $"No value for name property");
            Assert.That(o.Property("version").Any(), $"No value for version property");
            Assert.That(o.Property("built").Any(), $"No value for build property");
        }

        /// <summary>
        /// Validate that fields are present.
        /// </summary>
        [Test]
        public void Assembly_Calling_ToJson()
        {
            var o = new AssemblyVersion(AssemblySource.Calling).ToJson();

            Assert.That(o.Property("file").Any(), $"No value for file property");
            Assert.That(o.Property("name").Any(), $"No value for name property");
            Assert.That(o.Property("version").Any(), $"No value for version property");
            Assert.That(o.Property("built").Any(), $"No value for build property");
        }

        /// <summary>
        /// Validate that fields are present.
        /// </summary>
        [Test]
        public void Assembly_Executing_ToJson()
        {
            var o = new AssemblyVersion(AssemblySource.Executing).ToJson();

            Assert.That(o.Property("file").Any(), $"No value for file property");
            Assert.That(o.Property("name").Any(), $"No value for name property");
            Assert.That(o.Property("version").Any(), $"No value for version property");
            Assert.That(o.Property("built").Any(), $"No value for build property");
        }

        /// <summary>
        /// Validate that fields are present.
        /// </summary>
        [Test]
        public void Assembly_Type_ToJson()
        { 
            var t = Type.GetType("BOG.SwissArmyKnife.Test.AssemblyVersionTest");
            var o = new AssemblyVersion(t).ToJson();

            Assert.That(o.Property("file").Any(), $"No value for file property");
            Assert.That(o.Property("name").Any(), $"No value for name property");
            Assert.That(o.Property("version").Any(), $"No value for version property");
            Assert.That(o.Property("built").Any(), $"No value for build property");
        }
    }
}