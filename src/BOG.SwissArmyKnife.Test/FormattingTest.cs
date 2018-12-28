using NUnit.Framework;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class FormattingTest
    {
        [Test, Description("KiloToYotta, double == 0.0")]
        public void Formatting_KiloToYotta_d0()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)0);

            Assert.That(answer == "0", "0.0 == 0");
        }

        [Test, Description("KiloToYotta, long == 0.0")]
        public void Formatting_KiloToYotta_l0()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)0);

            Assert.That(answer == "0", "0 == 0");
        }

        [Test, Description("KiloToYotta, double == 1.0")]
        public void Formatting_KiloToYotta_d1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)1);

            Assert.That(answer == "1", "1.0 == 1");
        }

        [Test, Description("KiloToYotta, long == 1")]
        public void Formatting_KiloToYotta_l1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)1);

            Assert.That(answer == "1", "1 == 1");
        }

        [Test, Description("KiloToYotta, double == 1023.0")]
        public void Formatting_KiloToYotta_d1023()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)1023);

            Assert.That(answer == "1,023", "1023.0 == 1,023");
        }

        [Test, Description("KiloToYotta, double == 1023.0, baseValue = 1000")]
        public void Formatting_KiloToYotta_d1023_b1000()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)1023, false);

            Assert.That(answer == "1.0K", "1023.0 == 1.0K");
        }

        [Test, Description("KiloToYotta, long == 1023")]
        public void Formatting_KiloToYotta_l1023()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)1023);

            Assert.That(answer == "1,023", "1023 == 1,023");
        }

        [Test, Description("KiloToYotta, long == 1023, baseValue = 1000")]
        public void Formatting_KiloToYotta_l1023_b1000()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)1023, false);

            Assert.That(answer == "1.0K", "1023 == 1.0K");
        }

        [Test, Description("KiloToYotta, double == 1024.0")]
        public void Formatting_KiloToYotta_d1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)1024);

            Assert.That(answer == "1.0K", "1024.0 == 1.0K");
        }

        [Test, Description("KiloToYotta, long == 1024")]
        public void Formatting_KiloToYotta_l1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)1024);

            Assert.That(answer == "1.0K", "1024 == 1.0K");
        }

        [Test, Description("KiloToYotta, double == 1024.0 * 1024.0 - 1")]
        public void Formatting_KiloToYotta_d1024x1024_1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024 * 1024 - 1));

            Assert.That(answer == "1,024.0K", "1024.0 * 1024.0 - 1 == 1,024.0K");
        }

        [Test, Description("KiloToYotta, long == 1024")]
        public void Formatting_KiloToYotta_l1024x1024_1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)(1024 * 1024 - 1));

            Assert.That(answer == "1,024.0K", "1024 * 1024 - 1 == 1,024.0K");
        }

        [Test, Description("KiloToYotta, double == 1024.0 * 1024.0")]
        public void Formatting_KiloToYotta_d1024x1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024 * 1024));

            Assert.That(answer == "1.00M", "1024.0 * 1024.0 == 1.00M");
        }

        [Test, Description("KiloToYotta, long == 1024 * 1024")]
        public void Formatting_KiloToYotta_l1024x1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)(1024 * 1024));

            Assert.That(answer == "1.00M", "1024.0 * 1024.0 == 1.00M");
        }

        [Test, Description("KiloToYotta, double == 1024.0 * 1024.0 * 1024.0 - 1")]
        public void Formatting_KiloToYotta_d1024x1024x1024_1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024 * 1024 * 1024 - 1));

            Assert.That(answer == "1,024.00M", "1024.0 * 1024.0 * 1024.0 - 1 == 1,024.00M");
        }

        [Test, Description("KiloToYotta, long == 1024.0 * 1024.0 * 1024.0 - 1")]
        public void Formatting_KiloToYotta_l1024x1024x1024_1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)(1024 * 1024 * 1024 - 1));

            Assert.That(answer == "1,024.00M", "1024.0 * 1024.0 * 1024.0 - 1 == 1,024.00M");
        }

        [Test, Description("KiloToYotta, double == 1024.0 * 1024.0 * 1024.0")]
        public void Formatting_KiloToYotta_d1024x1024x1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024.0 * 1024.0 * 1024.0));

            Assert.That(answer == "1.000G", "1024.0 * 1024.0 * 1024.0 == 1.000G");
        }

        [Test, Description("KiloToYotta, long == 1024.0 * 1024.0 * 1024.0")]
        public void Formatting_KiloToYotta_l1024x1024x1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((long)(1024 * 1024 * 1024));

            Assert.That(answer == "1.000G", "1024 * 1024 * 1024 == 1.000G");
        }

        [Test, Description("KiloToYotta, long == 1024.0 * 1024.0 * 1024.0 * 1024.0 - 1")]
        public void Formatting_KiloToYotta_d1024x1024x1024x1024_1()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024.0 * 1024.0 * 1024.0 * 1024.0 - 1));

            Assert.That(answer == "1,024.000G", "1024.0 * 1024.0 * 1024.0 * 1024.0 - 1 == 1,024.000G");
        }

        [Test, Description("KiloToYotta, long == 1024.0 * 1024.0 * 1024.0 * 1024.0")]
        public void Formatting_KiloToYotta_d1024x1024x1024x1024()
        {
            string answer = BOG.SwissArmyKnife.Formatting.KiloToYotta((double)(1024.0 * 1024.0 * 1024.0 * 1024.0));

            Assert.That(answer == "1.000T", "1024.0 * 1024.0 * 1024.0 * 1024.0 == 1.000T");
        }

        [Test, Description("RJLZ(): length less than length parameter")]
        public void Formatting_RJLZ_long_OK()
        {
            long number = 72889;
            int length = 6;
            string Result = Formatting.RJLZ(number, length);
            Assert.IsTrue(string.Compare("072889", Result) == 0);
        }

        [Test, Description("RJLZ(): length equal to length parameter")]
        public void Formatting_RJLZ_long_Same()
        {
            long number = 72889;
            int length = 5;
            string Result = Formatting.RJLZ(number, length);
            Assert.IsTrue(string.Compare("72889", Result) == 0);
        }

        [Test, Description("RJLZ(): length less than length parameter")]
        public void Formatting_RJLZ_long_lessthan()
        {
            long number = 72889;
            int length = 4;
            string Result = Formatting.RJLZ(number, length);
            Assert.IsTrue(string.Compare("72889", Result) == 0);
        }
        [Test, Description("RJLZ(): 0")]
        public void Formatting_RJLZ_long_0()
        {
            long number = 0;
            int length = 6;
            string Result = Formatting.RJLZ(number, length);
            Assert.IsTrue(string.Compare("000000", Result) == 0);
        }
    }
}
