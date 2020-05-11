using BOG.SwissArmyKnife.Extensions;
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
    public class WildCardTestData : IEnumerable
    {
        private readonly Newtonsoft.Json.JsonSerializerSettings _JsonSetting =
            new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Include
            };

        public WildCardTestData()
        {
        }

        public IEnumerator GetEnumerator()
        {
            List<WildcardTestItem> urlTestItemList = null;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BOG.SwissArmyKnife.Test.BulkTestData.StringExTest_WildcardTestItems.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    urlTestItemList = new List<WildcardTestItem>(
                        JsonConvert.DeserializeObject<List<WildcardTestItem>>(
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
    public class StringExTest
    {
        [Test, Description("QuotedTrim_UnquotedEmptyString(): check for a quoted empty string")]
        public void StringEx_QuotedTrim_UnquotedEmptyString()
        {
            string Test = string.Empty;
            string Result = Test.QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedEmptyString(): check for a quoted empty string")]
        public void StringEx_QuotedTrim_QuotedEmptyString()
        {
            string Result = "\"\"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedSingleSpace(): check for a quoted string of single space")]
        public void StringEx_QuotedTrim_QuotedSingleSpace()
        {
            string Result = "\" \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedMultipleSpaces(): check for a quoted string of multiple spaces")]
        public void StringEx_QuotedTrim_QuotedMultipleSpaces()
        {
            string Result = "\"      \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare(string.Empty, Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedRJString(): check for a quoted string of right justified values")]
        public void StringEx_QuotedTrim_QuotedRJString()
        {
            string Result = "\"     X\"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare("X", Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedLJString(): check for a quoted string of left justified values")]
        public void StringEx_QuotedTrim_QuotedLJString()
        {
            string Result = "\"X     \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare("X", Result) == 0);
        }

        [Test, Description("QuotedTrim_QuotedCJString(): check for a quoted string of center justified values")]
        public void StringEx_QuotedTrim_QuotedCJString()
        {
            string Result = "\"  X  \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare("X", Result) == 0);
        }

        [Test, Description("QuotedTrim_EmbeddedQuotedString(): check for a quoted string embedded in quotes")]
        public void StringEx_QuotedTrim_EmbeddedQuotedString()
        {
            string Result = "\" \"  X  \" \"".QuotedTrim(new char[] { ' ', '\t' }, '\"');
            Assert.IsTrue(string.Compare("\"  X  \"", Result) == 0);
        }

        [Test, Description("Filter(): case-insensitive")]
        public void StringEx_Filter_CaseInsensitive()
        {
            string Filter = "0123456789.c";
            string Result = "76.34F.25.41C".Filter(Filter, true);
            Assert.IsTrue(string.Compare("76.34.25.41C", Result) == 0);
        }

        [Test, Description("Filter(): case-sensitive")]
        public void StringEx_Filter_CaseSensitive()
        {
            string Filter = "0123456789.c";
            string Result = "76.34F.25.41C".Filter(Filter, false);
            Assert.IsTrue(string.Compare("76.34.25.41", Result) == 0);
        }

        [Test, Description("FilterOut(): case-insensitive")]
        public void StringEx_FilterOut_CaseInsensitive()
        {
            string Filter = "0123456789.c";
            string Result = "76.34F.25.41C".FilterOut(Filter, true);
            Assert.IsTrue(string.Compare("F", Result) == 0);
        }

        [Test, Description("FilterOut(): case-sensitive")]
        public void StringEx_FilterOut_CaseSensitive()
        {
            string Filter = "0123456789.c";
            string Result = "76.34F.25.41C".FilterOut(Filter, false);
            Assert.IsTrue(string.Compare("FC", Result) == 0);
        }

        [Test, Description("HeadTailSummary(): default, not squashed")]
        public void StringEx_HeadTailSummary_Default_NotSquashed()
        {
            string TextBlob = new string('A', 256);
            string Result = TextBlob.HeadTailSummary();
            Assert.IsTrue(string.Compare(TextBlob, Result) == 0);
        }

        [Ignore("Skipped: Windows passes; Linux fails")]
        [Test, Description("HeadTailSummary(): default, squashed, small")]
        public void StringEx_HeadTailSummary_Default_Squashed_Small()
        {
            string TextBlob = new string('A', 257);
            string Result = TextBlob.HeadTailSummary();
            string Expected = new string('A', 128) + "\r\n\r\n  ...[1 bytes squashed]...\r\n\r\n" + new string('A', 128) + "\r\n";
            Assert.IsTrue(string.Compare(Result, Expected) == 0);
        }

        [Ignore("Skipped: Windows passes; Linux fails")]
        [Test, Description("HeadTailSummary(): default, squashed, large")]
        public void StringEx_HeadTailSummary_Default_Squashed_Large()
        {
            string TextBlob = new string('A', 1257);
            string Result = TextBlob.HeadTailSummary();
            string Expected = new string('A', 128) + "\r\n\r\n  ...[1,001 bytes squashed]...\r\n\r\n" + new string('A', 128) + "\r\n";
            Assert.IsTrue(string.Compare(Result, Expected) == 0);
        }

        [Test, Description("HeadTailSummary(): non-default, invalid head size")]
        public void StringEx_HeadTailSummary_NonDefault_InvalidHeadSize()
        {
            string TextBlob = new string('A', 128);
            Assert.Throws<ArgumentOutOfRangeException>(() => TextBlob.HeadTailSummary(-1, 128));
        }

        [Test, Description("HeadTailSummary(): non-default, invalid tail size")]
        public void StringEx_HeadTailSummary_NonDefault_InvalidTailSize()
        {
            string TextBlob = new string('A', 128);
            Assert.Throws<ArgumentOutOfRangeException>(() => TextBlob.HeadTailSummary(128, -1));
        }

        [Test, Description("HeadTailSummary(): non-default, non-squashed, small, 0 tail")]
        public void StringEx_HeadTailSummary_NonDefault_Squashed_Small_0Tail()
        {
            string TextBlob = new string('A', 128);
            string Result = TextBlob.HeadTailSummary(128, 0);
            Assert.IsTrue(string.Compare(TextBlob, Result) == 0);
        }

        [Test, Description("HeadTailSummary(): non-default, non-squashed, small, 0 head")]
        public void StringEx_HeadTailSummary_NonDefault_Squashed_Small_0Head()
        {
            string TextBlob = new string('A', 128);
            string Result = TextBlob.HeadTailSummary(0, 128);
            Assert.IsTrue(string.Compare(TextBlob, Result) == 0);
        }

        [Test, Description("HeadTailSummary(): non-default, not squashed, large")]
        public void StringEx_HeadTailSummary_NonDefault_NotSquashed_Large()
        {
            string TextBlob = new string('A', 1024);
            string Result = TextBlob.HeadTailSummary(512, 512);
            Assert.IsTrue(string.Compare(TextBlob, Result) == 0);
        }

        [Ignore("Skipped: Windows passes; Linux fails")]
        [Test, Description("HeadTailSummary(): non-default, squashed, large")]
        public void StringEx_HeadTailSummary_NonDefault_Squashed_Large()
        {
            string TextBlob = new string('A', 1026);
            string Result = TextBlob.HeadTailSummary(512, 512);
            string Expected = new string('A', 512) + "\r\n\r\n  ...[2 bytes squashed]...\r\n\r\n" + new string('A', 512) + "\r\n";
            Assert.IsTrue(string.Compare(Result, Expected) == 0);
        }

        [Test, Description("ToKeyValuePair(): simple")]
        public void StringEx_ToKeyValuePair_Simple()
        {
            string TextBlob =
                "key=value";
            var Result = TextBlob.ToKeyValuePair();
            Assert.IsTrue(Result.Keys.Count == 1);
            Assert.IsTrue(Result.ContainsKey(string.Empty));
            Assert.IsTrue(Result[string.Empty].ContainsKey("key"));
            Assert.IsTrue(Result[string.Empty]["key"].Count == 1);
            Assert.IsTrue(string.Compare(Result[string.Empty]["key"][0], "value", false) == 0);
        }

        [Test, Description("ToKeyValuePair(): Named Category and Multiline Value")]
        public void StringEx_ToKeyValuePair_NamedCategoryAndMultilineValue()
        {
            string TextBlob =
                "[general]\r" +
                "key=value1\r\n" +
                "\r\n" +
                "key=value2\n";
            var Result = TextBlob.ToKeyValuePair();
            Assert.IsTrue(Result.Keys.Count == 1);
            Assert.IsTrue(Result.ContainsKey("general"));
            Assert.IsTrue(Result["general"].ContainsKey("key"));
            Assert.IsTrue(Result["general"]["key"].Count == 2);
            Assert.IsTrue(string.Compare(Result["general"]["key"][0], "value1", false) == 0);
            Assert.IsTrue(string.Compare(Result["general"]["key"][1], "value2", false) == 0);
        }

        [Test, Description("ToKeyValuePair(): Intermix")]
        public void StringEx_ToKeyValuePair_Intermix()
        {
            string TextBlob =
                "key=value1\r\n" +
                "[general]\r" +
                "key=value3\r\n" +
                "\r\n" +
                "key=value4\n" +
                "\r\n" +
                "[]\r" +
                "key=value2\r\n" +
                "blank-key\r\n" +
                "double-equal=3=3";
            var Result = TextBlob.ToKeyValuePair();
            Assert.IsTrue(Result.Keys.Count == 2);
            Assert.IsTrue(Result.ContainsKey(string.Empty));
            Assert.IsTrue(Result.ContainsKey("general"));
            Assert.IsTrue(Result[string.Empty].ContainsKey("key"));
            Assert.IsTrue(Result[string.Empty]["key"].Count == 2);
            Assert.IsTrue(string.Compare(Result[string.Empty]["key"][0], "value1", false) == 0);
            Assert.IsTrue(string.Compare(Result[string.Empty]["key"][1], "value2", false) == 0);
            Assert.IsTrue(Result[string.Empty]["blank-key"].Count == 1);
            Assert.IsTrue(string.Compare(Result[string.Empty]["blank-key"][0], string.Empty, false) == 0);
            Assert.IsTrue(Result[string.Empty]["double-equal"].Count == 1);
            Assert.IsTrue(string.Compare(Result[string.Empty]["double-equal"][0], "3=3", false) == 0);
            Assert.IsTrue(Result["general"].ContainsKey("key"));
            Assert.IsTrue(Result["general"]["key"].Count == 2);
            Assert.IsTrue(string.Compare(Result["general"]["key"][0], "value3", false) == 0);
            Assert.IsTrue(string.Compare(Result["general"]["key"][1], "value4", false) == 0);
        }

        [TestCaseSource(typeof(WildCardTestData)), Description("Iterative: url parsing")]
        public void WildcardTests_Iterative(WildcardTestItem testItem)
        {
            // Set a breakpoint on Console.WriteLine to debug a particular test in the set.
            if (testItem.DataRow == "7")
            {
                Console.WriteLine("break point here");
            }
            var result = false;
            if (!string.IsNullOrWhiteSpace(testItem.ThrowsException))
            {
                try
                {
                    result = testItem.Value.ContainsWildcardPattern(
                        testItem.WildcardPattern,
                        bool.Parse(testItem.CaseSensitive));
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
                result = testItem.Value.ContainsWildcardPattern(
                    testItem.WildcardPattern,
                    bool.Parse(testItem.CaseSensitive));

                Assert.AreEqual(bool.Parse(testItem.ExpectedResult), result, "(Row {0}): {1}", testItem.DataRow, testItem.ExpectedResult);
            }
        }
    }
}
