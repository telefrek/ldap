using System;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{
    internal class BindResponse : ProtocolOperation
    {
        public override ProtocolOp Operation => ProtocolOp.BIND_RESPONSE;
        public int ResultCode { get; set; }
        public string MatchedDN { get; set; }
        public string DiagnosticMessage { get; set; }

        protected override async Task ReadContentsAsync(LDAPReader reader)
        {
            // Validate the state of the reader
            if (reader.Tag != 1 || reader.Scope != EncodingScope.APPLICATION)
                throw new LDAPProtocolException("Invalid cast to a bind response");

            var contentReader = reader.CreateReader();
            await contentReader.GuardAsync((int)EncodingType.ENUMERATED);

            ResultCode = await contentReader.ReadAsIntAsync();

            await contentReader.GuardAsync((int)EncodingType.OCTET_STRING);
            MatchedDN = await contentReader.ReadAsStringAsync();

            await contentReader.GuardAsync((int)EncodingType.OCTET_STRING);
            DiagnosticMessage = await contentReader.ReadAsStringAsync();
        }

        protected override Task WriteContentsAsync(LDAPWriter target) =>
            throw new InvalidOperationException("Cannot write a response");
    }
}