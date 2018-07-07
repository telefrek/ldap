using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.Security.LDAP.Protocol.BER.Test
{
    [TestClass]
    public class BEREncodingTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task EncodingWriterTests()
        {
            // Write an int
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, 1);

                var bytes = ms.ToArray();
                Assert.AreEqual(3, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(1, bytes[2], "Wrong value encoded");
            }

            // Write a bool
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, false);
                await BEREncoding.WriteAsync(ms, true);

                var bytes = ms.ToArray();
                Assert.AreEqual(6, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.BOOLEAN, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(0, bytes[2], "Wrong value encoded");

                Assert.AreEqual((int)BERType.BOOLEAN, bytes[3], "Wrong tag value");
                Assert.AreEqual(1, bytes[4], "Wrong length value");
                Assert.AreEqual(0xFF, bytes[5], "Wrong value encoded");
            }

            // Write a null
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteNullAsync(ms);

                var bytes = ms.ToArray();
                Assert.AreEqual(2, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.NULL, bytes[0], "Wrong tag value");
                Assert.AreEqual(0, bytes[1], "Wrong length value");
            }

            // Write a string
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, "Hello");

                var bytes = ms.ToArray();
                Assert.AreEqual(7, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.OCTET_STRING, bytes[0], "Wrong tag value");
                Assert.AreEqual(5, bytes[1], "Wrong length value");
            }
        }

        [TestMethod]
        public async Task TestStringReadWrite()
        {
            // Write a string
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, "Hello");

                var bytes = ms.ToArray();
                Assert.AreEqual(7, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.OCTET_STRING, bytes[0], "Wrong tag value");
                Assert.AreEqual(5, bytes[1], "Wrong length value");

                ms.Position = 0;

                var value = await BEREncoding.ReadStringAsync(ms);
                Assert.IsNotNull(value, "String should not be null");
                Assert.AreEqual("Hello", value, "Wrong value returned from reader");
            }
        }

        [TestMethod]
        public async Task TestIntReadWrite()
        {
            // Write an int
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, 1);

                var bytes = ms.ToArray();
                Assert.AreEqual(3, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(1, bytes[2], "Wrong value encoded");

                ms.Position = 0;

                var val = await BEREncoding.ReadIntAsync(ms);
                Assert.AreEqual(1, val, "Read incorrect value");
            }

            // Write a failover int
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, 257);

                var bytes = ms.ToArray();
                Assert.AreEqual(4, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(2, bytes[1], "Wrong length value");

                ms.Position = 0;

                var val = await BEREncoding.ReadIntAsync(ms);
                Assert.AreEqual(257, val, "Read incorrect value");
            }

            // Write a big int
            using (var ms = new MemoryStream())
            {
                await BEREncoding.WriteAsync(ms, 1234567);

                var bytes = ms.ToArray();
                Assert.AreEqual(5, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)BERType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(3, bytes[1], "Wrong length value");

                ms.Position = 0;

                var val = await BEREncoding.ReadIntAsync(ms);
                Assert.AreEqual(1234567, val, "Read incorrect value");
            }
        }
    }
}