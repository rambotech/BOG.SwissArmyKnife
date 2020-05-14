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
	class MyObject
	{
		public long Index { get; set; }
		public string Message { get; set; }
		public bool Succeeded { get; set; } = false;
	}

	[TestFixture]
	public class AccordionTest
	{
		/// <summary>
		/// Validate that default values are correct
		/// </summary>
		[Test]
		public void Accordion_Init_Default()
		{
			var acc = new Accordion<MyObject>();

			Assert.That(acc.IndexStart == 0, $"acc.IndexStart: expected 0, but got {acc.IndexStart}");
			Assert.That(acc.IndexEnd == 0, $"acc.IndexEnd: expected 0, but got {acc.IndexEnd}");
			Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got {acc.IndexOffset}");
			Assert.That(acc.MaxInProgress == 0, $"acc.MaxInProgress: expected 0, but got {acc.MaxInProgress}");
			Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got {acc.ItemsInProgress.Count}");
		}

		/// <summary>
		/// Validate that specific init values are correct
		/// </summary>
		[Test]
		public void Accordion_Init_Specific()
		{
			var acc = new Accordion<MyObject>(55, 300, 200);

			Assert.That(acc.IndexStart == 55, $"acc.IndexStart: expected 55, but got {acc.IndexStart}");
			Assert.That(acc.IndexEnd == 355, $"acc.IndexEnd: expected 354, but got {acc.IndexEnd}");
			Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got {acc.IndexOffset}");
			Assert.That(acc.MaxInProgress == 200, $"acc.MaxInProgress: expected 200, but got {acc.MaxInProgress}");
			Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got {acc.ItemsInProgress.Count}");
		}

		/// <summary>
		/// Check for a starting value under 0
		/// </summary>
		[Test]
		public void Accordion_Init_BadIndexStart()
		{
			static object testDelegate() => new Accordion<MyObject>(-1, 1, 1);
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "indexStart must be >= 0");
		}

		/// <summary>
		/// Check for a count value over 0
		/// </summary>
		[Test]
		public void Accordion_Init_Bad_CountTooLow()
		{
#pragma warning disable IDE0039 // Use local function
			ActualValueDelegate<object> testDelegate = () => new Accordion<MyObject>(1, 0, 1);
#pragma warning restore IDE0039 // Use local function
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "count must be > 0");
		}

		/// <summary>
		/// Check for a max value over 0
		/// </summary>
		[Test]
		public void Accordion_Init_MaxBad()
		{
#pragma warning disable IDE0039 // Use local function
			ActualValueDelegate<object> testDelegate = () => new Accordion<MyObject>(1, 1, 9);
#pragma warning restore IDE0039 // Use local function
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be >= 10");
		}

		/// <summary>
		/// Check that a max value under 10 is invalid
		/// </summary>
		[Test]
		public void Accordion_Init_Max_Bad_Under_MinValue()
		{
#pragma warning disable IDE0039 // Use local function
			TestDelegate testDelegate = () => new Accordion<MyObject>(2, 2, 9);
#pragma warning restore IDE0039 // Use local function
			Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be >= 10");
		}

		/// <summary>
		/// Check that a max value of 10 (lowest) is OK
		/// </summary>
		[Test]
		public void Accordion_Init_Max_Good_MinValue()
		{
#pragma warning disable IDE0039 // Use local function
			TestDelegate testDelegate = () => new Accordion<MyObject>(2, 2, 10);
#pragma warning restore IDE0039 // Use local function
			Assert.DoesNotThrow(testDelegate, "max should allow min value of 10, but does not.");
		}

		[Test]
		public void Accordion_Basics_A()
		{
			int itemsRemaining;
			var acc = new Accordion<MyObject>(0, 200, 100);
			{
				// Gets 1 of the 200.            
				var itemsA = acc.GetItems(60, 0, true); // always returns a minimun of one item when available.
				Assert.That(itemsA.Count == 1, $"(A) GetItems() expected to return 1 item, but returned {itemsA.Count} item(s).");
				Assert.That(acc.IndexOffset == 100, $"(A) acc.IndexOffset: expected 100, but got {acc.IndexOffset}");
				itemsRemaining = acc.GetInProgressCount();
				Assert.That(itemsRemaining == 100, $"(A) GetInProgressCount(): expected 100, but got {itemsRemaining}");

				// up to 101, but only 99 are retrieved.
				var itemsB = acc.GetItems(60, 101, true);
				Assert.That(itemsB.Count == 99, $"(B) GetItems() expected to return 99 items, but returned {itemsB.Count} item(s).");
				Assert.That(acc.IndexOffset == 100, $"(B) acc.IndexOffset: expected 100, but got {acc.IndexOffset}");
				itemsRemaining = acc.GetInProgressCount();
				Assert.That(itemsRemaining == 100, $"(B) GetInProgressCount(): expected 100, but got {itemsRemaining}");

				// Since timeout has not expired, no item is returned.
				var itemsC = acc.GetItems(60, 0, true);
				Assert.That(itemsC.Count == 0, $"(C) GetItems() expected to return 0 items, but returned {itemsB.Count} item(s).");

				// Mark all items as Completed
				foreach (var item in itemsA) acc.CompleteItem(item.Index);
				foreach (var item in itemsB) acc.CompleteItem(item.Index);
			}

			// Get 99 of the 100 remaining items.
			var itemsD = acc.GetItems(60, 101, true);
			Assert.That(itemsD.Count == 100, $"(D) GetItems() expected to return 100 items, but returned {itemsD.Count} item(s).");
			Assert.That(acc.IndexOffset == 200, $"(D) acc.IndexOffset: expected 200, but got {acc.IndexOffset}");
			itemsRemaining = acc.GetInProgressCount();
			Assert.That(itemsRemaining == 100, $"(D) GetInProgressCount(): expected 100, but got {itemsRemaining}");
		}

		[Test]
		public void Accordion_Basics_B()
		{
			int itemsRemaining;
			var acc = new Accordion<MyObject>(0, 200, 100);

			// Gets 50 of the 200 with a 60 second timeout.
			var itemsA = acc.GetItems(60, 50, true);
			Assert.That(itemsA.Count == 50, $"(A) GetItems() expected to return 50 item, but returned {itemsA.Count} item(s).");
			Assert.That(acc.IndexOffset == 100, $"(A) acc.IndexOffset: expected 100, but got {acc.IndexOffset}");
			itemsRemaining = acc.GetInProgressCount();
			Assert.That(itemsRemaining == 100, $"(A) GetInProgressCount(): expected 100, but got {itemsRemaining}");

			// Gets 50 of the 200 with a 5 second timeout.
			var itemsB = acc.GetItems(5, 50, true);
			Assert.That(itemsB.Count == 50, $"(B) GetItems() expected to return 50 items, but returned {itemsB.Count} item(s).");
			Assert.That(acc.IndexOffset == 100, $"(B) acc.IndexOffset: expected 100, but got {acc.IndexOffset}");
			itemsRemaining = acc.GetInProgressCount();
			Assert.That(itemsRemaining == 100, $"(B) GetInProgressCount(): expected 100, but got {itemsRemaining}");

			// Since timeout has not expired, no item is returned.
			var itemsC = acc.GetItems(60, 0, true);
			Assert.That(itemsC.Count == 0, $"(C) GetItems() expected to return 0 items, but returned {itemsC.Count} item(s).");

			Thread.Sleep(5000);

			// Since timeout has expired on 50 items, 50 items should be returned--all with an attempt value of 1 and a timeout count of 1.
			var itemsD = acc.GetItems(60, 200, true);
			Assert.That(itemsD.Count == 50, $"(D) GetItems() expected to return 50 items, but returned {itemsD.Count} item(s).");
			var attemptCount = itemsD.Count(o => o.IssueHistory.Count == 2);
			Assert.That(attemptCount == 50, $"(D) expected to return 50 items with an issue count of 2, but returned {attemptCount} item(s).");

			// The next call should return no items, since their timeouts have not expired.
			var itemsE = acc.GetItems(60, 1, true);
			Assert.That(itemsE.Count == 0, $"(E) GetItems() expected to return 0 items, but returned {itemsD.Count} item(s).");
		}

		[Test]
		public void Accordion_Basics_C()
		{
			int itemsRetrieved = 0; ;
			var acc = new Accordion<MyObject>(0, 20000, 1000);

			while (!acc.IsFinished() && itemsRetrieved < 100000)
			{
				foreach (var item in acc.GetItems(1, 1000, true))
				{
					itemsRetrieved++;
					if (item.IssueHistory.Count < 5)
					{
						acc.RetryItem(item.Index);
					}
					else
					{
						Assert.That(item.AvailableOn > DateTime.Now, $"Expected Unavailable, but is available.");
						acc.CompleteItem(item.Index);
					}
				}
			}
			int inProgressCount = acc.GetInProgressCount();
			Assert.That(acc.GetInProgressCount() == 0, $"GetInProgressCount() expected to return 0 items, but returned {inProgressCount} item(s).");
			Assert.That(acc.IsFinished(), $"acc.IsFinished() expected to return true, but returned {acc.IsFinished()}.");
			Assert.That(itemsRetrieved == 100000, $"itemsRetrieved expected to return 100000 items, but returned {itemsRetrieved} item(s).");
		}

		[Test]
		public void Accordion_Basics_FavorNew()
		{
			var acc = new Accordion<MyObject>(0, 20, 20);

			var items = acc.GetItems(60, 5, true);
			var issues = items.Count(o => o.IssueHistory.Count == 1);
			Assert.That(items.Count(o => o.IssueHistory.Count == 1) == 5, $"Item count expected to be 5 items with 1 issuance, but had {items.Count} with {issues} items marked as 1 issuance");

			// mark the items for retry

			foreach (var item in items)
			{
				acc.RetryItem(item.Index);
			}

			var retryItems = acc.GetItems(60, 5, false);  // true == new items (IssueHistory.Count == 0) to the top of the list.
			var retryAttempts = retryItems.Count(o => o.IssueHistory.Count == 2);

			var newItems = acc.GetItems(60, 15, true);
			var newAttempts = newItems.Count(o => o.IssueHistory.Count == 1);

			Assert.That(retryAttempts == 5, $"Expected 5 retry items with 2 Attempts, but had {retryItems.Count} with {retryAttempts} items marked as 2 Attempts");
			Assert.That(newAttempts == 15, $"Expected 15 new items with 1 Attempts, but had {newItems.Count} with {newAttempts} items marked as 1 Attempts");
		}

		[Test]
		public void Accordion_Basics_FavorRetry()
		{
			var acc = new Accordion<MyObject>(0, 20, 20);

			var items = acc.GetItems(60, 5, true);
			var attempts = items.Count(o => o.IssueHistory.Count == 1);
			Assert.That(items.Count(o => o.IssueHistory.Count == 1) == 5, $"Item count expected to be 5 items with 1 Attempt, but had {items.Count} with {attempts} items marked as 1 Attempts");

			// mark the items for retry

			foreach (var item in items)
			{
				acc.RetryItem(item.Index);
			}

			var newItems = acc.GetItems(60, 15, true);
			var newAttempts = newItems.Count(o => o.IssueHistory.Count == 1);

			var retryItems = acc.GetItems(60, 5, false);  // false == retry items to the top of the list.
			var retryAttempts = retryItems.Count(o => o.IssueHistory.Count == 2);

			Assert.That(retryAttempts == 5, $"Expected 5 retry items with 2 Attempts, but had {retryItems.Count} with {retryAttempts} items marked as 2 Attempts");
			Assert.That(newAttempts == 15, $"Expected 15 new items with 1 Attempts, but had {newItems.Count} with {newAttempts} items marked as 1 Attempts");
		}

		[Test]
		public void Accordion_Basics_ExtendTime()
		{
			var acc = new Accordion<MyObject>(0, 20, 20);

			var items1 = acc.GetItems(2, 5, true);
			var items2 = acc.GetItems(2, 15, true);

			// extend AvailableOn for these 15 items for items for an extra 10 sec.
			var newAvailableOn = DateTime.Now.AddSeconds(5);
			foreach (var item in items2)
			{
				acc.ExtendItemTimeout(item.Index, newAvailableOn);
			}

			Thread.Sleep(3000);

			var standardItems1 = acc.GetItems(60, 20, true);
			Assert.That(standardItems1.Count == 5, $"(1) Expected 5 items, but had {standardItems1.Count}");

			var standardItems2 = acc.GetItems(60, 20, true);
			Assert.That(standardItems2.Count == 0, $"(1) Expected 0 items, but had {standardItems2.Count}");

			Thread.Sleep(5500);

			var standardItems3 = acc.GetItems(60, 20, true);
			Assert.That(standardItems3.Count == 15, $"(1) Expected 15 items, but had {standardItems3.Count}");

			foreach (var item in items1)
			{
				var thisItem = standardItems1.Where(o => o.Index == item.Index).FirstOrDefault();
				if (thisItem != null)
				{
					standardItems1.Remove(thisItem);
				}
			}
			Assert.That(standardItems1.Count == 0, "The 5 items returned are not the same");

			foreach (var item in items2)
			{
				var thisItem = standardItems3.Where(o => o.Index == item.Index).FirstOrDefault();
				if (thisItem != null)
				{
					standardItems2.Remove(thisItem);
				}
			}
			Assert.That(standardItems2.Count == 0, "The 15 items returned are not the same");
		}

		[Test]
		public void Accordion_Basics_Timeout1()
		{
			var acc = new Accordion<MyObject>(0, 5, 10);

			var items1 = acc.GetItems(2, 10, true);
			foreach (var item in items1)
			{
				Assert.That(item.IssueHistory.Count == 1, $"(A) Item {item.Index}: Expected 1 attempt but has {item.IssueHistory.Count}");
			}

			Thread.Sleep(2200);

			var items2 = acc.GetItems(2, 10, true);
			foreach (var item in items2)
			{
				Assert.That(item.IssueHistory.Count == 2, $"(B) Item {item.Index}: Expected 2 attempt but has {item.IssueHistory.Count}");
				acc.RetryItem(item.Index);
			}

			var items3 = acc.GetItems(2, 10, true);
			foreach (var item in items3)
			{
				Assert.That(item.IssueHistory.Count == 3, $"(C) Item {item.Index}: Expected 3 attempt but has {item.IssueHistory.Count}");
			}
		}

		[Test]
		public void Accordion_Basics_Timeout2()
		{
			var acc = new Accordion<MyObject>(0, 20, 20);

			var summary1 = acc.GetTimeoutCountSummary();
			Assert.That(summary1.Count == 0, $"Expected summary1 to have zero entries, but it has {summary1.Count} entries.");

			var items1 = acc.GetItems(1, 10, true);
			foreach (var item in items1)
			{
				Assert.That(item.IssueHistory.Count == 1, $"Item {item.Index}: Expected 1 attempt but has {item.IssueHistory.Count}");
			}
			summary1 = acc.GetTimeoutCountSummary();
			Assert.That(summary1.Count == 2, $"Expected summary1 to have 2 entries, but it has {summary1.Count} entries.");
			Assert.That(summary1.ContainsKey(0), $"Expected summary1 to have a key value of 2, but it has a different value.");
			Assert.That(summary1.ContainsKey(0) && summary1[0] == 10, $"Expected summary1 (1 issuance) to have a value of 10, but it has a different value.");
			Assert.That(summary1.ContainsKey(1), $"Expected summary1 to have a key value of 2, but it has a different value.");
			Assert.That(summary1.ContainsKey(1) && summary1[1] == 10, $"Expected summary1 (2 issuances) to have a value of 10, but it has a different value.");

			Thread.Sleep(1200);

			var items2 = acc.GetItems(1, 10, false);
			var items3 = acc.GetItems(1, 10, false);
			var summary2 = acc.GetTimeoutCountSummary();
			foreach (var item in items2)
			{
				Assert.That(item.IssueHistory.Count == 2, $"Item {item.Index} in items2: Expected 2 attempt but has {item.IssueHistory.Count}");
			}
			foreach (var item in items3)
			{
				Assert.That(item.IssueHistory.Count == 1, $"Item {item.Index} in items3: Expected 1 attempt but has {item.IssueHistory.Count}");
			}
			Assert.That(summary2.Count == 2, $"Expected summary2 to have 2 entries, but it has {summary2.Count} entries.");
			Assert.That(summary2.ContainsKey(1), $"Expected summary2 to have a key value of 2, but it has a different value.");
			Assert.That(summary2.ContainsKey(1) && summary2[1] == 10, $"Expected summary2 (1 issuance) to have a value of 10, but it has a different value.");
			Assert.That(summary2.ContainsKey(2), $"Expected summary2 to have a key value of 2, but it has a different value.");
			Assert.That(summary2.ContainsKey(2) && summary2[2] == 10, $"Expected summary2 (2 issuances) to have a value of 10, but it has a different value.");
		}

		[Test]
		public void Accordion_Basics_Payload()
		{
			var acc = new Accordion<MyObject>(0, 5, 10);

			var item = acc.GetItem(1, true);
			Assert.That(item.Payload == default, $"Payload not in default state");
			acc.SetItemPayload(item.Index, new MyObject
			{
				Index = item.Index,
				Succeeded = (item.Index % 2 == 1),
				Message = "Hello"
			});
			Thread.Sleep(1200);
			// item is a local copy, and state should be unchanged by the above method,.
			Assert.That(item.Payload == default, $"Payload did not remain in default state");

			item = acc.GetItem(1, false);
			Assert.That(item.Payload != default && item.Payload.Message == "Hello", $"Payload not in new state");
		}
	}
}