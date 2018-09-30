using System;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal abstract class LDAPResponse
    {
        public int ResultCode { get; set; }
        public string MatchedDN { get; set; }
        public int MessageId { get; set; }
        public string DiagnosticMessage { get; set; }
        public abstract ProtocolOp Operation { get; }
        public virtual bool IsTerminating => true;

        public virtual async Task ReadContentsAsync(LDAPReader reader)
        {
            // Validate the state of the reader
            if (reader.Tag != (int)Operation || reader.Scope != EncodingScope.APPLICATION)
                throw new LDAPProtocolException("Invalid cast to response");
            var contentReader = reader.CreateReader();
            await contentReader.GuardAsync((int)EncodingType.ENUMERATED);

            ResultCode = await contentReader.ReadAsIntAsync();

            await contentReader.GuardAsync((int)EncodingType.OCTET_STRING);
            MatchedDN = await contentReader.ReadAsStringAsync();

            await contentReader.GuardAsync((int)EncodingType.OCTET_STRING);
            DiagnosticMessage = await contentReader.ReadAsStringAsync();

            // check for more data
            if (await contentReader.ReadAsync())
            {
                // May be optional referral or additional data from request
                if (contentReader.Tag == 3 && contentReader.Scope == EncodingScope.CONTEXT_SPECIFIC)
                {
                    // Read the referral
                    await contentReader.SkipAsync();
                    if (await contentReader.ReadAsync())
                        await ReadResponseAsync(contentReader);
                }
                else
                {
                    await ReadResponseAsync(contentReader);
                }
            }
        }

        protected virtual Task ReadResponseAsync(LDAPReader reader) => Task.CompletedTask;
    }
}