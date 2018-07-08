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
        [Timeout(5000)]
        public async Task TestConnection()
        {
            try
            {
                var session = new LDAPSession(new LDAPOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin");

                await session.CloseAsync();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestSSLConnection()
        {
            try
            {
                var session = new LDAPSession(new LDAPOptions { Port = 10636, IsSecured = true, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin");

                await session.CloseAsync();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }
    }
}