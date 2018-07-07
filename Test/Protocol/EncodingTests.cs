using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.Security.LDAP.Protocol.Test
{
    [TestClass]
    public class EncodingTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task EncodingWriterTests()
        {
            // Write an int
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, 1);

                var bytes = ms.ToArray();
                Assert.AreEqual(3, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(1, bytes[2], "Wrong value encoded");
            }

            // Write a bool
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, false);
                await ProtocolEncoding.WriteAsync(ms, true);

                var bytes = ms.ToArray();
                Assert.AreEqual(6, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.BOOLEAN, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(0, bytes[2], "Wrong value encoded");

                Assert.AreEqual((int)EncodingType.BOOLEAN, bytes[3], "Wrong tag value");
                Assert.AreEqual(1, bytes[4], "Wrong length value");
                Assert.AreEqual(0xFF, bytes[5], "Wrong value encoded");
            }

            // Write a null
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteNullAsync(ms);

                var bytes = ms.ToArray();
                Assert.AreEqual(2, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.NULL, bytes[0], "Wrong tag value");
                Assert.AreEqual(0, bytes[1], "Wrong length value");
            }

            // Write a string
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, "Hello");

                var bytes = ms.ToArray();
                Assert.AreEqual(7, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.OCTET_STRING, bytes[0], "Wrong tag value");
                Assert.AreEqual(5, bytes[1], "Wrong length value");
            }
        }

        [TestMethod]
        public async Task TestStringReadWrite()
        {
            // Write a string
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, "Hello");

                var bytes = ms.ToArray();
                Assert.AreEqual(7, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.OCTET_STRING, bytes[0], "Wrong tag value");
                Assert.AreEqual(5, bytes[1], "Wrong length value");

                ms.Position = 0;

                var value = await ProtocolEncoding.ReadStringAsync(ms);
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
                await ProtocolEncoding.WriteAsync(ms, 1);

                var bytes = ms.ToArray();
                Assert.AreEqual(3, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(1, bytes[2], "Wrong value encoded");

                ms.Position = 0;

                var val = await ProtocolEncoding.ReadIntAsync(ms);
                Assert.AreEqual(1, val, "Read incorrect value");
            }

            // Write a failover int
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, 257);

                var bytes = ms.ToArray();
                Assert.AreEqual(4, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(2, bytes[1], "Wrong length value");

                ms.Position = 0;

                var val = await ProtocolEncoding.ReadIntAsync(ms);
                Assert.AreEqual(257, val, "Read incorrect value");
            }

            // Write a big int
            using (var ms = new MemoryStream())
            {
                await ProtocolEncoding.WriteAsync(ms, 1234567);

                var bytes = ms.ToArray();
                Assert.AreEqual(5, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(3, bytes[1], "Wrong length value");

                ms.Position = 0;

                var val = await ProtocolEncoding.ReadIntAsync(ms);
                Assert.AreEqual(1234567, val, "Read incorrect value");
            }
        }
    }
}