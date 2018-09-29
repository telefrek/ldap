using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telefrek.LDAP.Managers;

namespace Telefrek.LDAP.Test
{
    [TestClass]
    public class TestLDAPConnection
    {
        protected IConfiguration Configuration { get; private set; }

        public TestLDAPConnection()
        {

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = configBuilder.Build();
        }
        public TestContext TestContext { get; set; }
        protected const string SERVICE_PROVIDER_KEY = "service.provider";

        protected ServiceProvider ServiceProvider { get => (ServiceProvider)TestContext.Properties[SERVICE_PROVIDER_KEY]; }

        [TestInitialize]
        public void TestInitialize()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILoggerFactory>(new LoggerFactory().AddConsole());
            services.AddLDAPAuth(Configuration);
            services.AddLogging();

            services.AddTransient<ILDAPSession, LDAPSession>();
            services.AddTransient<ILDAPSchemaManager, LDAPSchemaManager>();
            services.AddTransient<ILDAPUserManager, LDAPUserManager>();

            var serviceProvider = services.BuildServiceProvider();
            TestContext.Properties.Add(SERVICE_PROVIDER_KEY, serviceProvider);
        }

        [TestMethod]
        public async Task TestUserManager()
        {
            try
            {
                using (var mgr = ServiceProvider.GetService<ILDAPUserManager>())
                {
                    var cts = new CancellationTokenSource(3000);
                    var res = await mgr.FindUserAsync("admin", "example.org", cts.Token);

                    Assert.IsNotNull(res, "User shouldn't be null");
                    Assert.AreEqual("admin", res.Name, true, "Invalid user name");
                    Assert.AreEqual("example.org", res.Domain, true, "Invalid domain");
                }
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
                var session = ServiceProvider.GetService<ILDAPSession>();
                await session.StartAsync();

                var success = await session.TryBindAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                var result = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_CN, new CancellationTokenSource(15000).Token);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Expected successful search");

                var results = result.Objects.ToArray();
                foreach (var obj in result.Objects)
                {
                    TestContext.WriteLine("DN: {0}", obj.DistinguishedName);
                    foreach (var attr in obj.Attributes)
                    {
                        TestContext.WriteLine("{0} : {1}", attr.Description, string.Join(';', attr.Values.ToArray()));
                    }
                }
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(1, results.Length, "Expected 1 objects to be returned");

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
                var session = ServiceProvider.GetService<ILDAPSession>();
                await session.StartAsync();

                var success = await session.TryBindAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                var result = await session.TrySearch("cn=admin,dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_CN, new CancellationTokenSource(15000).Token);
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

                var session2 = ServiceProvider.GetService<ILDAPSession>();
                await session2.StartAsync();
                var testLogin = await session2.TryBindAsync(newObj.DistinguishedName, "testPassword", CancellationToken.None);
                await session2.CloseAsync();

                result = await session.TrySearch("dc=example,dc=org", LDAPScope.EntireSubtree, LDAPAliasDereferencing.Always, LDAPFilter.ALL_CN, new CancellationTokenSource(15000).Token);
                Assert.AreEqual(LDAPResultCode.Success, result.ResultCode, "Expected successful search");

                results = result.Objects.ToArray();
                Assert.IsNotNull(results, "Expected object to be returned");
                Assert.AreEqual(2, results.Length, "Expected 2 objects to be returned");

                result = await session.TryRemove(newObj, CancellationToken.None);
                Assert.IsTrue(result.WasSuccessful, "Failed to remove the user");

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
                var session = ServiceProvider.GetService<ILDAPSession>();
                await session.StartAsync();

                var success = await session.TryBindAsync("", "", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as anonymous");

                success = await session.TryBindAsync("cn=admin,dc=example,dc=org", "admin", CancellationToken.None);
                Assert.IsTrue(success, "Failed to login as admin");

                success = await session.TryBindAsync("cn=test,dc=example,dc=org", "password", CancellationToken.None);
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