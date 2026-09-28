using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class SettingsDictionaryTest
    {
        SettingsDictionary sd;
        string persist_file1 = Path.Combine(Path.GetTempPath(), $"TestSettings1-{Guid.NewGuid().ToString("N").ToLower()}.xml");
        string persist_file2 = Path.Combine(Path.GetTempPath(), $"TestSettings2-{Guid.NewGuid().ToString("N").ToLower()}.xml");
		string persist_file3 = Path.Combine(Path.GetTempPath(), $"TestSettings3-{Guid.NewGuid().ToString("N").ToLower()}.xml");
		string persist_file4 = Path.Combine(Path.GetTempPath(), $"TestSettings4-{Guid.NewGuid().ToString("N").ToLower()}.xml");

		[Test, Description("Create a new object, default instantiation method")]
        public void SettingsDictionary_Instantiate1()
        {
			sd = new SettingsDictionary();
            Assert.That(sd != null);
		}

		[Test, Description("Create Test Settings, save in #1")]
        public void SettingsDictionary_LoadTestData1()
        {
			sd = new SettingsDictionary(persist_file1);
            sd.SetSetting("Key1", "Things I don't like");
            sd.SetSetting("Key2", "another thing");
            sd.SaveSettings();
            Assert.That(sd.GetKeys().Length == 2);
		}

		[Test, Description("Create Test Settings, save in #2")]
        public void SettingsDictionary_LoadTestData2()
        {
			sd = new SettingsDictionary(persist_file2);
            sd.SetSetting("Key1", "Things I really don't like");
            sd.SetSetting("Key3", "yet another thing");
            sd.SaveSettings();
            Assert.That(sd.GetKeys().Length == 2);
		}

		[Test, Description("Merge Settings 1 (precedence) and 2, save to 3")]
        public void SettingsDictionary_MergeTestData3()
        {
            SettingsDictionary sd1 = new(persist_file2);
            sd1.LoadSettings();
            sd = new SettingsDictionary(persist_file1);
            sd.LoadSettings();
            sd.MergeSettings(sd1, false);
            sd.ConfigurationFile = persist_file3;
            sd.SaveSettings();
            Assert.That(sd.GetKeys().Length == 3);
		}

		[Test, Description("Merge Settings 1 and 2 (precedence), save to 4")]
        public void SettingsDictionary_MergeTestData4()
        {
			SettingsDictionary sd1 = new(persist_file2);
            sd1.LoadSettings();
            sd = new SettingsDictionary(persist_file1);
            sd.LoadSettings();
            sd.MergeSettings(sd1, true);
            sd.ConfigurationFile = persist_file4;
            sd.SaveSettings();
            Assert.That(sd.GetKeys().Length == 3);
		}
	}
}
