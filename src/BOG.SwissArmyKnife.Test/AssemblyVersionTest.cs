using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using NUnit.Framework.Constraints;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;

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
    }
}