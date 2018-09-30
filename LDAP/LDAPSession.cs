using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        LDAPObject _current;
        LDAPSessionState _state;
        ILogger<LDAPSession> _log;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="options">Options for LDAP communication</param>
        /// <param name="log"></param>
        public LDAPSession(IOptions<LDAPConfiguration> options, ILogger<LDAPSession> log)
        {
            // Sessions start closed
            _state = LDAPSessionState.Closed;

            _options = options.Value;
            _connection = new LDAPConnection(_options.IsSecured, log);
            _log = log;
        }

        /// <summary>
        /// Gets the current ssession state
        /// </summary>
        public LDAPSessionState State => _state;

        /// <summary>
        /// Gets the currently bound object scope
        /// </summary>
        public LDAPObject CurrentScope => _current;

        /// <summary>
        /// Starts the session asynchronously
        /// </summary>
        public async Task StartAsync()
        {
            // Only start the connection if not initialized
            if (_state == LDAPSessionState.Closed || _connection.State == LDAPConnectionState.NotInitialized)
                await _connection.ConnectAsync(_options.Host, _options.Port);

            _state = LDAPSessionState.Open;
        }

        /// <summary>
        /// Attempts a login with the given credentials
        /// </summary>
        /// <param name="domainUser">The domain qualified user</param>
        /// <param name="credentials">The associated credentials</param>
        /// <param name="token"></param>
        /// <returns>True if the login was successful</returns>
        public async Task<bool> TryBindAsync(string domainUser, string credentials, CancellationToken token)
        {
            var op = new BindRequest { Name = domainUser, Authentication = new SimpleAuthentication { Credentials = credentials } };
            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                var res = msg as BindResponse;
                if (res != null && res.ResultCode == 0)
                {
                    _state = LDAPSessionState.Bound;
                    var dn = res.MatchedDN;
                    _current = new LDAPObject
                    {
                        DistinguishedName = dn,
                        Domain = string.Join(",", dn.Split(',')
                            .Where(s => s.StartsWith("dc=", true, CultureInfo.InvariantCulture)).ToArray())
                    };
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<LDAPResult> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, LDAPFilter filter, CancellationToken token) =>
            await TrySearch(dn, scope, aliasing, filter, null, token);

        /// <summary>
        /// Stub for now
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="scope"></param>
        /// <param name="aliasing"></param>
        /// <param name="filter"></param>
        /// <param name="attributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<LDAPResult> TrySearch(string dn, LDAPScope scope, LDAPAliasDereferencing aliasing, LDAPFilter filter, string[] attributes, CancellationToken token)
        {
            _log.LogInformation("Searching for {0}", dn);
            var op = new SearchRequest { DistinguishedName = dn, Scope = scope, Aliasing = aliasing, Filter = filter, Attributes = attributes };
            var objList = new List<LDAPObject>();
            var result = new LDAPResult
            {
                Objects = objList,
                IsStreaming = false,
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                _log.LogInformation("Received {0}", msg.Operation);
                switch (msg.Operation)
                {
                    case ProtocolOp.SEARCH_RESPONSE:
                        var sResponse = msg as SearchResponse;
                        _log.LogInformation("Found {0}", sResponse.DistinguishedName);
                        if (!string.IsNullOrWhiteSpace(sResponse.DistinguishedName))
                            objList.Add(new LDAPObject { DistinguishedName = sResponse.DistinguishedName, Attributes = new List<LDAPAttribute>(sResponse.Attributes) });
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
        /// <param name="obj">The LDAP entity to add</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        public async Task<LDAPResult> TryAdd(LDAPObject obj, CancellationToken token)
        {
            var op = new AddRequest { DistinguishedName = obj.DistinguishedName, Attributes = obj.Attributes.ToArray() };
            var objList = new List<LDAPObject>();
            var result = new LDAPResult
            {
                Objects = objList,
                IsStreaming = false,
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                result.ResultCode = (LDAPResultCode)msg.ResultCode;
                result.WasSuccessful = msg.ResultCode == 0;
                objList.Add(obj);
                break;
            }

            return result;
        }

        /// <summary>
        /// Try to add a record to the directory
        /// </summary>
        /// <param name="obj">The LDAP Entity to add</param>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        /// <param name="update"></param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        public async Task<LDAPResult> TryModify(LDAPObject obj, ICollection<LDAPAttribute> add, ICollection<LDAPAttribute> remove, ICollection<LDAPAttribute> update, CancellationToken token)
        {
            var op = new ModifyRequest { DistinguishedName = obj.DistinguishedName };

            if (add != null && add.Count > 0)
                op.Added = add.ToArray();

            if (remove != null && remove.Count > 0)
                op.Removed = remove.ToArray();

            if (update != null && update.Count > 0)
                op.Modified = update.ToArray();

            var objList = new List<LDAPObject>();
            var result = new LDAPResult
            {
                Objects = objList,
                IsStreaming = false,
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                result.ResultCode = (LDAPResultCode)msg.ResultCode;
                result.WasSuccessful = msg.ResultCode == 0;

                // Modify the attributes
                if (result.WasSuccessful)
                {
                    foreach (var attr in add ?? new List<LDAPAttribute>())
                        obj.Attributes.Add(attr);

                    foreach (var attr in update ?? new List<LDAPAttribute>())
                    {
                        obj.Attributes.RemoveAll((p) => p.Description.Equals(attr.Description));
                        obj.Attributes.Add(attr);
                    }

                    foreach (var attr in remove ?? new List<LDAPAttribute>())
                        obj.Attributes.RemoveAll((p) => p.Description.Equals(attr.Description));
                }

                objList.Add(obj);
                break;
            }

            return result;
        }


        /// <summary>
        /// Try to remove a record from the directory
        /// </summary>
        /// <param name="obj">The entity to remove</param>
        /// <param name="token"></param>
        /// <returns>True if successful</returns>
        public async Task<LDAPResult> TryRemove(LDAPObject obj, CancellationToken token)
        {
            var op = new DeleteRequest { DistinguishedName = obj.DistinguishedName };
            var objList = new List<LDAPObject>();
            var result = new LDAPResult
            {
                Objects = objList,
                IsStreaming = false,
            };

            foreach (var msg in await _connection.TryQueueOperation(op, token))
            {
                result.ResultCode = (LDAPResultCode)msg.ResultCode;
                result.WasSuccessful = msg.ResultCode == 0;
                objList.Add(obj);
                break;
            }

            return result;
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

            _state = LDAPSessionState.Closed;
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