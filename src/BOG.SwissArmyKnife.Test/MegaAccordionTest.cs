using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using BOG.SwissArmyKnife.Entity;
using BOG.SwissArmyKnife.Enums;
using NUnit.Framework.Constraints;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;

namespace BOG.SwissArmyKnife.Test
{
	[JsonObject]
	class MyMegaObject
	{
		[JsonProperty]
		public string Message { get; set; } = string.Empty;

		[JsonProperty]
		public bool Succeeded { get; set; } = false;
	}

	[TestFixture]
	public class MegaAccordionTest
	{
		#region Initialization
		/// <summary>
		/// Validate that no arguments are rejected.
		/// </summary>
		[Test]
		public void Accordion_Init_NoArguments()
		{
			try
			{
				var acc = MakeMegaAccordionTest_NoArguments();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "ArgumentItems must have at least one item in the list.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that all states other than Active and Sunsetting are invalid.
		/// </summary>
		[Test]
		public void Accordion_Init_InvalidState()
		{
			var invalidStates = new MegaAccordionState[] {
				MegaAccordionState.Completed
			 };
			var acc = MakeMegaAccordionTest_OneArgument_Valid();
			foreach (var state in invalidStates)
			{
				try
				{
					acc.State = state;
					acc.ResetMegaAccordion();
					Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
				}
				catch (ArgumentException ex)
				{
					Assert.That(ex.Message == $"The state must be set to Active or Sunsetting, but is set to {state}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
				}
				catch (Exception gex)
				{
					Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
				}
			}

			acc.State = MegaAccordionState.Active;
			acc.ResetMegaAccordion();
			acc.State = MegaAccordionState.Sunsetting;
			acc.ResetMegaAccordion();
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Level_Count()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevel();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "One or more values in Levels is less than the minumum of 1.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Levels_Count_External()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevels_External();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "There are 1 arguments, but there are 2 arguments indicated in the Levels.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Bad_Levels_Count_Internal()
		{
			try
			{
				var acc = MakeMegaAccordionTest_OneArgBadLevels_Internal();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == "Offset count of 1 does not equal the ArgumentItems count of 2.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default level values and counts are correct
		/// </summary>
		[Test]
		public void Accordion_Dupe_Arg_Names()
		{
			try
			{
				var acc = MakeMegaAccordionTest_DupeArgName();
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"ArgumentItems has one or more items with duplicated names: each item must have a unique name.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_TooManyInProgress()
		{
			var acc = MakeMegaAccordionTest_TooManyInProgress();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"ItemsInProgress count {acc.ItemsInProgress.Keys.Count} is greater than allowed MaxInProgress of {acc.MaxInProgress}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_LevelTooHigh()
		{
			var acc = MakeMegaAccordionTest_LevelTooHigh();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				Assert.That(ex.Message == $"Level value of {acc.Level} must be less than the number of levels: {acc.Levels.Length}.", $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_IndexesNegVal()
		{
			var acc = MakeMegaAccordionTest_IndexesNegVal();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				var r = new Regex($"The\\svalue\\s-\\d+\\sin\\sIndexes\\[\\d+\\]\\scan\\snot\\sbe\\snegative.");
				Assert.That(r.IsMatch(ex.Message), $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_IndexesOutOfBounds()
		{
			var acc = MakeMegaAccordionTest_IndexesOutOfBounds();
			try
			{
				acc.ResetMegaAccordion();
				Assert.IsTrue(false, "Expected ArgumentException to be thrown.");
			}
			catch (ArgumentException ex)
			{
				var r = new Regex($"The\\svalue\\s\\d+\\sin\\sIndexes\\[\\d+\\]\\sis\\sout\\sof\\sbounds\\sfor\\s.+\\sitems\\).");
				Assert.That(r.IsMatch(ex.Message), $"Argument exception has unexpected message content: \"{ex.Message}\"");
			}
			catch (Exception gex)
			{
				Assert.IsTrue(false, $"Expected ArgumentException but type was {gex.GetType()}: \"{gex.Message}\"");
			}
		}

		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_Init_Valid()
		{
			var acc = MakeMegaAccordionTest_OneArgument_Valid();

			acc.ResetMegaAccordion();
			Assert.That(acc.Level == 0, $"acc.Level: expected 0, but got {acc.Level}");
			Assert.That(acc.Levels.Length == 1, $"acc.IndexEnd: expected 0, but got {acc.Levels.Length}");
			Assert.That(acc.ArgumentItems.Count == 1, $"acc.IndexOffset: expected 0, but got {acc.ArgumentItems.Count}");
			Assert.That(acc.MaxInProgress == 100, $"acc.MaxInProgress: expected 0, but got {acc.MaxInProgress}");
			Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got {acc.ItemsInProgress.Count}");
		}

		/// <summary>
		/// Validate single-level, single index behavior.
		/// </summary>
		[Test]
		public void Accordion_Validate_Set1()
		{
			var acc = MakeMegaAccordionTest_Set1();
			Accordion_Validate_Set_Helper("Set1", acc);
		}

		/// <summary>
		/// Validate single argument, single-level.
		/// </summary>
		[Test]
		public void Accordion_Validate_Set2()
		{
			var acc = MakeMegaAccordionTest_Set2();
			acc.Level = 0;  // 3 : 0 : 0
			Accordion_Validate_Set_Helper("Set2", acc);
		}

		/// <summary>
		/// Validate multiple argument, multiple level, high level.
		/// </summary>
		[Test]
		public void Accordion_Validate_Set3()
		{
			var acc = MakeMegaAccordionTest_Set3();
			acc.Level = 0;  // 3 : 0 : 0
			Accordion_Validate_Set_Helper("Set3", acc);
		}

		#endregion

		#region Index behavior
		/// <summary>
		/// Validate Level 0 increment and key generation on a 3:2:6 set, with one level.  36 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_SL0()
		{
			var acc = MakeMegaAccordionTest_Set2();
			acc.Level = 0;  // 3 : 2 : 6  (36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 36; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 36)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 0 increment and key generation on a 3:2:6 set, with one level.  3 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_ML0()
		{
			var acc = MakeMegaAccordionTest_Set3();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 0;  // 3 : 2 : 6   (3 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 3; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 3)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 1 increment and key generation on a 3:2:6 set, with one level.  2 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_ML1()
		{
			var acc = MakeMegaAccordionTest_Set3();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 1;  // 3 : 2 : 6   (2 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 2; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 2)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 2 increment and key generation on a 3:2:6 set, with one level.  6 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_ML2()
		{
			var acc = MakeMegaAccordionTest_Set3();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 2;  // 3 : 2 : 6   (6 of 36)
			for (var index1 = 0; index1 <= 6; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 6)
				{
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 0 increment and key generation on a [3:2]:6 set, with one level.  6 iterations.
		/// </summary> 
		[Test]
		public void Accordion_Index_Level_Keys_4_ML0()
		{
			var acc = MakeMegaAccordionTest_Set4();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 0;  // [3 : 2] : 6   (6 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 6; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 6)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 1 increment and key generation on a [3:2]:6 set, with one level.  6 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_4_ML1()
		{
			var acc = MakeMegaAccordionTest_Set4();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 1;  // [3 : 2] : 6   (6 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 6; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 6)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 0 increment and key generation on a 3:[2:6] set, with one level.  3 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_5_ML0()
		{
			var acc = MakeMegaAccordionTest_Set5();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 0;  // [3 : 2] : 6   (6 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 3; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 3)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}

		/// <summary>
		/// Validate Level 1 increment and key generation on a 3:[2:6] set, with one level.  12 iterations.
		/// </summary>
		[Test]
		public void Accordion_Index_Level_Keys_5_ML1()
		{
			var acc = MakeMegaAccordionTest_Set5();
			acc.Indexes = new long[] { 2, 1, 0 };
			acc.Level = 0;  // [3 : 2] : 6   (6 of 36)
			acc.ResetMegaAccordion();
			for (var index1 = 0; index1 <= 3; index1++)
			{
				var key = acc.BuildKeyFromIndexes();
				var expectedKey = BuildKeyForTestingIndex(acc, index1);
				if (index1 == 3)
				{
					Assert.IsTrue(key == "0:0:0", $"Expected zero-based index for end but was \"{key}\".");
					Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"Expected ItemsInProgress with no items but has \"{acc.ItemsInProgress.Count}\".");
					Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"Expected State as CompletedSuccessfully but is \"{acc.State}\".");
					continue;
				}
				Assert.IsTrue(key == expectedKey, $"Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				if (acc.TryGetWorkItem(5, true, out var o))
				{
					acc.CompleteItem(o.Key);
				}
			}
		}
		#endregion

		#region Payload and State behavior
		/// <summary>
		/// Validate payload update and retrieval on a two-pass retrieval: pass 1 retry, pass 2 complete.
		/// </summary>
		[Test]
		public void Accordion_Payload1()
		{
			var acc = MakeMegaAccordionTest_Payload1();
			acc.Level = 0;
			acc.ResetMegaAccordion();
			int index = 0;
			Assert.IsTrue(acc.State == MegaAccordionState.Active, $"(Pre-loop) Expected State as Active but is \"{acc.State}\".");
			while (index < 200)
			{
				if (index == 100) Thread.Sleep(2500);
				MegaAccordionItem<MyMegaObject> myObj;
				Assert.IsTrue(acc.TryGetWorkItem(1, true, out myObj), $"({index}) failed to get item");
				if (index < 100)
				{
					Assert.IsTrue(myObj.Attempts.Count == 1, $"({index}) Expected 1 attempt entry, but have {myObj.Attempts.Count}.");
					Assert.IsNull(myObj.Value, $"({index}) Expected null value for value.");
					myObj.Value = new MyMegaObject
					{
						Succeeded = false
					};
					acc.UpdateItem(myObj); // updates the item in the ItemsInProgress queue.
					Assert.IsTrue(acc.State == MegaAccordionState.Sunsetting, $"({index}) Expected State as Sunsetting but is \"{acc.State}\".");
					acc.RetryItem(myObj.Key); // requeues the item to be processes again.
					Assert.IsTrue(acc.State == MegaAccordionState.Sunsetting, $"({index}) Expected State as Sunsetting but is \"{acc.State}\".");
				}
				else
				{
					Assert.IsNotNull(myObj.Value, $"({index}) Expected not null value for value.");
					Assert.IsTrue(acc.State == MegaAccordionState.Sunsetting, $"({index}) Expected State as Sunsetting but is \"{acc.State}\".");
					Assert.IsTrue(myObj.Attempts.Count == 2, $"({index}) Expected 2 attempt entries, but have {myObj.Attempts.Count}.");
					acc.CompleteItem(myObj.Key); // clears the item from the in-progress queue.
					if (index == 199)
					{
						Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"({index}) Expected State as Completed but is \"{acc.State}\".");
					}
					else
					{
						Assert.IsTrue(acc.State == MegaAccordionState.Sunsetting, $"({index}) Expected State as Sunsetting but is \"{acc.State}\".");
					}
				}
				index++;
			}
		}

		#endregion

		#region Shared (tests)

		/// <summary>
		/// Set evaluator
		/// </summary>
		private void Accordion_Validate_Set_Helper(string title, MegaAccordion<MyMegaObject> acc)
		{
			acc.ResetMegaAccordion();
			var key = acc.BuildKeyFromIndexes();
			var expectedKey = key;
			Assert.IsTrue(key == BuildKeyForTestingIndex(acc, 0), $"{title}: Expected BuildKeyFromIndexes to be \"0\", but was \"{key}\".");
			var allItemsCount = 1;
			var levelIndex = 0;
			for (var i = 0; i <= acc.Level; i++)
			{
				for (var j = 0; j < acc.Levels[i]; j++)
				{
					allItemsCount *= acc.ArgumentItems[levelIndex].Items.Length;
					levelIndex++;
				}
			}
			var index = 0;
			var nextIndex = 0;
			for (; index < allItemsCount; index++)
			{
				nextIndex = (index + 1) % allItemsCount;
				Assert.IsTrue(acc.State == MegaAccordionState.Active, $"{title}: For index {index}, expected state to be Active but was {acc.State}.");
				key = acc.BuildKeyFromIndexes();
				expectedKey = BuildKeyForTestingIndex(acc, index);
				Assert.IsTrue(key == expectedKey, $"{title}: Expected BuildKeyFromIndexes (1) to be \"{expectedKey}\", but was \"{key}\".");
				MegaAccordionItem<MyMegaObject> o;
				Assert.IsTrue(acc.TryGetWorkItem(600, true, out o), $"{title}: Expected TryGetWorkItem to be true.");
				key = acc.BuildKeyFromIndexes();
				expectedKey = BuildKeyForTestingIndex(acc, nextIndex);
				Assert.IsTrue(key == expectedKey, $"{title}: Expected BuildKeyFromIndexes (2) to be \"{expectedKey}\", but was \"{key}\".");
				Assert.IsTrue(acc.ItemsInProgress.Count == 1, $"{title}: Expected ItemsInProgress with one item but has \"{acc.ItemsInProgress.Count}\".");
				acc.CompleteItem(o.Key);
				Assert.IsTrue(acc.ItemsInProgress.Count == 0, $"{title}: Expected ItemsInProgress with not items but has \"{acc.ItemsInProgress.Count}\".");
			}
			Assert.IsTrue(acc.State == MegaAccordionState.Completed, $"{title}: Expected final state to be CompletedSuccessfully but was {acc.State}.");
		}

		#endregion

		#region Private Helpers

		private string BuildKeyForTestingIndex(MegaAccordion<MyMegaObject> o, int currentIndex)
		{
			string result = string.Empty;
			var count = currentIndex;

			// define the increment boundries
			var start = 0;
			for (var i = 0; i < o.Level; i++) start += o.Levels[i];
			var end = start + o.Levels[o.Level];

			for (var i = o.Indexes.Length - 1; i >= 0; i--)
			{
				var value = 0L;
				if (result.Length > 0) result = ":" + result;
				if (i < end)
				{
					if (i < start)
					{
						value = o.Indexes[i];
					}
					else
					{
						value = count % o.ArgumentItems[i].Items.Length;
						count /= o.ArgumentItems[i].Items.Length;
					}
				}
				result = $"{value}" + result;
			}
			return result;
		}

		#endregion

		#region Test Objects

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_NoArguments()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>()
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevel()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevels_External()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1, 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgBadLevels_Internal()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1, 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgument()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_OneArgument_Valid()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = Enums.MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_DupeArgName()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_TooManyInProgress()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 2,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				},
				ItemsInProgress = new Dictionary<string, MegaAccordionItem<MyMegaObject>>
				{
					{"0:0:0", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:0",
							Value=new MyMegaObject
							{
								Message = "Hello",
								Succeeded=false
							}
						}
					},
					{"0:0:1", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:1",
							Value=new MyMegaObject
							{
								Message = "World",
								Succeeded=false
							}
						}
					},
					{"0:0:2", new MegaAccordionItem<MyMegaObject>
						{
							DateAvailableTicks= DateTime.MaxValue.Ticks,
							Key="0:0:2",
							Value=new MyMegaObject
							{
								Message = "Piece",
								Succeeded=false
							}
						}
					}
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_LevelTooHigh()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 3,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_IndexesNegVal()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 2,
				Levels = new int[] { 1, 1, 1 },
				Indexes = new long[] { -1, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_IndexesOutOfBounds()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 2,
				Levels = new int[] { 1, 1, 1 },
				Indexes = new long[] { 0, 2, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_ArgsIndexes()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 0, 1, 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// Single Argument, single level
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set1()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 1 },
				Indexes = new long[] { 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2", "4", "5"}
						}
					}
				}
			};
		}

		/// <summary>
		/// MultiArgument, single level
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set2()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 3 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// MultiArgument, multiple level
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set3()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 1, 1, 1 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// MultiArgument, multiple level, level with more than one index
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set4()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 2, 1 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// MultiArgument, multiple level, level with more than one index
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Set5()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 1,
				Level = 0,
				Levels = new int[] { 1, 2 },
				Indexes = new long[] { 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"100", "101", "102", "103", "104", "105"}
						}
					},
				}
			};
		}

		/// <summary>
		/// MultiArgument, multiple level, 100 test objects, 100 in progress.
		/// </summary>
		/// <returns></returns>
		private MegaAccordion<MyMegaObject> MakeMegaAccordionTest_Payload1()
		{
			return new MegaAccordion<MyMegaObject>
			{
				State = MegaAccordionState.Active,
				MaxInProgress = 100,
				Level = 0,
				Levels = new int[] { 4 },
				Indexes = new long[] { 0, 0, 0, 0 },
				ArgumentItems = new Dictionary<int, ArgumentItem>
				{
					{0, new ArgumentItem
						{
							Name = "Arg1",
							Items = new string[] {"0", "1", "2", "3", "5"}
						}
					},
					{1, new ArgumentItem
						{
							Name = "Arg2",
							Items = new string[] {"50", "1"}
						}
					},
					{2, new ArgumentItem
						{
							Name = "Arg3",
							Items = new string[] {"101", "102", "103", "104", "105"}
						}
					},
					{3, new ArgumentItem
						{
							Name = "Arg4",
							Items = new string[] {"106", "107"}
						}
					}
				}
			};
		}
		#endregion
	}
}