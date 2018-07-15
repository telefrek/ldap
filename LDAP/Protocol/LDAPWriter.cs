using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Telefrek.LDAP.Protocol
{
    /// <summary>
    /// Writes objects to the underlying stream using the LDAP protocol
    /// </summary>
    public class LDAPWriter
    {
        Stream _target;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LDAPWriter() : this(new MemoryStream())
        {

        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="target">The stream to write to</param>
        public LDAPWriter(Stream target)
        {
            _target = target;
        }

        /// <summary>
        /// Writes a null value to the stream
        /// </summary>
        public async Task WriteNullAsync() => await WriteNullAsync((int)EncodingType.NULL);

        /// <summary>
        /// Writes a null value to the stream
        /// </summary>
        /// <param name="tag">The tag for the value</param>
        public async Task WriteNullAsync(int tag) => await WriteNullAsync(tag, EncodingScope.UNIVERSAL);

        /// <summary>
        /// Writes a null value to the stream
        /// </summary>
        /// <param name="tag">The tag to use</param>
        /// <param name="scope">The scope for the value</param>
        public async Task WriteNullAsync(int tag, EncodingScope scope)
        {
            await EncodeHeaderAsync(tag, scope, true);
            await EncodeLengthAsync(0);
        }

        /// <summary>
        /// Writes a string value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        public async Task WriteAsync(string value) => await WriteAsync(value, (int)EncodingType.OCTET_STRING);

        /// <summary>
        /// Writes a string value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        public async Task WriteAsync(string value, int tag) => await WriteAsync(value, tag, EncodingScope.UNIVERSAL);

        /// <summary>
        /// Writes a string value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        /// <param name="scope">The scope for the value</param>
        public async Task WriteAsync(string value, int tag, EncodingScope scope)
        {
            var buffer = Encoding.UTF8.GetBytes(value);
            await EncodeHeaderAsync(tag, scope, true);
            await EncodeLengthAsync(buffer.Length);
            await _target.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes the integer value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        public async Task WriteAsync(int value) => await WriteAsync(value, (int)EncodingType.INTEGER);

        /// <summary>
        /// Writes the integer value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        public async Task WriteAsync(int value, int tag) => await WriteAsync(value, tag, EncodingScope.UNIVERSAL);

        /// <summary>
        /// Writes the integer value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        /// <param name="scope">The scope for the value</param>
        public async Task WriteAsync(int value, int tag, EncodingScope scope)
        {
            // Write the header
            await EncodeHeaderAsync(tag, scope, true);

            // Calculate the bits required to write this value
            var bits = value.MSB() + 1;
            var len = bits >> 3;
            var rem = bits & 0x7;

            // Will we require extra bitss?
            if (rem > 0)
                len++;

            // Write the length
            await EncodeLengthAsync(len);

            // Write the value
            if (len == 0)
                _target.WriteByte((byte)value);
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
                await _target.WriteAsync(contents, 0, idx);
            }
        }

        /// <summary>
        /// Writes the boolean value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        public async Task WriteAsync(bool value) => await WriteAsync(value, (int)EncodingType.BOOLEAN);

        /// <summary>
        /// Writes the boolean value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        public async Task WriteAsync(bool value, int tag) => await WriteAsync(value, tag, EncodingScope.UNIVERSAL);

        /// <summary>
        /// Writes the boolean value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        /// <param name="tag">The tag for the value</param>
        /// <param name="scope">The scope for the value</param>
        public async Task WriteAsync(bool value, int tag, EncodingScope scope)
        {
            await EncodeHeaderAsync(tag, scope, true);
            await EncodeLengthAsync(1);

            _target.WriteByte(value ? (byte)0xFF : (byte)0x0);
        }

        /// <summary>
        /// Copies the writer to this as a complex object
        /// </summary>
        /// <param name="writer">The writer to copy</param>
        public async Task WriteAsync(LDAPWriter writer) => await WriteAsync(writer, (int)EncodingType.SEQUENCE);

        /// <summary>
        /// Copies the writer to this as a complex object
        /// </summary>
        /// <param name="writer">The writer to copy</param>
        /// <param name="tag">The tag to use for the object</param>
        public async Task WriteAsync(LDAPWriter writer, int tag) => await WriteAsync(writer, tag, EncodingScope.UNIVERSAL);

        /// <summary>
        /// Copies the writer to this as a complex object
        /// </summary>
        /// <param name="writer">The writer to copy</param>
        /// <param name="tag">The tag to use for the object</param>
        /// <param name="scope">The scope to use for the object</param>
        public async Task WriteAsync(LDAPWriter writer, int tag, EncodingScope scope)
        {
            await EncodeHeaderAsync(tag, scope, false);

            // Get the underlying stream
            var s = writer._target;

            // Reset the stream if possible for copying
            if (s.Position != 0 && s.CanSeek)
                s.Seek(0, SeekOrigin.Begin);
            else if (s.Length - s.Position <= 0)
                throw new LDAPProtocolException("Stream has data that cannot be recovered");

            // Write the number of bytes available in the stream
            await EncodeLengthAsync((int)(s.Length - s.Position));

            // Copy the streams
            await s.CopyToAsync(_target);
        }

        /// <summary>
        /// Encodes the length to the underlying stream
        /// </summary>
        /// <param name="length">The length to encode</param>
        async Task EncodeLengthAsync(int length)
        {
            if (length < 128)
                _target.WriteByte((byte)length);
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
                await _target.WriteAsync(contents, 0, idx);
            }
        }

        /// <summary>
        /// Encodes the header information to the stream
        /// </summary>
        /// <param name="tag">The tag value</param>
        /// <param name="scope">The current scope</param>
        /// <param name="isPrimitive">Flag for if the contents are primitive</param>
        /// <returns></returns>
        async Task EncodeHeaderAsync(int tag, EncodingScope scope, bool isPrimitive)
        {
            // Create the tag header
            var header = (int)scope << 6;
            header |= (isPrimitive ? 0 : 1) << 5;

            // Safe to write entire tag
            if (tag < 31)
                _target.WriteByte((byte)(header | tag));
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
                await _target.WriteAsync(contents, 0, idx);
            }
        }
    }
}