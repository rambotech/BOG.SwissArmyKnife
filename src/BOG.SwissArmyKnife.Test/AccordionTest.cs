using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using NUnit.Framework.Constraints;
using System.Threading;
using System.Linq;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class AccordionTest
    {
        /// <summary>
        /// Validate that default values are correct
        /// </summary>
        [Test]
        public void Accordion_Init_Default()
        {
            Accordion acc = new Accordion();

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
            Accordion acc = new Accordion(55, 300, 200);

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
            ActualValueDelegate<object> testDelegate = () => new Accordion(-1, 1, 1);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "indexStart must be >= 0");
        }

        /// <summary>
        /// Check for a count value over 0
        /// </summary>
        [Test]
        public void Accordion_Init_Bad_CountTooLow()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(1, 0, 1);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "count must be > 0");
        }

        /// <summary>
        /// Check for a max value over 0
        /// </summary>
        [Test]
        public void Accordion_Init_MaxBad()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(1, 1, 9);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be >= 10");
        }

        /// <summary>
        /// Check that a max value under 10 is invalid
        /// </summary>
        [Test]
        public void Accordion_Init_Max_Bad_Under_MinValue()
        {
            TestDelegate testDelegate = () => new Accordion(2, 2, 9);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be >= 10");
        }

        /// <summary>
        /// Check that a max value of 10 (lowest) is OK
        /// </summary>
        [Test]
        public void Accordion_Init_Max_Good_MinValue()
        {
            TestDelegate testDelegate = () => new Accordion(2, 2, 10);
            Assert.DoesNotThrow(testDelegate, "max should allow min value of 10, but does not.");
        }

        [Test]
        public void Accordion_Basics_A()
        {
            int itemsRemaining;
            Accordion acc = new Accordion(0, 200, 100);

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
            Accordion acc = new Accordion(0, 200, 100);

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
            Assert.That(itemsC.Count == 0, $"(C) GetItems() expected to return 0 items, but returned {itemsB.Count} item(s).");

            Thread.Sleep(5000);

            // Since timeout has expired on 50 items, 50 items should be returned--all with an attempt value of 1 and a timeout count of 1.
            var itemsD = acc.GetItems(60, 200, false);
            Assert.That(itemsD.Count == 50, $"(D) GetItems() expected to return 50 items, but returned {itemsC.Count} item(s).");
            var attemptCount = itemsD.Count(o => o.Attempts == 1);
            Assert.That(attemptCount == 50, $"(D) expected to return 50 items with an Attempt count of 1, but returned {attemptCount} item(s).");
            var timeoutCount = itemsD.Count(o => o.Timeouts == 1);
            Assert.That(timeoutCount == 50, $"(D) expected to return 50 items with an Timeout count of 1, but returned {timeoutCount} item(s).");

            // The next call should return no items, since their timeouts have not expired.
            var itemsE = acc.GetItems(60, 1, false);
            Assert.That(itemsE.Count == 0, $"(E) GetItems() expected to return 0 items, but returned {itemsC.Count} item(s).");
        }

        [Test]
        public void Accordion_Basics_C()
        {
            int itemsRetrieved = 0; ;
            Accordion acc = new Accordion(0, 20000, 1000);

            while (!acc.IsFinished() && itemsRetrieved < 100000)
            {
                var items = acc.GetItems(1, 1000, false);
                foreach (var item in items)
                {
                    itemsRetrieved++;
                    if (item.Attempts < 5)
                    {
                        acc.RetryItem(item.Index);
                    }
                    else
                    {
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
            Accordion acc = new Accordion(0, 20, 20);

            var items = acc.GetItems(60, 5, true);
            var attempts = items.Count(o => o.Attempts == 1);
            Assert.That(items.Count(o => o.Attempts == 1) == 5, $"Item count expected to be 5 items with 1 Attempt, but had {items.Count} with {attempts} items marked as 1 Attempts");

            // mark the items for retry

            foreach (var item in items)
            {
                acc.RetryItem(item.Index);
            }

            var retryItems = acc.GetItems(60, 5, false);  // true == new items (Attempts == 0) to the top of the list.
            var retryAttempts = retryItems.Count(o => o.Attempts == 2);

            var newItems = acc.GetItems(60, 15, true);
            var newAttempts = newItems.Count(o => o.Attempts == 1);

            Assert.That(retryAttempts == 5, $"Expected 5 retry items with 2 Attempts, but had {retryItems.Count} with {retryAttempts} items marked as 2 Attempts");
            Assert.That(newAttempts == 15, $"Expected 15 new items with 1 Attempts, but had {newItems.Count} with {newAttempts} items marked as 1 Attempts");
        }

        [Test]
        public void Accordion_Basics_FavorRetry()
        {
            Accordion acc = new Accordion(0, 20, 20);

            var items = acc.GetItems(60, 5, true);
            var attempts = items.Count(o => o.Attempts == 1);
            Assert.That(items.Count(o => o.Attempts == 1) == 5, $"Item count expected to be 5 items with 1 Attempt, but had {items.Count} with {attempts} items marked as 1 Attempts");

            // mark the items for retry

            foreach (var item in items)
            {
                acc.RetryItem(item.Index);
            }

            var newItems = acc.GetItems(60, 15, true);
            var newAttempts = newItems.Count(o => o.Attempts == 1);

            var retryItems = acc.GetItems(60, 5, false);  // false == retry items to the top of the list.
            var retryAttempts = retryItems.Count(o => o.Attempts == 2);

            Assert.That(retryAttempts == 5, $"Expected 5 retry items with 2 Attempts, but had {retryItems.Count} with {retryAttempts} items marked as 2 Attempts");
            Assert.That(newAttempts == 15, $"Expected 15 new items with 1 Attempts, but had {newItems.Count} with {newAttempts} items marked as 1 Attempts");
        }

        [Test]
        public void Accordion_Basics_ExtendDeadline()
        {
            Accordion acc = new Accordion(0, 20, 20);

            var items1 = acc.GetItems(2, 5, true);
            var items2 = acc.GetItems(2, 15, true);

            // extend the deadline for these 15 items for items for an extra 10 sec.
            var newDeadline = DateTime.Now.AddSeconds(5);
            foreach (var item in items2)
            {
                acc.ExtendItemTimeout(item.Index, newDeadline);
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
            Accordion acc = new Accordion(0, 5, 10);

            var items1 = acc.GetItems(2, 10, true);
            foreach (var item in items1)
            {
                Assert.That(item.Attempts == 1, $"Item {item.Index}: Expected 1 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts== 0, $"Item {item.Index}: Expected 0 timeouts but has {item.Timeouts}");
            }

            Thread.Sleep(2200);

            var items2 = acc.GetItems(2, 10, true);
            foreach (var item in items2)
            {
                Assert.That(item.Attempts == 1, $"Item {item.Index}: Expected 1 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 1, $"Item {item.Index}: Expected 1 timeouts but has {item.Timeouts}");
                acc.RetryItem(item.Index);
            }

            var items3 = acc.GetItems(2, 10, true);
            foreach (var item in items3)
            {
                Assert.That(item.Attempts == 2, $"Item {item.Index}: Expected 2 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 0, $"Item {item.Index}: Expected 0 timeouts but has {item.Timeouts}");
            }
        }

        [Test]
        public void Accordion_Basics_Timeout2()
        {
            Accordion acc = new Accordion(0, 5, 10);

            var items1 = acc.GetItems(2, 10, true);
            foreach (var item in items1)
            {
                Assert.That(item.Attempts == 1, $"Item {item.Index}: Expected 1 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 0, $"Item {item.Index}: Expected 0 timeouts but has {item.Timeouts}");
            }
            var summary1 = acc.GetTimeoutCountSummary();
            Assert.That(summary1.Count == 1, $"Expected summary1 to have one entry, but it has {summary1.Count} entries.");
            Assert.That(summary1.ContainsKey(0), $"Expected summary1 to have a key value of 0, but it has a different value.");
            Assert.That(summary1.ContainsKey(0) && summary1[0] == 5, $"Expected summary to have a value of 5, but it has a different value.");

            Thread.Sleep(2200);

            var items2 = acc.GetItems(2, 10, true);
            foreach (var item in items2)
            {
                Assert.That(item.Attempts == 1, $"Item {item.Index} in items2: Expected 1 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 1, $"Item {item.Index} in items2: Expected 1 timeouts but has {item.Timeouts}");
                acc.RetryItem(item.Index);
            }

            var summary2 = acc.GetTimeoutCountSummary();
            Assert.That(summary2.Count == 1, $"Expected summary2 to have one entry, but it has {summary2.Count} entries.");
            Assert.That(summary2.ContainsKey(1), $"Expected summary2 to have a key value of 1, but it has a different value.");
            Assert.That(summary1.ContainsKey(1) && summary2[1] == 5, $"Expected summary2 to have a value of 5, but it has a different value.");

            var items3 = acc.GetItems(2, 10, true);
            foreach (var item in items3)
            {
                Assert.That(item.Attempts == 2, $"Item {item.Index} in items3: Expected 1 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 0, $"Item {item.Index} in items3: Expected 1 timeouts but has {item.Timeouts}");
                acc.RetryItem(item.Index);
            }

            var summary3 = acc.GetTimeoutCountSummary();
            Assert.That(summary3.Count == 1, $"Expected summary3 to have one entry, but it has {summary3.Count} entries.");
            Assert.That(summary3.ContainsKey(0), $"Expected summary3 to have a key value of 1, but it has a different value.");
            Assert.That(summary3.ContainsKey(0) && summary3[0] == 5, $"Expected summary3 to have a value of 5, but it has a different value.");

            var items4 = acc.GetItems(2, 10, true);
            foreach (var item in items4)
            {
                Assert.That(item.Attempts == 2, $"Item {item.Index} in items4: Expected 2 attempt but has {item.Attempts}");
                Assert.That(item.Timeouts == 0, $"Item {item.Index} in items4: Expected 0 timeouts but has {item.Timeouts}");
            }
        }
    }
}