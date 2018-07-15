using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.Encoding;

namespace Telefrek.LDAP.Protocol
{
    internal class LDAPAttribute
    {
        public string Description { get; set; }
        public string[] Values { get; set; }

        public async Task WriteContentsAsync(LDAPWriter writer)
        {
            var attWriter = new LDAPWriter();

            await attWriter.WriteAsync(Description);

            var valuesWriter = new LDAPWriter();
            foreach (var value in Values)
                await valuesWriter.WriteAsync(value);

            await attWriter.WriteAsync(valuesWriter, (int)EncodingType.SET);

            await writer.WriteAsync(attWriter);
        }

        public async Task ReadContentsAsync(LDAPReader reader)
        {
            // Read the description
            if (!await reader.ReadAsync())
                throw new LDAPProtocolException("Invalid attribute stream");
            await reader.GuardAsync((int)EncodingType.OCTET_STRING);
            Description = await reader.ReadAsStringAsync();

            // Read the values
            if (!await reader.ReadAsync())
                throw new LDAPProtocolException("Invalid attribute stream");
            await reader.GuardAsync((int)EncodingType.SET);

            var valReader = reader.CreateReader();
            var vals = new List<string>();
            while (await valReader.ReadAsync())
                vals.Add(await valReader.ReadAsStringAsync());
            Values = vals.ToArray();
        }
    }
}