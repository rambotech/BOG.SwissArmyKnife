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

		public IterationTestData()
		{
		}

		public IEnumerator GetEnumerator()
		{
			List<IterationTestItem> iterationTestItemList = null;

			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "BOG.SwissArmyKnife.Test.BulkTestData.IterationTestItems.json";

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				using StreamReader reader = new StreamReader(stream);
					iterationTestItemList = new List<IterationTestItem>(
						JsonConvert.DeserializeObject<List<IterationTestItem>>(
							reader.ReadToEnd(),
							_JsonSetting));
			}

			foreach (var testItem in iterationTestItemList)
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

			if (testItem.DataRow == "13")
			{
				// Set a breakpoint on Console.WriteLine to debug the particular test from the set.
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
							actualCount = testObj.AddNumberRange("iteration1", testItem.StartValue, testItem.StepValue, testItem.EndValue, testItem.EndEval);
						if (testItem.Method == IterationTestItem.OrdinalNumberMethod.Count)
							actualCount = testObj.AddNumberSequence("iteration1", testItem.StartValue, testItem.StepValue, testItem.CountValue);
						break;
				}
			}
			catch (Exception err1)
			{
				if (string.IsNullOrWhiteSpace(testItem.ThrowsException))
				{
					Assert.Fail($"Unexpected Exception (Row {testItem.DataRow}): {DetailedException.WithUserContent(ref err1)}");
				}
				//string[] err = err1.GetType().ToString().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
				Assert.Multiple(() =>
				{
					Assert.AreEqual(testItem.ThrowsException, err1.GetType().ToString(), $"Exception expected, but caught a different exception (Row {testItem.DataRow}).");
					if (!string.IsNullOrWhiteSpace(testItem.ExceptionContains))
					{
						Assert.IsTrue(err1.Message.ToUpper().Contains(testItem.ExceptionContains.ToUpper()), "Exception thrown, but message does not contain expected text \"" + testItem.ExceptionContains + "\"\r\nMessage: \"" + err1.Message + "\"  (Row {0}).", testItem.DataRow);
					}
				});
				return;
			}
			if (!string.IsNullOrWhiteSpace(testItem.ThrowsException))
			{
				Assert.Fail("Expected Exception " + testItem.ThrowsException + ", but no exception was thrown. (Row {0}).", testItem.DataRow);
			}
			Assert.IsTrue(testItem.CountTestValue == actualCount, $"Invalid count, expected {testItem.CountTestValue} but got {actualCount}  (Row {testItem.DataRow}).");
		}

		[Test, Description("IterationTests_TotalItemCount1: Instantiate only")]
		public void IterationTests_TotalItemCount1()
		{
			Iteration i = new Iteration();
			Assert.AreEqual(i.TotalIterationCount, 0, "Count is not zero after instantiation");
		}

		[Test, Description("IterationTests_TotalItemCount2(): Single entry number set by count")]
		public void IterationTests_TotalItemCount2()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", -1, -1, 100);
			Assert.AreEqual(i.TotalIterationCount, 100);
		}

		[Test, Description("IterationTests_TotalItemCount3(): Single entry number set by range (exclusive)")]
		public void IterationTests_TotalItemCount3()
		{
			Iteration i = new Iteration();
			i.AddNumberRange("numbers1", -1, -1, -100, Iteration.EndValueEval.Exclusive);
			Assert.AreEqual(i.TotalIterationCount, 99);
		}

		[Test, Description("IterationTests_TotalItemCount4(): Single entry number set by range (inclusive)")]
		public void IterationTests_TotalItemCount4()
		{
			Iteration i = new Iteration();
			i.AddNumberRange("numbers1", -1, -1, -100, Iteration.EndValueEval.Inclusive);
			Assert.AreEqual(i.TotalIterationCount, 100);
		}

		[Test, Description("IterationTests_TotalItemCount5(): one number set and one list with one item")]
		public void IterationTests_TotalItemCount5()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 100);
			i.AddListItems("list1", new List<string>(new string[] { "item1" }));
			Assert.AreEqual(i.TotalIterationCount, 100);
		}

		[Test, Description("IterationTests_TotalItemCount6(): one number set and one list with two items")]
		public void IterationTests_TotalItemCount6()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 100);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			Assert.AreEqual(i.TotalIterationCount, 200);
		}

		[Test, Description("IterationTests_TotalItemAutoCount(): validate correct total after new item added")]
		public void IterationTests_TotalItemAutoCount()
		{
			Iteration i = new Iteration
			{
				IterationItems = new Dictionary<int, IterationItem>
				{
					{0, new IterationItem
						{
							Name = "Influence.1",
							HandleAs = IterationItem.Handling.OrdinalNumber,
							NumericStartValue=0.2m,
							NumericStepValue=0.01m,
							NumericValueCount=2400
						}
					},
					{1, new IterationItem
						{
							Name = "Influence.2",
							HandleAs = IterationItem.Handling.Literal,
							LiteralValues = new Dictionary<int, string>
							{
								{1, "big" },
								{2, "bad" },
								{3, "blue" }
							}
						}
					}
				}
			};
			Assert.AreEqual(i.TotalIterationCount, 7200L);  // 2400 * 3
			i.AddNumberSequence("numbers1", 1, 1, 100);
			Assert.AreEqual(i.TotalIterationCount, 720000L);  // 2400 * 3 * 100
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			Assert.AreEqual(i.TotalIterationCount, 1440000L);  // 2400 * 3 * 100 * 2
			i.AddNumberRange("numbers2", 1.0m, .1m, 2.0m, Iteration.EndValueEval.Exclusive);
			Assert.AreEqual(i.TotalIterationCount, 14400000L);  // 2400 * 3 * 100 * 2 * 10
		}

		[Test, Description("IterationTests_Serialization(): validate serialize/deserialize")]
		public void IterationTests_Serialization()
		{
			var settings = new JsonSerializerSettings()
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTime,
				Formatting = Newtonsoft.Json.Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore
			};

			Iteration o = new Iteration();
			o.AddNumberSequence("numbers1", 1, 1, 100);
			o.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			o.AddNumberRange("numbers2", 1.0m, .1m, 2.0m, Iteration.EndValueEval.Exclusive);

			var s =JsonConvert.SerializeObject(o, settings);

			var d = JsonConvert.DeserializeObject<Iteration>(s, settings);

			Assert.AreEqual(o.TotalIterationCount, 2000L, $"Serialization: Expected 2,000 iterations, but got {o.TotalIterationCount}");  // 100 * 2 * 10
			Assert.AreEqual(d.TotalIterationCount, 2000L, $"Desrialization: Expected 2,000 iterations, but got {d.TotalIterationCount}");  // 100 * 2 * 10
			Assert.AreEqual(o.IterationItems.Count, d.IterationItems.Count, $"Desrialization: Expected {o.IterationItems.Count} IterationItem, but has {d.IterationItems.Count}");
		}

		[Test, Description("IterationTests_GetIterationValueSet_NegativeIndex(): negative index throws exception")]
		public void IterationTests_GetIterationValueSet_NegativeIndex()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 2);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));

			object testDelegate() => i.GetIterationValueSet(-1);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
		}

		[Test, Description("IterationTests_GetIterationValueSet_NegativeCount(): negative count throws exception")]
		public void IterationTests_GetIterationValueSet_NegativeCount()
		{
			Iteration i = new Iteration();
			Assert.Throws<ArgumentException>(() =>  i.AddNumberSequence("numbers1", 1, 1, -1));
		}

		[Test, Description("IterationTests_GetIterationValueSet_IndexOverMax(): index over max range throws exception")]
		public void IterationTests_GetIterationValueSet_IndexOverMax()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 2);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));

			object testDelegate() => i.GetIterationValueSet(5);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
		}

		[Test, Description("IterationTests_ValueByIndex(): one number set with two items and one list with two items")]
		public void IterationTests_ValueByIndex()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 2);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			Assert.AreEqual(i.TotalIterationCount, 4, "Total is not 4");

			var valueSet = i.GetIterationValueSet(0);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 0): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 0): list1 is not item1");

			valueSet = i.GetIterationValueSet(1);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 1): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 1): list1 is not item2");

			valueSet = i.GetIterationValueSet(2);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 0): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 0): list1 is not item1");

			valueSet = i.GetIterationValueSet(3);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 1): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 1): list1 is not item2");
		}

		[Test, Description("IterationTests_ValueByIndex(): one number set with two items and one list with two items")]
		public void IterationTests_ComplexSets_ValidStartAndEndValuesPerValueSet()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 3);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			i.AddNumberSequence("numbers2", 1, 7, 7);
			Assert.AreEqual(i.TotalIterationCount, 42, "Total is not 42");

			var valueSet = i.GetIterationValueSet(0);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 0): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 0): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 0): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(6);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 6): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 6): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 6): numbers2 is not 43");

			valueSet = i.GetIterationValueSet(7);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 7): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 7): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 7): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(13);
			Assert.AreEqual(valueSet["numbers1"], "1", "(index 13): numbers1 is not 1");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 13): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 13): numbers2 is not 43");

			valueSet = i.GetIterationValueSet(14);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 14): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 14): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 14): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(20);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 20): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 20): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 20): numbers2 is not 43");

			valueSet = i.GetIterationValueSet(21);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 21): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 21): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 21): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(27);
			Assert.AreEqual(valueSet["numbers1"], "2", "(index 27): numbers1 is not 2");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 27): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 27): numbers2 is not 43");

			valueSet = i.GetIterationValueSet(28);
			Assert.AreEqual(valueSet["numbers1"], "3", "(index 33): numbers1 is not 3");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 33): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 33): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(34);
			Assert.AreEqual(valueSet["numbers1"], "3", "(index 34): numbers1 is not 3");
			Assert.AreEqual(valueSet["list1"], "item1", "(index 34): list1 is not item1");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 34): numbers2 is not 43");

			valueSet = i.GetIterationValueSet(35);
			Assert.AreEqual(valueSet["numbers1"], "3", "(index 35): numbers1 is not 3");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 35): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "1", "(index 35): numbers2 is not 1");

			valueSet = i.GetIterationValueSet(41);
			Assert.AreEqual(valueSet["numbers1"], "3", "(index 0): numbers1 is not 3");
			Assert.AreEqual(valueSet["list1"], "item2", "(index 13): list1 is not item2");
			Assert.AreEqual(valueSet["numbers2"], "43", "(index 13): numbers2 is not 43");

			object testDelegate() => i.GetIterationValueSet(42);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "i.GetIterationValueSet(42) did not throw expected ArgumentException");
		}

		[Test, Description("IterationTests_GetIterationItemsForNameByName()")]
		public void IterationTests_GetIterationItemsForNameByName()
		{
			Iteration i = new Iteration();
			i.AddNumberSequence("numbers1", 1, 1, 3);
			i.AddListItems("list1", new List<string>(new string[] { "item1", "item2" }));
			i.AddNumberSequence("numbers2", 1, 7, 7);

			object testDelegate() => i.GetIterationItemsForName("test1");
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "i.GetIterationItemsForName(\"test1\") did not throw expected ArgumentException");

			var x = i.GetIterationItemsForName("numbers1");
			Assert.AreEqual(x.Keys.Count, 3, $"numbers1 should contain 3 items, but has {x.Keys.Count}");
			x = i.GetIterationItemsForName("list1");
			Assert.AreEqual(x.Keys.Count, 2, $"list1 should contain 3 items, but has {x.Keys.Count}");
			x = i.GetIterationItemsForName("numbers2");
			Assert.AreEqual(x.Keys.Count, 7, $"numbers2 should contain 7 items, but has {x.Keys.Count}");
		}
	}
}
