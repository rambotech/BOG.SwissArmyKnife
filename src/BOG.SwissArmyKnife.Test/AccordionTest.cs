using NUnit.Framework;
using System;
using System.Collections.Generic;
using BOG.SwissArmyKnife;
using NUnit.Framework.Constraints;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class AccordionTest
    {
        [Test]
        public void Accordion_Add300_Max200_Init_Default()
        {
            Accordion acc = new Accordion();

            Assert.That(acc.IndexStart == 0, $"acc.IndexStart: expected 0, but got ${acc.IndexStart}");
            Assert.That(acc.IndexEnd == 0, $"acc.IndexEnd: expected 0, but got ${acc.IndexEnd}");
            Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got ${acc.IndexOffset}");
            Assert.That(acc.MaxInProgress == 0, $"acc.MaxInProgress: expected 0, but got ${acc.MaxInProgress}");
            Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got ${acc.ItemsInProgress.Count}");
        }

        [Test]
        public void Accordion_Add300_Max200_Init_Specific()
        {
            Accordion acc = new Accordion(55, 300, 200);

            Assert.That(acc.IndexStart == 55, $"acc.IndexStart: expected 55, but got ${acc.IndexStart}");
            Assert.That(acc.IndexEnd == 354, $"acc.IndexEnd: expected 354, but got ${acc.IndexEnd}");
            Assert.That(acc.IndexOffset == 0, $"acc.IndexOffset: expected 0, but got ${acc.IndexOffset}");
            Assert.That(acc.MaxInProgress == 200, $"acc.MaxInProgress: expected 200, but got ${acc.MaxInProgress}");
            Assert.That(acc.ItemsInProgress.Count == 0, $"acc.ItemsInProgress.Count: expected 0, but got ${acc.ItemsInProgress.Count}");
        }

        [Test]
        public void Accordion_Add300_Max200_Init_BadIndexStart()
        {
            ActualValueDelegate<object> testDelegate = () => new Accordion(-1, 1, 1);

            Assert.That(testDelegate, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Accordion_Add300_Max200_C()
        {
            Accordion acc = new Accordion(0, 300, 200);
            var result = false;
            AccordionItem item;
            for (int index = 1; index <= 200; index++)
            {
                result = acc.GetItem(60, out item);
            }

            Assert.That(acc.IndexOffset == 200, $"acc.IndexOffset: expected 200, but got ${acc.IndexOffset}");
            Assert.That(result, $"GetItem iteration #200 should be true but was false");

            result = acc.GetItem(60, out item);
            Assert.That(!result, $"GetItem iteration #201 should be false but was true");
            Assert.That(item == null, $"GetItem iteration #201 should return null object.");
        }

        [Test]
        public void Accordion_Add300_Max200_D()
        {                                                                                                  b
            Accordion acc = new Accordion(0, 100, 50);
            var result = false;
            AccordionItem item;
            for (int index = 1; index <= 50; index++)
            {
                result = acc.GetItem(60, out item);
            }

            Assert.That(acc.IndexOffset == 200, $"acc.IndexOffset: expected 200, but got ${acc.IndexOffset}");
            Assert.That(result, $"GetItem iteration #200 should be true but was false");

            result = acc.GetItem(60, out item);
            Assert.That(!result, $"GetItem iteration #201 should be false but was true");
            Assert.That(item == null, $"GetItem iteration #201 should return null object.");
        }
    }
}
