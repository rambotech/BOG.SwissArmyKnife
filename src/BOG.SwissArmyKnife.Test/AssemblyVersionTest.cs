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
using Newtonsoft.Json.Linq;

namespace BOG.SwissArmyKnife.Test
{
	[TestFixture]
	public class AssemblyVersionTest
	{
		#region DEBUG ONLY for dynamic value checking
		/// <summary>
		/// DEBUG ONLY: Validate date time format is properly formatted.
		/// </summary>
		/// <returns></returns>
		[Test]
		public void DO_Assembly_ToString()
		{
			var o = new AssemblyVersion(AssemblySource.Entry);
			var s = o.ToString();

			Assert.That(0 == 0);
		}

		/// <summary>
		/// DEBUG ONLY: Validate date time format is properly formatted.
		/// </summary>
		/// <returns></returns>
		[Test]
		public void DO_Assembly_ToString_UserFormatStr()
		{
			var o = new AssemblyVersion(AssemblySource.Entry);
			var s = o.ToString("{0:s}");

			Assert.That(0 == 0);  // break here for debug
		}

		/// <summary>
		/// DEBUG ONLY: Validate date time format is properly formatted.
		/// </summary>
		/// <returns></returns>
		[Test]
		public void DO_Assembly_ToJson_WithSource_FormatStr()
		{
			var o = new AssemblyVersion(AssemblySource.Entry);
			var s = o.ToJson(true, "{0:hh:mm tt}");

			Assert.That(0 == 0);  // break here for debug
		}
		#endregion

		/// <summary>
		/// Validate that fields are present.
		/// </summary>
		[Test]
		public void Assembly_ToJson_NoSource()
		{
			var o = new AssemblyVersion().ToJson();
			Assert.That(o.Property("file").Any(), $"No value for file property");
			Assert.That(o.Property("name").Any(), $"No value for name property");
			Assert.That(o.Property("version").Any(), $"No value for version property");
			Assert.That(o.Property("built").Any(), $"No value for build property");
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
		}

		/// <summary>
		/// Validate that fields are present.
		/// </summary>
		[Test]
		public void Assembly_ToJson_WithSource()
		{
			var o = new AssemblyVersion().ToJson(true);

			Assert.That(o.Property("file").Any(), $"No value for file property");
			Assert.That(o.Property("name").Any(), $"No value for name property");
			Assert.That(o.Property("version").Any(), $"No value for version property");
			Assert.That(o.Property("built").Any(), $"No value for build property");
			Assert.That(o.Property("source").Any(), $"Expected source property to be absent... but NO!");
		}

		/// <summary>
		/// Validate that fields are present.
		/// </summary>
		[Test]
		public void Assembly_ToJson_WithDateFormat()
		{
			var o = new AssemblyVersion().ToJson("{0:dddd MMMM dd, yyyy} at {0:hh:mm tt}");

			Assert.That(o.Property("file").Any(), $"No value for file property");
			Assert.That(o.Property("name").Any(), $"No value for name property");
			Assert.That(o.Property("version").Any(), $"No value for version property");
			Assert.That(o.Property("built").Any(), $"No value for build property");
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
		}

		/// <summary>
		/// Validate that fields are present.
		/// </summary>
		[Test]
		public void Assembly_Entry_ToJson()
		{
			var o = new AssemblyVersion(AssemblyVersion.AssemblySource.Entry).ToJson();

			Assert.That(o.Property("file").Any(), $"No value for file property");
			Assert.That(o.Property("name").Any(), $"No value for name property");
			Assert.That(o.Property("version").Any(), $"No value for version property");
			Assert.That(o.Property("built").Any(), $"No value for build property");
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
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
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
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
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
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
			Assert.That(o.Properties().ToList().Count == 4, $"Expected source property to be absent... but NO!");
		}
	}
}