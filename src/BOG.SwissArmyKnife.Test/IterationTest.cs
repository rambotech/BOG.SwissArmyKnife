using BOG.SwissArmyKnife.Test.Support;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BOG.SwissArmyKnife.Test
{
    public class IterationTestData : IEnumerable
    {
        private readonly Newtonsoft.Json.JsonSerializerSettings _JsonSetting =
            new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Include
            };

        readonly List<IterationTestItem> testItemsStatic = new List<IterationTestItem>()
        {
            new IterationTestItem {
                DataRow= "0",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Count,
                StartValue= 0,
                StepValue= 1,
                CountValue= 15,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "1",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Count,
                StartValue= 0,
                StepValue= 1,
                CountValue= 15,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "2",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 0,
                StepValue= 1,
                CountValue= 0,
                EndValue=15,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "3",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 0,
                StepValue= 1,
                CountValue= 0,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 16
            },
            new IterationTestItem {
                DataRow= "4",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Count,
                StartValue= 15,
                StepValue= -1,
                CountValue= 15,
                EndValue= 0,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "5",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Count,
                StartValue= 15,
                StepValue= -1,
                CountValue= 15,
                EndValue= 0,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "6",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= -1,
                CountValue= 0,
                EndValue= 0,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "7",
                HandleAs= IterationItem.Handling.OrdinalNumber,

                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= -1,
                CountValue= 0,
                EndValue= 0,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 16
            },
            new IterationTestItem {
                DataRow= "8",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= -1.3M,
                CountValue= 0,
                EndValue= 12,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 2
            },
            new IterationTestItem {
                DataRow= "9",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= -1.3M,
                CountValue= 0,
                EndValue= 12,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= string.Empty,
                ExceptionContains= string.Empty,
                CountTestValue= 2
            },
            new IterationTestItem {
                DataRow= "10",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= 1,
                CountValue= 0,
                EndValue= 0,
                EndEval= Iteration.EndValueEval.Exclusive,
                ThrowsException= "ArgumentException",
                ExceptionContains= "is invalid: it should be negative, but is postive.",
                CountTestValue= 14
            },
            new IterationTestItem {
                DataRow= "11",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 0,
                StepValue= -1,
                CountValue= 0,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= "ArgumentException",
                ExceptionContains= " is invalid: it should be postive, but is negative.",
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "12",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 1,
                StepValue= 0,
                CountValue= 0,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= "ArgumentException",
                ExceptionContains= " can not be zero.",
                CountTestValue= 15
            },
            new IterationTestItem {
                DataRow= "12",
                HandleAs= IterationItem.Handling.OrdinalNumber,
                LiteralValues= null,
                Method= IterationTestItem.OrdinalNumberMethod.Range,
                StartValue= 15,
                StepValue= 57,
                CountValue= 0,
                EndValue= 15,
                EndEval= Iteration.EndValueEval.Inclusive,
                ThrowsException= "ArgumentException",
                ExceptionContains= " can not be zero.",
                CountTestValue= 1
            }
        };

        public IterationTestData()
        {
        }

        public IEnumerator GetEnumerator()
        {
#if FALSE
            List<IterationTestItem> iterationTestItemList = null;
            using (var sr = new StreamReader(@"IterationTestItems.json"))
            {
                iterationTestItemList = new List<IterationTestItem>(
                    JsonConvert.DeserializeObject<List<IterationTestItem>>(
                        sr.ReadToEnd(),
                        _JsonSetting));
            }

            foreach (var testItem in iterationTestItemList)
            {
                yield return testItem;
            }
#endif
            foreach (var testItem in testItemsStatic)
            {
                yield return testItem;
            }
        }
    }

    [TestFixture]
    public class IterationTest
    {
        [TestCaseSource(typeof(IterationTestData)), Description("Iterative: iteration class tests")]
        public void IterationTests_BulkTests(IterationTestItem testItem)
        {
            BOG.SwissArmyKnife.Iteration testObj = null;

            if (testItem.DataRow == "10")
            {
                // Set a breakpoint on Console.WriteLine to debug this particular test in the set.
                Console.WriteLine("break point here");
            }

            long actualCount = 0;
            try
            {
                testObj = new Iteration();
                switch (testItem.HandleAs)
                {
                    case IterationItem.Handling.Literal:
                        actualCount = testObj.AddListItems("iteration1", testItem.LiteralValues);
                        break;

                    case IterationItem.Handling.OrdinalNumber:
                        if (testItem.Method == IterationTestItem.OrdinalNumberMethod.Range)
                            actualCount = testObj.AddNumberRange("ITeration1", testItem.StartValue, testItem.StepValue, testItem.EndValue, testItem.EndEval);
                        if (testItem.Method == IterationTestItem.OrdinalNumberMethod.Count)
                            actualCount = testObj.AddNumberSequence("ITeration1", testItem.StartValue, testItem.StepValue, testItem.CountValue);
                        break;
                }
            }
            catch (Exception err1)
            {
                if (string.IsNullOrWhiteSpace(testItem.ThrowsException))
                {
                    throw;
                }
                string[] err = err1.GetType().ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(testItem.ThrowsException, err[err.Length - 1], "Exception expected, but caught a different exception (Row {0}).", testItem.DataRow);
                    if (!string.IsNullOrWhiteSpace(testItem.ExceptionContains))
                    {
                        Assert.IsTrue(err1.Message.ToUpper().Contains(testItem.ExceptionContains.ToUpper()), "Exception message does not contain expected text \"" + testItem.ExceptionContains + "\"\r\nMessage: \"" + err1.Message + "\"  (Row {0}).", testItem.DataRow);
                    }
                });
            }
            if (string.IsNullOrWhiteSpace(testItem.ThrowsException))
            {
                Assert.IsTrue(testItem.CountTestValue == actualCount, $"Invalid count, expected {testItem.CountTestValue} but got {actualCount}  (Row {testItem.DataRow}).");
            }
            else
            {
                Assert.IsTrue(false, "Expected Exception " + testItem.ThrowsException + ", but no exception was thrown. (Row {0}).", testItem.DataRow);
            }
        }
    }
}
