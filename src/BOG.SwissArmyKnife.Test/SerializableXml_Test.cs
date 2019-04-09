using BOG.SwissArmyKnife.Test.Support;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace BOG.SwissArmyKnife.Test
{
    [TestFixture]
    public class SerializableXml_Test
    {
        [Test]
        public void SerializableJson_ObjectToContainerToObject_ImplicitAlgorithm()
        {
            MyDataSet original = MakeMyDataSet();
            int setCount = original.coll.Count;
            string password = "Uncomplicated";
            string salt = "JustAsEasy";

            string rawContent = ObjectXMLSerializer<MyDataSet>.CreateDocumentFormat(original);
            //int rawContentLength = rawContent.Length;

            string transitContent = ObjectXMLSerializer<MyDataSet>.CreateTransitContainerForObject(original, password, salt);
            //int transitContentLength = transitContent.Length;

            MyDataSet result = ObjectXMLSerializer<MyDataSet>.CreateObjectFromTransitContainer(transitContent, password, salt);

            Assert.AreEqual(result.s1, original.s1, "s1 not same");
            Assert.AreEqual(result.i16, original.i16, "i16 not same");
            Assert.AreEqual(result.i32, original.i32, "i32 not same");
            Assert.AreEqual(result.i64, original.i64, "i64 not same");
            Assert.AreEqual(result.Timestamp, original.Timestamp, "Timestamp not same");
            Assert.AreEqual(result.coll.Count, setCount, "coll.Count not " + setCount.ToString());
            Assert.AreEqual(result.coll.Count, original.coll.Count, "coll.Count not same");
            for (int index = 0; index < setCount; index++)
            {
                Assert.AreEqual(result.coll[index], original.coll[index], "coll[] not same at index " + index.ToString());
            }
        }

        [Test]
        public void SerializableJson_ObjectToContainerToObject_ExplicitAlgorithm()
        {
            MyDataSet original = MakeMyDataSet();
            int setCount = original.coll.Count;
            string password = "Uncomplicated";
            string salt = "JustAsEasy";

            SymmetricAlgorithm algorithm = new DESCryptoServiceProvider();

            string transitContent = ObjectXMLSerializer<MyDataSet>.CreateTransitContainerForObject(original, password, salt, algorithm);

            MyDataSet result = ObjectXMLSerializer<MyDataSet>.CreateObjectFromTransitContainer(transitContent, password, salt, algorithm);

            Assert.AreEqual(result.s1, original.s1, "s1 not same");
            Assert.AreEqual(result.i16, original.i16, "i16 not same");
            Assert.AreEqual(result.i32, original.i32, "i32 not same");
            Assert.AreEqual(result.i64, original.i64, "i64 not same");
            Assert.AreEqual(result.Timestamp, original.Timestamp, "Timestamp not same");
            Assert.AreEqual(result.coll.Count, setCount, "coll.Count not " + setCount.ToString());
            Assert.AreEqual(result.coll.Count, original.coll.Count, "coll.Count not same");
            for (int index = 0; index < setCount; index++)
            {
                Assert.AreEqual(result.coll[index], original.coll[index], "coll[] not same at index " + index.ToString());
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
