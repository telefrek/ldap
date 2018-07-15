using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.LDAP.Protocol.Test
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
                await new LDAPWriter(ms).WriteAsync(1);

                var bytes = ms.ToArray();
                Assert.AreEqual(3, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.INTEGER, bytes[0], "Wrong tag value");
                Assert.AreEqual(1, bytes[1], "Wrong length value");
                Assert.AreEqual(1, bytes[2], "Wrong value encoded");
            }

            // Write a bool
            using (var ms = new MemoryStream())
            {
                var writer = new LDAPWriter(ms);
                await writer.WriteAsync(false);
                await writer.WriteAsync(true);

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
                await new LDAPWriter(ms).WriteNullAsync();

                var bytes = ms.ToArray();
                Assert.AreEqual(2, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.NULL, bytes[0], "Wrong tag value");
                Assert.AreEqual(0, bytes[1], "Wrong length value");
            }

            // Write a string
            using (var ms = new MemoryStream())
            {
                await new LDAPWriter(ms).WriteAsync("Hello");

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
                await new LDAPWriter(ms).WriteAsync("Hello");

                var bytes = ms.ToArray();
                Assert.AreEqual(7, bytes.Length, "Invalid number of bytes written");
                Assert.AreEqual((int)EncodingType.OCTET_STRING, bytes[0], "Wrong tag value");
                Assert.AreEqual(5, bytes[1], "Wrong length value");

                ms.Position = 0;

                var reader = new LDAPReader(ms);
                Assert.IsTrue(await reader.ReadAsync(), "Reader has no data");
                Assert.AreEqual((int)EncodingType.OCTET_STRING, reader.Tag, "Invalid tag");
                Assert.AreEqual(EncodingScope.UNIVERSAL, reader.Scope, "Invalid scope");
                Assert.AreEqual(5, reader.Length, "Invalid length");
                var value = await reader.ReadAsStringAsync();

                Assert.IsNotNull(value, "String should not be null");
                Assert.AreEqual("Hello", value, "Wrong value returned from reader");
            }
        }
    }
}