using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Telefrek.LDAP.Test
{
    [TestClass]
    public class TestLDAPConnection
    {
        class TestOptions : LDAPConfiguration, IOptions<LDAPConfiguration>
        {
            public LDAPConfiguration Value => this;
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        [Timeout(5000)]
        public async Task TestSearch()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                success = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, new CancellationTokenSource(5000).Token);
                await session.CloseAsync();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }


        [TestMethod]
        //[Timeout(5000)]
        public async Task TestLifecycle()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                success = await session.TryAdd("cn=test,dc=example,dc=org", CancellationToken.None);
                Assert.IsTrue(success, "Failed to add the user");


                success = await session.TryRemove("cn=test,dc=example,dc=org", CancellationToken.None);
                Assert.IsTrue(success, "Failed to remove the user");

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
        public async Task TestConnection()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("", "", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as anonymous");

                success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                success = await session.TryLoginAsync("cn=test,dc=example,dc=org", "password", CancellationToken.None);
                Assert.IsFalse(success, "User login should have failed");

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
                var session = new LDAPSession(new TestOptions { Port = 10636, IsSecured = true, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("", "", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as anonymous");

                success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                success = await session.TryLoginAsync("cn=test,dc=example,dc=org", "password", CancellationToken.None);
                Assert.IsFalse(success, "User login should have failed");

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