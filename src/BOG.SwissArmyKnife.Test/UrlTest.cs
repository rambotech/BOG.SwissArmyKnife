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
                        Assert.AreEqual(testItem.ThrowsException, err[err.Length - 1], "Exception expected, but caught a different exception (Row {0}).", testItem.DataRow);
                        if (!string.IsNullOrWhiteSpace(testItem.ExceptionContains))
                        {
                            Assert.IsTrue(err1.Message.ToUpper().Contains(testItem.ExceptionContains.ToUpper()), "Exception message does not contain expected text \"" + testItem.ExceptionContains + "\"\r\nMessage: \"" + err1.Message + "\"  (Row {0}).", testItem.DataRow);
                        }
                    });
                    return;
                }
                Assert.IsTrue(false, "Expected Exception " + testItem.ThrowsException + ", but no exception was thrown. (Row {0}).", testItem.DataRow);
            }
            else
            {
                testObj = new Url(testItem.OriginalUrl);

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testItem.UrlDecodedScheme, testObj.Scheme, "UrlDecoded Scheme mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedUser, testObj.User, "UrlDecoded User mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedPassword, testObj.Password, "UrlDecoded Password mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedHost, testObj.Host, "UrlDecoded Host mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedPath, testObj.Path, "UrlDecoded Path mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedPort, testObj.PortExplicit, "UrlDecoded Port mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedQuery, testObj.Query, "UrlDecoded Query mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.UrlDecodedFragment, testObj.Fragment, "UrlDecoded Fragment mismatch (Row {0}).", testItem.DataRow);

                    Assert.AreEqual(testItem.User, testObj.GetRaw(Url.UrlPart.User), "non-UrlDecoded User mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Password, testObj.GetRaw(Url.UrlPart.Password), "non-UrlDecoded Password mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Host, testObj.GetRaw(Url.UrlPart.Host), "non-UrlDecoded Host mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Port, testObj.GetRaw(Url.UrlPart.Port), "non-UrlDecoded Port mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Path, testObj.GetRaw(Url.UrlPart.Path), "non-UrlDecoded Path mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Query, testObj.GetRaw(Url.UrlPart.Query), "non-UrlDecoded Query mismatch (Row {0}).", testItem.DataRow);
                    Assert.AreEqual(testItem.Fragment, testObj.GetRaw(Url.UrlPart.Fragment), "non-UrlDecoded Fragment mismatch (Row {0}).", testItem.DataRow);

                    Assert.AreEqual(testItem.AsString, testObj.ToString(), "original Url reconstruction (Row {0}).", testItem.DataRow);
                });
            }
        }
    }
}
