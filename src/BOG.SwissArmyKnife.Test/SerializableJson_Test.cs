using BOG.SwissArmyKnife.Test.Support;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class SerializableJson_Test
    {
        [Test]
        public void SerializableJson_ObjectToContainerToObject_ImplicitAlgorithm()
        {
            MyDataSet original = MakeMyDataSet();
            int setCount = original.coll.Count;
            string password = "Uncomplicated";
            string salt = "JustAsEasy";

            string rawContent = ObjectJsonSerializer<MyDataSet>.CreateDocumentFormat(original);

            string transitContent = ObjectJsonSerializer<MyDataSet>.CreateTransitContainerForObject(original, password, salt);

            MyDataSet result = ObjectJsonSerializer<MyDataSet>.CreateObjectFromTransitContainer(transitContent, password, salt);

            Assert.That(string.Compare(result.s1, original.s1, true) == 0, "s1 not same");
            Assert.That(result.i16 == original.i16, "i16 not same");
            Assert.That(result.i32 == original.i32, "i32 not same");
            Assert.That(result.i64 == original.i64, "i64 not same");
            Assert.That(result.Timestamp == original.Timestamp, "Timestamp not same");
            Assert.That(result.coll.Count == setCount, "coll.Count not " + setCount.ToString());
            Assert.That(result.coll.Count == original.coll.Count, "coll.Count not same");
            for (int index = 0; index < setCount; index++)
            {
                Assert.That(string.Compare(result.coll[index], original.coll[index], true) == 0, "coll[] not same at index " + index.ToString());
            }
        }

        [Test]
        public void SerializableJson_ObjectToContainerToObject_ExplicitAlgorithm()
        {
            MyDataSet original = MakeMyDataSet();
            int setCount = original.coll.Count;
            string password = "Uncomplicated";
            string salt = "JustAsEasy";

            SymmetricAlgorithm algorithm = TripleDES.Create();

            string transitContent = ObjectJsonSerializer<MyDataSet>.CreateTransitContainerForObject(original, password, salt, algorithm);

            MyDataSet result = ObjectJsonSerializer<MyDataSet>.CreateObjectFromTransitContainer(transitContent, password, salt, algorithm);

            Assert.That(string.Compare(result.s1, original.s1, true) == 0, "s1 not same");
            Assert.That(result.i16 == original.i16, "i16 not same");
            Assert.That(result.i32 == original.i32, "i32 not same");
            Assert.That(result.i64 == original.i64, "i64 not same");
            Assert.That(result.Timestamp == original.Timestamp, "Timestamp not same");
            Assert.That(result.coll.Count == setCount, "coll.Count not " + setCount.ToString());
            Assert.That(result.coll.Count == original.coll.Count, "coll.Count not same");
            for (int index = 0; index < setCount; index++)
            {
                Assert.That(string.Compare(result.coll[index], original.coll[index], true) == 0, "coll[] not same at index " + index.ToString());
            }
        }

        private MyDataSet MakeMyDataSet()
        {
            var result = new MyDataSet();
            result.s1 = "A large set of strings in the list";

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BOG.SwissArmyKnife.Test.BulkTestData.UrlTestItems.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        result.coll.Add(reader.ReadLine());
                    }
                }
            }
            return result;
        }
    }
}
