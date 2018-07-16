using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telefrek.LDAP.Managers;

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
        public async Task TestUserManager()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                var mgr = new LDAPUserManager();
                var res = await mgr.FindUserAsync("admin", "example.org", session, CancellationToken.None);

                Assert.IsNotNull(res, "User shouldn't be null");
                Assert.AreEqual("admin", res.Name, true, "Invalid user name");
                Assert.AreEqual("example.org", res.Domain, true, "Invalid domain");

                await session.CloseAsync();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }

        [TestMethod]
        public async Task TestSearch()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                var result = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_OBJECTS, new CancellationTokenSource(5000).Token);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Expected successful search");

                var results = result.Objects.ToArray();
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(1, results.Length, "Expected 1 object to be returned");
                Assert.AreEqual("cn=admin,dc=example,dc=org", results[0].DistinguishedName, true, "mismatch DN");
                Assert.AreEqual(4, results[0].Attributes.Count, "Wrong number of attributes for default object");

                await session.CloseAsync();
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }

        [TestMethod]
        public async Task TestLifecycle()
        {
            try
            {
                var session = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session.StartAsync();

                var success = await session.TryLoginAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                var result = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_OBJECTS, new CancellationTokenSource(5000).Token);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Expected successful search");

                var results = result.Objects.ToArray();
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(1, results.Length, "Expected 1 object to be returned");
                Assert.AreEqual("cn=admin,dc=example,dc=org", results[0].DistinguishedName, true, "mismatch DN");
                Assert.AreEqual(4, results[0].Attributes.Count, "Wrong number of attributes for default object");

                // Clone the admin
                var newObj = results[0].Clone();
                newObj.DistinguishedName = "cn=test,dc=example,dc=org";

                foreach (var attr in newObj.Attributes)
                {
                    if (attr.Description.Equals("userPassword", StringComparison.InvariantCultureIgnoreCase))
                        attr.Values = new List<string>() { "testPassword" };
                    else if (attr.Description.Equals("cn", StringComparison.InvariantCultureIgnoreCase))
                        attr.Values = new List<string>() { "test" };
                    else if (attr.Description.Equals("description", StringComparison.InvariantCultureIgnoreCase))
                        attr.Values = new List<string>() { "Test User" };
                }

                result = await session.TryAdd(newObj, CancellationToken.None);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Failed to add the user");
                results = result.Objects.ToArray();
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(1, results.Length, "Expected 1 object to be returned");

                var session2 = new LDAPSession(new TestOptions { Port = 10389, IsSecured = false, });
                await session2.StartAsync();
                var testLogin = await session2.TryLoginAsync(newObj.DistinguishedName, "testPassword", CancellationToken.None);
                await session2.CloseAsync();

                result = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_OBJECTS, new CancellationTokenSource(5000).Token);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Expected successful search");

                results = result.Objects.ToArray();
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(2, results.Length, "Expected 2 objects to be returned");
                
                success = await session.TryRemove("cn=test,dc=example,dc=org", CancellationToken.None);
                Assert.IsTrue(success, "Failed to remove the user");

                await session.CloseAsync();

                Assert.IsTrue(testLogin, "Failed to login with test credentials");
            }
            catch (LDAPException ldapEx)
            {
                TestContext.WriteLine("Invalid exception : {0}", ldapEx);
                Assert.Fail("Unhandled exception");
            }
        }

        [TestMethod]
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