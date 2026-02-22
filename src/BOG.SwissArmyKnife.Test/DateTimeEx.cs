using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BOG.SwissArmyKnife.Extensions;
using BOG.SwissArmyKnife.Test.Support;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BOG.SwissArmyKnife.Test
{
    public class DateTimeExTestData : IEnumerable
    {
        private readonly Newtonsoft.Json.JsonSerializerSettings _JsonSetting =
            new()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Include
            };

        public DateTimeExTestData()
        {
        }

        public IEnumerator GetEnumerator()
        {
            List<DateTimeExTestItem> dateTimeTestItemList = null;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BOG.SwissArmyKnife.Test.BulkTestData.DateTimeExTestItems.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new(stream))
                {
                    dateTimeTestItemList = new List<DateTimeExTestItem>(
                        JsonConvert.DeserializeObject<List<DateTimeExTestItem>>(
                            reader.ReadToEnd(),
                            _JsonSetting));
                }
            }

            foreach (var testItem in dateTimeTestItemList)
            {
                yield return testItem;
            }
        }
    }

    [TestFixture]
    public class DateTimeEx
    {
        [TestCaseSource(typeof(DateTimeExTestData)), Description("Iterative")]
        public void DateTimeExTests_Iterative(DateTimeExTestItem testItem)
        {
            if (testItem.DataRow == "1")
            {
                Console.WriteLine("break point here for debug only");
            }
            DateTime result;
            if (!string.IsNullOrWhiteSpace(testItem.ThrowsException))
            {
                try
                {
                    switch (testItem.TestAs)
                    {
                        case DateTimeExTestItem.DateTimeExTestType.Earliest:
                            result = testItem.OriginalDateTime.Earliest(testItem.DateTimeValuesToCompare);
                            break;
                        case DateTimeExTestItem.DateTimeExTestType.Latest:
                            result = testItem.OriginalDateTime.Latest(testItem.DateTimeValuesToCompare);
                            break;
                        default:
                            throw new Exception($"({testItem.DataRow}): Undefined/Unhandled method for TestAs: {testItem.TestAs}");
                    }
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
                switch (testItem.TestAs)
                {
                    case DateTimeExTestItem.DateTimeExTestType.Earliest:
                        result = testItem.OriginalDateTime.Earliest(testItem.DateTimeValuesToCompare);
                        break;
                    case DateTimeExTestItem.DateTimeExTestType.Latest:
                        result = testItem.OriginalDateTime.Latest(testItem.DateTimeValuesToCompare);
                        break;
                    default:
                        throw new Exception($"({testItem.DataRow}): Undefined/Unhandled method for TestAs: {testItem.TestAs}");
                }
                Assert.That(testItem.ExpectedResultValue == result,
                    $"(Data Row: \"{testItem.DataRow}\"): Expected: {testItem.ExpectedResultValue}, returned {result}");
            }
        }
    }
}

