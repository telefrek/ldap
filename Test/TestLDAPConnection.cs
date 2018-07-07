using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.Security.LDAP.Test
{
    [TestClass]
    public class TestLDAPConnection
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task TestConnection()
        {
            try
            {
                var session = new LDAPSession(new LDAPOptions { Port = 10389, IsSecured = false, });
                await session.OpenAsync();

                Thread.Sleep(1000);

                await session.Close();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }
    }
}