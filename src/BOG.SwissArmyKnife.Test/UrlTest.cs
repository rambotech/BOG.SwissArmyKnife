using BOG.SwissArmyKnife.Test.Support;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BOG.SwissArmyKnife.Test
{
    public class UrlTestData : IEnumerable
    {
        private Newtonsoft.Json.JsonSerializerSettings _JsonSetting =
            new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Include
            };

        public UrlTestData()
        {
        }

        public IEnumerator GetEnumerator()
        {
            List<UrlTestItem> urlTestItemList = null;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BOG.SwissArmyKnife.Test.BulkTestData.UrlTestItems.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    urlTestItemList = new List<UrlTestItem>(
                        JsonConvert.DeserializeObject<List<UrlTestItem>>(
                            reader.ReadToEnd(),
                            _JsonSetting));
                }
            }

            foreach (var testItem in urlTestItemList)
            {
                yield return testItem;
            }
        }
    }

    [TestFixture]
    public class UrlTest
    {
        [TestCaseSource(typeof(UrlTestData)), Description("Iterative: url parsing")]
        public void UrlTests_Iterative(UrlTestItem testItem)
        {
            BOG.SwissArmyKnife.Url testObj = null;

            // Set a breakpoint on Console.WriteLine to debug a particular test in the set.
            if (testItem.DataRow == "33")
            {
                Console.WriteLine("break point here");
            }

            if (!string.IsNullOrWhiteSpace(testItem.ThrowsException))
            {
                try
                {
                    testObj = new Url(testItem.OriginalUrl);
                }
                catch (Exception err1)
                {
                    string[] err = err1.GetType().ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    Assert.Multiple(() =>
                    {
                        Assert.That(string.Compare(testItem.ThrowsException, err[err.Length - 1], true) == 0, "Exception expected, but caught a different exception (Row {0}).", testItem.DataRow);
                        if (!string.IsNullOrWhiteSpace(testItem.ExceptionContains))
                        {
                            Assert.That(err1.Message.ToUpper().Contains(testItem.ExceptionContains.ToUpper()), "Exception message does not contain expected text \"" + testItem.ExceptionContains + "\"\r\nMessage: \"" + err1.Message + "\"  (Row {0}).", testItem.DataRow);
                        }
                    });
                    return;
                }
                Assert.That(false, "Expected Exception " + testItem.ThrowsException + ", but no exception was thrown. (Row {0}).", testItem.DataRow);
            }
            else
            {
                testObj = new Url(testItem.OriginalUrl);

                Assert.Multiple(() =>
                {
                    Assert.That(string.Compare(testItem.UrlDecodedScheme, testObj.Scheme, true) == 0, "UrlDecoded Scheme mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedUser, testObj.User, true) == 0, "UrlDecoded User mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedPassword, testObj.Password, true) == 0, "UrlDecoded Password mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedHost, testObj.Host, true) == 0, "UrlDecoded Host mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedPath, testObj.Path, true) == 0, "UrlDecoded Path mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedPort, testObj.PortExplicit, true) == 0, "UrlDecoded Port mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedQuery, testObj.Query, true) == 0, "UrlDecoded Query mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.UrlDecodedFragment, testObj.Fragment, true) == 0, "UrlDecoded Fragment mismatch (Row {0}).", testItem.DataRow);

                    Assert.That(string.Compare(testItem.User, testObj.GetRaw(Url.UrlPart.User), true) == 0, "non-UrlDecoded User mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Password, testObj.GetRaw(Url.UrlPart.Password), true) == 0, "non-UrlDecoded Password mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Host, testObj.GetRaw(Url.UrlPart.Host), true) == 0, "non-UrlDecoded Host mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Port, testObj.GetRaw(Url.UrlPart.Port), true) == 0, "non-UrlDecoded Port mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Path, testObj.GetRaw(Url.UrlPart.Path), true) == 0, "non-UrlDecoded Path mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Query, testObj.GetRaw(Url.UrlPart.Query), true) == 0, "non-UrlDecoded Query mismatch (Row {0}).", testItem.DataRow);
                    Assert.That(string.Compare(testItem.Fragment, testObj.GetRaw(Url.UrlPart.Fragment), true) == 0, "non-UrlDecoded Fragment mismatch (Row {0}).", testItem.DataRow);

                    Assert.That(string.Compare(testItem.AsString, testObj.ToString(), true) == 0, "original Url reconstruction (Row {0}).", testItem.DataRow);
                });
            }
        }
    }
}
