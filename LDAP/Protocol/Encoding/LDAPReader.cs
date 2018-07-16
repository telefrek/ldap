using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telefrek.LDAP.Protocol.IO;

namespace Telefrek.LDAP.Protocol.Encoding
{
    /// <summary>
    /// Reads a stream as an LDAP protocol stream and decodes objects
    /// </summary>
    internal class LDAPReader
    {
        Stream _source;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="source">The reader source</param>
        public LDAPReader(Stream source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets the current tag for the reader
        /// </summary>
        public int Tag { get; private set; }

        /// <summary>
        /// Gets the current scope for the reader
        /// </summary>
        public EncodingScope Scope { get; private set; }

        /// <summary>
        /// Gets the primitive flag for the reader
        /// </summary>
        public bool IsPrimitive { get; private set; }

        /// <summary>
        /// Getss the length of the next token
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Tries to read the next token metadata from the stream
        /// </summary>
        /// <returns>True if another token was read successfully</returns>
        public async Task<bool> ReadAsync()
        {
            // Try to read the next byte
            try
            {
                var b = _source.ReadByte();
                if (b < 0)
                    return false;

                // Get the scope and primitive flags
                Scope = (EncodingScope)(b >> 6);
                IsPrimitive = (b & 0x20) == 0;

                // Check for overflow
                if ((b & 0x1F) == (0x1F))
                {
                    Tag = 0;
                    for (var i = 0; i < 8; ++i)
                    {
                        b = _source.ReadByte();
                        Tag = (Tag << 7) | (b & 0x7F);

                        // Check if we need to read more
                        if ((b & 0x80) == 0)
                            break;
                    }
                }
                else
                    Tag = b & 0x1F;

                // Read the length
                // Detect if we have a small byte
                b = _source.ReadByte();

                if ((b & 0x80) == 0)
                    Length = b;
                else
                {
                    var numBytes = b & 0x7F;
                    var contents = new byte[numBytes];
                    var idx = 0;

                    // Read from the stream
                    do
                    {
                        var numRead = await _source.ReadAsync(contents, idx, numBytes);
                        if (numRead == -1)
                            throw new LDAPProtocolException("Stream disconnected before read finished");
                        numBytes -= numRead;
                        idx += numRead;
                    } while (numBytes > 0);

                    Length = 0;
                    for (var i = 0; i < idx; ++i)
                        Length = (Length << 8) | contents[i];
                }

                // Success
                return true;
            }
            catch (IOException)
            {
                // TODO: log this
                return false;
            }
            catch (Exception e)
            {
                throw new LDAPProtocolException("Error during read", e);
            }
        }

        /// <summary>
        /// Create a reader for the non-primitive type to decode
        /// </summary>
        /// <returns>A reader scoped to the next complex token</returns>
        public LDAPReader CreateReader()
        {
            if (IsPrimitive == false)
                return new LDAPReader(new BoundedStream(_source, Length));

            throw new InvalidOperationException("Cannot create reader for primitive types");
        }

        /// <summary>
        /// Reads the LDAPResponse object off the stream
        /// </summary>
        /// <returns>An appropriately typed LDAPResponse</returns>
        public async Task<LDAPResponse> ReadResponseAsync()
        {
            if (Scope != EncodingScope.UNIVERSAL || Tag != (int)EncodingType.SEQUENCE)
                throw new LDAPProtocolException("Invalid envelope");

            var messageReader = CreateReader();
            if (!await messageReader.ReadAsync())
                throw new LDAPProtocolException("Truncated envelope");

            // Read the message id
            var messageId = await messageReader.ReadAsIntAsync();

            // Ensure we have more data
            if (!await messageReader.ReadAsync())
                throw new LDAPProtocolException("Truncated envelope");

            // Get the appropriate message type
            switch ((ProtocolOp)messageReader.Tag)
            {
                case ProtocolOp.BIND_RESPONSE:
                    return await ReadMessageContents<BindResponse>(messageReader, messageId);
                case ProtocolOp.SEARCH_RESPONSE:
                    return await ReadMessageContents<SearchResponse>(messageReader, messageId);
                case ProtocolOp.SEARCH_RESULT:
                    return await ReadMessageContents<SearchResult>(messageReader, messageId);
                case ProtocolOp.ADD_RESPONSE:
                    return await ReadMessageContents<AddResponse>(messageReader, messageId);
                case ProtocolOp.DEL_RESPONSE:
                    return await ReadMessageContents<DeleteResponse>(messageReader, messageId);
                default:
                    throw new LDAPProtocolException("Unknown/Invalid protocol operation");
            }
        }

        /// <summary>
        /// Reads an LDAPAttribute collection from the stream
        /// </summary>
        /// <returns>A collection of LDAPAttribute objects</returns>
        public async Task<ICollection<LDAPAttribute>> ReadPartialListAsync()
        {
            var aList = new List<LDAPAttribute>();

            // Validate reader has content
            while (await ReadAsync())
            {
                var aReader = CreateReader();
                if (await aReader.ReadAsync())
                    aList.Add(await ReadPartialAsync(aReader));
            }

            return aList;
        }

        /// <summary>
        /// Reads the individual PartialAttribute off the stream
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>The LDAPAttribute value</returns>
        async Task<LDAPAttribute> ReadPartialAsync(LDAPReader reader)
        {
            var attr = new LDAPAttribute();

            // Read the description
            attr.Description = await reader.ReadAsStringAsync();
            attr.Values = new List<string>();

            // Read the values
            await reader.GuardAsync((int)EncodingType.SET);
            var valReader = reader.CreateReader();
            while (await valReader.ReadAsync())
                attr.Values.Add(await valReader.ReadAsStringAsync());

            return attr;
        }

        public async Task<T> ReadMessageContents<T>(LDAPReader messageReader, int messageId)
        where T : LDAPResponse, new()
        {
            var op = new T() { MessageId = messageId };
            await op.ReadContentsAsync(messageReader);

            return op;
        }

        /// <summary>
        /// Reads the next token as a boolean value
        /// </summary>
        /// <returns>The next token as a bool</returns>
        public Task<bool> ReadAsBooleanAsync()
        {
            if (Length != 1)
                throw new LDAPProtocolException("Corrupted stream");

            var b = _source.ReadByte();

            if (b < 0)
                throw new LDAPProtocolException("Stream ended before value could be read");
            if (b == 0)
                return Task.FromResult(false);
            if (b == 0xFF)
                return Task.FromResult(true);

            throw new LDAPProtocolException("Invalid boolean value received");
        }

        /// <summary>
        /// Reads the next token from the stream as an int32 value
        /// </summary>
        /// <returns>The next token as an int32</returns>
        public async Task<int> ReadAsIntAsync()
        {
            var buffer = new byte[Length];
            await _source.ReadAsync(buffer, 0, Length);

            var val = 0;
            for (var i = 0; i < Length; ++i)
                val = (val << 8) | buffer[i];

            return val;
        }

        /// <summary>
        /// Reads the next token as a UTF-8 string
        /// </summary>
        /// <returns>The next token as a string</returns>
        public async Task<string> ReadAsStringAsync()
        {
            var buffer = new byte[Length];
            if (Length > 0)
                await _source.ReadAsync(buffer, 0, Length);

            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Skips the current token in the stream
        /// </summary>
        public async Task SkipAsync()
        {
            // Seek if possible
            if (_source.CanSeek)
                _source.Seek(Length, SeekOrigin.Current);
            else
            {
                // Read into a temp buffer to advance
                var buf = new byte[Length];
                var numRead = 0;

                // Keep reading, past partials until we pass the current token length
                while (numRead < Length)
                {
                    var n = await _source.ReadAsync(buf, numRead, Length - numRead);
                    if (n < 0)
                        throw new LDAPProtocolException("Failed to read from the underlying stream");
                    numRead += n;
                }
            }
        }
    }
}