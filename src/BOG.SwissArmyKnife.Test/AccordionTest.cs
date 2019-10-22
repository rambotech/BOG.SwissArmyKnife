using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using NUnit.Framework.Constraints;
using System.Threading;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class AccordionTest
    {
        [Test]
        public void Accordion_Init_Default()
        {
            Accordion acc = new Accordion();

            Assert.That(acc.IndexStart == 0, $"acc.IndexStart: expected 0, but got ${acc.IndexStart}");
            Assert.That(acc.IndexEnd == 0, $"acc.IndexEnd: expected 0, but got ${acc.IndexEnd}");
            Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got ${acc.IndexOffset}");
            Assert.That(acc.MaxInProgress == 0, $"acc.MaxInProgress: expected 0, but got ${acc.MaxInProgress}");
            Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got ${acc.ItemsInProgress.Count}");
        }

        [Test]
        public void Accordion_Init_Specific()
        {
            Accordion acc = new Accordion(55, 300, 200);

            Assert.That(acc.IndexStart == 55, $"acc.IndexStart: expected 55, but got ${acc.IndexStart}");
            Assert.That(acc.IndexEnd == 355, $"acc.IndexEnd: expected 354, but got ${acc.IndexEnd}");
            Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got ${acc.IndexOffset}");
            Assert.That(acc.MaxInProgress == 200, $"acc.MaxInProgress: expected 200, but got ${acc.MaxInProgress}");
            Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got ${acc.ItemsInProgress.Count}");
        }

        [Test]
        public void Accordion_Init_BadIndexStart()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(-1, 1, 1);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "indexStart must be >= 0");
        }

        [Test]
        public void Accordion_Init_BadCount()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(1, 0, 1);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "count must be >= 0");
        }

        [Test]
        public void Accordion_Init_BadMaxInProgress_Lt_10()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(1, 50000, 9);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be >= 10");
        }

        [Test]
        public void Accordion_Init_BadMaxInProgress_Gt_20000()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(1, 50000, 20001);
            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>(), "maxInProgress must be <= 20000");
        }

        [Test]
        public void Accordion_Add300_Max200_Get200_Wait()
        {
            Accordion acc = new Accordion(0, 300, 200);
            var result = false;
            AccordionItem item;
            for (int index = 1; index <= 200; index++)
            {
                result = acc.GetItem(5, out item);
                Assert.That(result, $"GetItem iteration #1 ({index}) should be true but was false");
            }

            Assert.That(result, $"GetItem iteration #200 should be true but was false");
            Assert.That(acc.IndexOffset == 200, $"acc.IndexOffset: expected 200, but got ${acc.IndexOffset}");
            Assert.That(acc.GetInProgressCount() == 200, $"acc.IndexOffset: expected 200, but got ${acc.IndexOffset}");

            result = acc.GetItem(60, out item);
            Assert.That(!result, $"GetItem iteration #201 should be false but was true");
            Assert.That(item == null, $"GetItem iteration #201 should return null object.");

            Thread.Sleep(6000);
            Assert.That(acc.GetInProgressCount() == 200, $"acc.IndexOffset: expected 200, but got ${acc.IndexOffset}");
            result = acc.GetItem(60, out item);
            Assert.That(result, $"GetItem iteration should be true but was false");
            Assert.That(item != null, $"GetItem iteration #201 returned null object.");
            for (int index = 1; index <= 199; index++)
            {
                result = acc.GetItem(5, out item);
                Assert.That(result, $"GetItem iteration #2 ({index}) should be true but was false");
            }
            result = acc.GetItem(5, out item);
            Assert.That(!result, $"GetItem iteration #2 #200 should be false but was true");
            Assert.That(item == null, $"GetItem iteration #2 #200 should return null object.");

        }

        [Test]
        public void Accordion_Add3000_Max500_Full()
        {
            var tracked = new Dictionary<Int64, int>();
            Accordion acc = new Accordion(900, 3900, 500);
            AccordionItem item;
            int totalAttempts = 0;
            while (!acc.IsFinished())
            {
                var result = acc.GetItem(3, out item);
                if (item != null )
                {
                    if (item.Attempts == 3)
                    {
                        acc.CompleteItem(item);
                    }
                    else  if (tracked.ContainsKey(item.Index))
                    {
                        tracked[item.Index]++;
                        totalAttempts++;
                    }
                    else
                    {
                        tracked.Add(item.Index, 0);
                    }
                }
            }

            Assert.That(tracked.Keys.Count == 3000, $"tracked.Keys.Count: expected 3000, but got ${tracked.Keys.Count}");
            Assert.That(totalAttempts == 6000, $"totalAttempts: expected 6000, but got ${totalAttempts}");
        }
    }
}
