using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol
{

    /// <summary>
    /// Encoding  for LDAP customized BER
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/X.690#BER_encoding
    /// https://tools.ietf.org/rfc/rfc4511.txt (section 5.1)
    /// https://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf
    /// </remarks>
    public static class ProtocolEncoding
    {
        public static async Task Validate(Stream source)
        {
            int tag, scope;
            bool isPrimitive;

            while (source.Position < source.Length)
            {
                GetDetails(source, out tag, out scope, out isPrimitive);

                Console.WriteLine("scope: {0}, tag: {1}, prim: {2}", scope, tag, isPrimitive);
                var len = await ReadLengthAsync(source);
                Console.WriteLine("len: {0}", len);
                source.Seek(len, SeekOrigin.Current);
            }


        }

        #region Read Methods
        public static async Task<string> ReadStringAsync(Stream source)
        {
            EncodingScope scope;
            EncodingType encoding;

            // Check and validate the stream
            GetDetails(source, out encoding, out scope);
            if (encoding == EncodingType.NULL)
            {
                // Validate the length
                if (source.ReadByte() != 0)
                    throw new LDAPProtocolException("Corrupted stream detected");

                return null;
            }

            encoding.Guard(EncodingType.OCTET_STRING);
            var len = await ReadLengthAsync(source);

            if (len == 0)
                throw new LDAPProtocolException("Length was 0 for non-null string");

            var buffer = new byte[len];
            await source.ReadAsync(buffer, 0, len);

            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<int> ReadIntAsync(Stream source)
        {
            EncodingScope scope;
            EncodingType encoding;

            // Check and validate the stream
            GetDetails(source, out encoding, out scope);
            encoding.Guard(EncodingType.INTEGER);
            var len = await ReadLengthAsync(source);

            if (len == 0)
                throw new LDAPProtocolException("Length was 0 for a numeric value");

            var buffer = new byte[len];
            await source.ReadAsync(buffer, 0, len);

            var val = 0;
            for (var i = 0; i < len; ++i)
                val = (val << 8) | buffer[i];

            return val;
        }

        public static Task<bool> ReadBoolASync(Stream source)
        {
            EncodingScope scope;
            EncodingType encoding;

            // Check and validate the stream
            GetDetails(source, out encoding, out scope);
            encoding.Guard(EncodingType.BOOLEAN);

            // validate the length
            if (source.ReadByte() != 1)
                throw new LDAPProtocolException("Boolean value had length != 1");

            // Any non-zero is true per spec, though we use 0xFF explictly in writing
            return Task.FromResult(source.ReadByte() > 0x0);
        }

        public static Task ReadNullAsync(Stream source)
        {
            EncodingScope scope;
            EncodingType encoding;

            // Check and validate the stream
            GetDetails(source, out encoding, out scope);
            encoding.Guard(EncodingType.NULL);

            // validate the null length
            if (source.ReadByte() != 0)
                throw new LDAPProtocolException("Null value read with non-zero length");

            return Task.CompletedTask;
        }

        public static async Task<Stream> ReadStreamAsync(Stream source)
        {
            EncodingScope scope;
            EncodingType encoding;

            // Check and validate the stream
            GetDetails(source, out encoding, out scope);
            encoding.Guard(EncodingType.SEQUENCE);

            var rem = await ReadLengthAsync(source);

            if (rem == 0)
                return new MemoryStream();

            // Create a new byte backed stream for this
            var buf = new byte[rem];

            var numRead = 0;
            while (numRead < rem)
            {
                var n = await source.ReadAsync(buf, numRead, rem - numRead);
                if (n < 0)
                    throw new LDAPProtocolException("Stream closed before read completed");
                numRead += n;
            }

            // Return a read only stream
            return new MemoryStream(buf, false);
        }
        #endregion

        #region Write Methods
        public static async Task WriteNullAsync(Stream target, EncodingScope scope = EncodingScope.UNIVERSAL) => await WriteNullAsync(target, (int)EncodingType.NULL, (int)scope);

        public static async Task WriteNullAsync(Stream target, int tag, int scope)
        {
            await encodeTagAsync(target, tag, scope);
            await encodeLengthAsync(target, 0);
        }

        public static async Task WriteAsync(Stream target, Stream source, EncodingType encoding = EncodingType.SEQUENCE,
            EncodingScope scope = EncodingScope.UNIVERSAL) => await WriteAsync(target, source, (int)encoding, (int)scope);

        public static async Task WriteAsync(Stream target, Stream source, int tag, int scope)
        {
            await encodeTagAsync(target, tag, scope, false);
            await encodeLengthAsync(target, (int)source.Length);
            await source.CopyToAsync(target);
        }

        public static async Task WriteAsync(Stream target, bool value, EncodingScope scope = EncodingScope.UNIVERSAL) => await WriteAsync(target, value, (int)EncodingType.BOOLEAN, (int)scope);

        public static async Task WriteAsync(Stream target, bool value, int tag, int scope)
        {
            await encodeTagAsync(target, tag, scope);
            await target.WriteAsync(new byte[] { 0x1, (byte)(value ? 0xFF : 0x0) }, 0, 2);

        }

        public static async Task WriteAsync(Stream target, string value, EncodingType encoding = EncodingType.OCTET_STRING, EncodingScope scope = EncodingScope.UNIVERSAL) => await WriteAsync(target, value, (int)encoding, (int)scope);

        public static async Task WriteAsync(Stream target, string value, int tag, int scope)
        {
            if (string.IsNullOrEmpty(value))
            {
                await WriteNullAsync(target, tag, scope);
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(value);

            // Encode away!
            await encodeTagAsync(target, tag, scope, true);
            await encodeLengthAsync(target, buffer.Length);
            await target.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task WriteAsync(Stream target, int value, EncodingScope scope = EncodingScope.UNIVERSAL) => await WriteAsync(target, value, (int)EncodingType.INTEGER, (int)scope);

        public static async Task WriteAsync(Stream target, int value, int tag, int scope)
        {

            // Write the tag
            await encodeTagAsync(target, tag, scope);

            // Calculate the bits required to write this value
            var bits = value.MSB() + 1;
            var len = bits >> 3;
            var rem = bits & 0x7;

            if (rem > 0)
                len++;

            // Write the length
            await encodeLengthAsync(target, len);

            // Write the value
            if (len == 0)
                target.WriteByte((byte)value);
            else
            {
                // Write the value
                var n = bits >> 3;
                var idx = 0;

                // 4 bytes for an integer max
                var contents = new byte[4];
                var rShift = bits - rem;

                // Handle overflow
                if (rem > 0)
                    contents[idx++] = (byte)((value >> rShift) & 0xFF);

                // Write remainder
                for (var i = n - 1; i > 0; --i)
                    contents[idx++] = (byte)((value >> ((rShift -= 8))) & 0xFF);

                // Write the last bits
                if (bits > 7)
                    contents[idx++] = (byte)(value & 0xFF);

                // Write the portion of the buffer used
                await target.WriteAsync(contents, 0, idx);
            }
        }
        #endregion

        #region Internal methods
        static void GetDetails(Stream source, out EncodingType encoding, out EncodingScope scope)
        {
            var b = source.ReadByte();
            scope = (EncodingScope)(b >> 6);

            // Check for overflow
            if ((b & 0x1F) == (0x1F))
            {
                var t = 0;
                for (var i = 0; i < 8; ++i)
                {
                    b = source.ReadByte();
                    t = (t << 7) | (b & 0x7F);

                    // Check if we need to read more
                    if ((b & 0x80) == 0)
                        break;
                }
                encoding = (EncodingType)t;
            }
            else
                encoding = (EncodingType)(b & 0x1F);
        }

        static void GetDetails(Stream source, out int tag, out int scope, out bool isPrimitive)
        {
            var b = source.ReadByte();
            scope = b >> 6;

            isPrimitive = (b & 0x20) == 0;

            // Check for overflow
            if ((b & 0x1F) == (0x1F))
            {
                tag = 0;
                for (var i = 0; i < 8; ++i)
                {
                    b = source.ReadByte();
                    tag = (tag << 7) | (b & 0x7F);

                    // Check if we need to read more
                    if ((b & 0x80) == 0)
                        break;
                }
            }
            else
                tag = b & 0x1F;
        }

        static async Task<int> ReadLengthAsync(Stream source)
        {
            // Detect if we have a small byte
            var b = source.ReadByte();

            if ((b & 0x80) == 0)
                return b;

            var numBytes = b & 0x7F;
            var contents = new byte[numBytes];
            var idx = 0;

            // Read from the stream
            do
            {
                var numRead = await source.ReadAsync(contents, idx, numBytes);
                if (numRead == -1)
                    throw new LDAPProtocolException("Stream disconnected before read finished");
                numBytes -= numRead;
                idx += numRead;
            } while (numBytes > 0);

            var len = 0;
            for (var i = 0; i < idx; ++i)
                len = (len << 8) | contents[i];

            return len;
        }

        static async Task encodeLengthAsync(Stream target, int length)
        {
            if (length < 128)
                target.WriteByte((byte)length);
            else
            {
                var bits = length.MSB() + 1;
                var n = bits >> 3;
                var idx = 1; // save byte 0 for length encoding

                var contents = new byte[8];

                // Handle overflow
                var rem = bits & 0x7;
                var rShift = bits - rem;
                if (rem > 0)
                    contents[idx++] = (byte)((length >> (rShift)) & 0xFF);

                // Write remainder
                for (var i = n - 1; i > 0; --i)
                    contents[idx++] = (byte)((length >> (rShift -= 8)) & 0xFF);

                // Write the last bits
                if (bits > 7)
                    contents[idx++] = (byte)(length & 0xFF);

                // Write the number of octets for the length
                contents[0] = (byte)(0x80 | (idx - 1));

                // Write the portion of the buffer used
                await target.WriteAsync(contents, 0, idx);
            }
        }

        static async Task encodeTagAsync(Stream target, int tag, int scope, bool isPrimitive = true)
        {
            // Create the tag header
            var header = scope << 6;
            header |= (isPrimitive ? 0 : 1) << 5;

            // Safe to write entire tag
            if (tag < 31)
                target.WriteByte((byte)(header | tag));
            else
            {
                // Check the number of bits used to represent the number
                var bits = tag.MSB() + 1;

                // Can't go over 6 bytes but 8 feels better for alignment
                var contents = new byte[8];
                var idx = 0;

                // Header + indicator
                contents[idx++] = (byte)(header | 0x1F);

                // Get the number of bits to the left remaining
                var rem = bits % 7;
                var rShift = bits - rem;
                if (rem > 0)
                    contents[idx++] = (byte)(0x80 | (((tag >> rShift) & 0x7F)));

                // Write middle bytes
                for (var n = bits / 7 - 1; n > 0; --n)
                    contents[idx++] = (byte)(0x80 | (((tag >> (rShift -= 7)) & 0x7F)));

                // Write any remaining bits
                if (bits > 6)
                    contents[idx++] = (byte)(tag & 0x7F);

                // Write the portion of the buffer used
                await target.WriteAsync(contents, 0, idx);
            }
        }
        #endregion

        #region MSB/LSB (Most/Least Significant Bit/Log Base 2)

        static int[] MSBDeBruijnLookup = new int[]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        /// <summary>
        /// Uses a DeBruijn Lookup to calculate the MSB.
        /// </summary>
        /// <param name="x">The value to calculate the MSB for.</param>
        /// <returns>The position of the highest set bit.</returns>
        public static int MSB(this int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return MSBDeBruijnLookup[(uint)(x * 0x07C4ACDDU) >> 27];
        }
        #endregion

        /// <summary>
        /// Extension method to ensure token types match
        /// </summary>
        /// <param name="typeToken">The current token</param>
        /// <param name="expected">The required type</param>
        static void Guard(this EncodingType typeToken, EncodingType expected)
        {
            if (typeToken != expected)
                throw new LDAPProtocolException("Invalid token in stream");
        }
    }
}