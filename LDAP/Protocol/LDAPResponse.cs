using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal abstract class LDAPResponse : ProtocolOperation
    {
        public int ResultCode { get; set; }
        public string MatchedDN { get; set; }
        public string DiagnosticMessage { get; set; }

        protected sealed override async Task ReadContentsAsync(LDAPReader reader)
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

        protected abstract Task ReadResponseAsync(LDAPReader reader);

        protected override Task WriteContentsAsync(LDAPWriter writer) => throw new InvalidOperationException("Cannot write a response");
    }
}