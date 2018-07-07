using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Telefrek.Security.LDAP.Protocol.BER
{

    /// <summary>
    /// Encoding that reads/writes BER objects
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/X.690#BER_encoding
    /// https://tools.ietf.org/rfc/rfc4511.txt (section 5.1)
    /// </remarks>
    public static class BEREncoding
    {
        public static async Task<string> ReadStringAsync(Stream source)
        {
            BERClass bCls;
            BERType bType;

            // Check and validate the stream
            GetDetails(source, out bType, out bCls);
            if (bType == BERType.NULL)
            {
                // Validate the length
                if (source.ReadByte() != 0)
                    throw new LDAPException("Corrupted stream detected");

                return null;
            }

            bType.Guard(BERType.OCTET_STRING);
            var len = await getLengthAsync(source);

            if (len == 0)
                throw new LDAPException("Length was 0 for non-null string");

            var buffer = new byte[len];
            await source.ReadAsync(buffer, 0, len);

            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task<int> ReadIntAsync(Stream source)
        {
            BERClass bCls;
            BERType bType;

            // Check and validate the stream
            GetDetails(source, out bType, out bCls);
            bType.Guard(BERType.INTEGER);
            var len = await getLengthAsync(source);

            if (len == 0)
                throw new LDAPException("Length was 0 for a numeric value");

            var buffer = new byte[len];
            await source.ReadAsync(buffer, 0, len);

            var val = 0;
            for (var i = 0; i < len; ++i)
                val = (val << 8) | buffer[i];

            return val;
        }

        public static Task<bool> ReadBoolASync(Stream source)
        {
            BERClass bCls;
            BERType bType;

            // Check and validate the stream
            GetDetails(source, out bType, out bCls);
            bType.Guard(BERType.BOOLEAN);

            // validate the length
            if (source.ReadByte() != 1)
                throw new LDAPException("Boolean value had length != 1");

            // Read for the true flag
            return Task.FromResult(source.ReadByte() == 0xFF);
        }

        public static Task ReadNullAsync(Stream source)
        {
            BERClass bCls;
            BERType bType;

            // Check and validate the stream
            GetDetails(source, out bType, out bCls);
            bType.Guard(BERType.NULL);

            // validate the null length
            if (source.ReadByte() != 0)
                throw new LDAPException("Null value read with non-zero length");

            return Task.CompletedTask;
        }

        public static async Task<Stream> ReadStreamAsync(Stream source)
        {
            BERClass bCls;
            BERType bType;

            // Check and validate the stream
            GetDetails(source, out bType, out bCls);
            bType.Guard(BERType.SEQUENCE);

            var rem = await getLengthAsync(source);

            if (rem == 0)
                return new MemoryStream();

            // Create a new byte backed stream for this
            var buf = new byte[rem];

            var numRead = 0;
            while (numRead < rem)
            {
                var n = await source.ReadAsync(buf, numRead, rem - numRead);
                if (n < 0)
                    throw new LDAPException("Stream closed before read completed");
                numRead += n;
            }

            // Return a read only stream
            return new MemoryStream(buf, false);
        }

        public static async Task WriteNullAsync(Stream target, BERClass bCls = BERClass.UNIVERSAL)
        {
            await encodeTagAsync(target, BERType.NULL, true, bCls);
            target.WriteByte(0x0);
        }

        public static async Task WriteAsync(Stream target, Stream source, BERClass bCls = BERClass.UNIVERSAL)
        {
            await encodeTagAsync(target, BERType.SEQUENCE, false, bCls);
            await encodeLengthAsync(target, (int)source.Length);
            await source.CopyToAsync(target);
        }

        public static async Task WriteAsync(Stream target, bool value, BERClass bCls = BERClass.UNIVERSAL)
        {
            await encodeTagAsync(target, BERType.BOOLEAN, true, bCls);
            await target.WriteAsync(new byte[] { 0x1, (byte)(value ? 0xFF : 0x0) }, 0, 2);
        }

        public static async Task WriteAsync(Stream target, string value, BERClass bCls = BERClass.UNIVERSAL)
        {
            if (string.IsNullOrEmpty(value))
            {
                await WriteNullAsync(target);
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(value);

            // Encode away!
            await encodeTagAsync(target, BERType.OCTET_STRING, true, bCls);
            await encodeLengthAsync(target, buffer.Length);
            await target.WriteAsync(buffer, 0, buffer.Length);
        }

        public static async Task WriteAsync(Stream target, int value, BERClass bCls = BERClass.UNIVERSAL)
        {
            // Write the tag
            await encodeTagAsync(target, BERType.INTEGER, true, bCls);

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

        static void GetDetails(Stream source, out BERType bType, out BERClass bCls)
        {
            var b = source.ReadByte();
            bCls = (BERClass)(b >> 6);

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
                bType = (BERType)t;
            }
            else
                bType = (BERType)(b & 0x1F);
        }

        static async Task<int> getLengthAsync(Stream source)
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
                    throw new LDAPException("Stream disconnected before read finished");
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

        static async Task encodeTagAsync(Stream target, BERType bType, bool isPrimitive = true, BERClass bClass = BERClass.UNIVERSAL)
        {
            // Create the tag header
            var header = ((int)bClass << 6);
            header |= (isPrimitive ? 0 : 1) << 5;
            var tag = (int)bType;

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

        static void Guard(this BERType typeToken, BERType expected)
        {
            if (typeToken != expected)
                throw new LDAPException("Invalid token in stream");
        }
    }
}