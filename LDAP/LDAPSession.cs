using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telefrek.LDAP.Protocol;
using Telefrek.LDAP.Protocol.IO;

namespace Telefrek.LDAP
{
    /// <summary>
    /// Represents an LDAP session and is the main entrypoint for the library
    /// </summary>
    public sealed class LDAPSession : ILDAPSession
    {
        ILDAPConnection _connection;
        LDAPConfiguration _options;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Options for LDAP communication</param>
        public LDAPSession(IOptions<LDAPConfiguration> options)
        {
            _options = options.Value;
            _connection = new LDAPConnection(_options.IsSecured);
        }

        /// <summary>
        /// Starts the session asynchronously
        /// </summary>
        public async Task StartAsync() => await _connection.ConnectAsync(_options.Host, _options.Port);

        /// <summary>
        /// Attempts a login with the given credentials
        /// </summary>
        /// <param name="domainUser">The domain qualified user</param>
        /// <param name="credentials">The associated credentials</param>
        /// <param name="token"></param>
        /// <returns>True if the login was successful</returns>
        public async Task<bool> TryLoginAsync(string domainUser, string credentials, CancellationToken token)
        {
            var op = new BindRequest { Name = domainUser, Authentication = new SimpleAuthentication { Credentials = credentials } };
            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as LDAPResponse;
                if (res != null) return res.ResultCode == 0;
            }

            return false;
        }

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<LDAPResult> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, CancellationToken token)
        {
            var op = new SearchRequest { DistinguishedName = dn, Scope = scope, Aliasing = aliasing };
            var objList = new List<LDAPObject>();
            var result = new LDAPResult
            {
                Objects = objList,
                IsStreaming = false,
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                switch (msg.Operation)
                {
                    case ProtocolOp.SEARCH_RESPONSE:
                        var sResponse = msg as SearchResponse;
                        if (!string.IsNullOrWhiteSpace(sResponse.DistinguishedName))
                            objList.Add(new LDAPObject { DistinguishedName = sResponse.DistinguishedName });
                        break;
                    case ProtocolOp.SEARCH_RESULT:
                        var sResult = msg as SearchResult;
                        result.ResultCode = (LDAPResultCode)sResult.ResultCode;
                        result.WasSuccessful = sResult.ResultCode == 0;
                        break;
                        
                }
            }

            return result;
        }

        /// <summary>
        /// Try to add a record to the directory
        /// </summary>
        /// <param name="dn">The directory name</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        public async Task<bool> TryAdd(string dn, CancellationToken token)
        {
            var op = new AddRequest { DistinguishedName = dn };
            op.Attributes = new LDAPAttribute[]
            {
                new LDAPAttribute
                {
                    Description = "objectClass",
                    Values = new string[] { "top", "person" }
                },
                new LDAPAttribute
                {
                    Description = "sn",
                    Values = new string[] { "test" }
                }
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as LDAPResponse;
                if (res != null) return res.ResultCode == 0;
            }

            return false;
        }

        /// <summary>
        /// Try to remove a record from the directory
        /// </summary>
        /// <param name="dn">The directory name</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        public async Task<bool> TryRemove(string dn, CancellationToken token)
        {
            var op = new DeleteRequest { DistinguishedName = dn };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as LDAPResponse;
                if (res != null) return res.ResultCode == 0;
            }

            return false;
        }


        /// <summary>
        /// Ends the session and closes all resources asynchronously
        /// </summary>
        /// <remarks>
        /// Note that any outstanding requests will be abandoned and may not finish appropriately
        /// and certain states will require this operation to wait for completion before stopping
        /// </remarks>
        public async Task CloseAsync()
        {
            await _connection.CloseAsync();
            Dispose();
        }

        /// <summary>
        /// Dispose of the object resources
        /// </summary>
        public void Dispose() => Dispose(true);

        bool _isDisposed = false;

        void Dispose(bool disposing)
        {
            // Check state flags
            if (disposing && !_isDisposed)
            {
                // Ensure we cleanup connection resources
                if (_connection != null)
                    _connection.Dispose();

                _connection = null;

                // Notify GC to ignore
                GC.SuppressFinalize(this);
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Destructor for releasing resources via GC callbacks
        /// </summary>
        /// <returns></returns>
        ~LDAPSession() => Dispose(false);
    }
}