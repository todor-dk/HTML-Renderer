using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    public sealed class StreamHtmlStream : HtmlStream
    {
        private readonly Stream Stream;

        private Encoding Encoding;

        private bool HasEncounteredNonAsciiCharacters;

        private Decoder Decoder;

        private bool ReadIntoByteBuffer;

        private readonly byte[] ByteBuffer;

        private int ByteBufferLimit;

        private int ByteBufferIndex;

        private readonly char[] CharBuffer;

        private int CharBufferLimit;

        private int CharBufferIndex;

        /// <summary>
        /// Instantiate a new StreamHtmlStream instance.
        /// </summary>
        /// <param name="stream">
        /// The underlaying network or file stream.
        /// </param>
        /// <param name="characterSet">
        /// The character set as either given by out-of-band metadata or by the user.
        /// This is used as a hint for the encoding, unless the <paramref name="enforceCharacterSet"/>
        /// is set, which means that the given encoding is authoritative.
        /// </param>
        /// <param name="enforceCharacterSet">
        /// Indicates if <paramref name="characterSet"/> is a hint (normally determined from
        /// out-of-band metadata, i.e. HTTP headers) or if set to true, the character set
        /// is authoritative.
        /// </param>
        public StreamHtmlStream(Stream stream, string characterSet = null, bool enforceCharacterSet = false)
        {
            Contract.RequiresNotNull(stream, nameof(stream));

            this.Stream = stream;
            this.ByteBuffer = new byte[4096];
            this.CharBuffer = new char[this.ByteBuffer.Length];
            this.ReadIntoByteBuffer = true;

            Encoding encoding = CharacterEncoding.GetEncodingForBom(stream);
            if (encoding != null)
            {
                this.SetEncoding(encoding);
                // ISSUE: Are we supposed to set this to always use UTF-8??? 
                // The spec says: If charset is a UTF-16 encoding, then set charset to UTF-8. 
                this.SetCharacterSet(encoding.WebName, ConfidenceEnum.Certain);
            }
            else
            {
                // If charset given from outside, store it.
                if (enforceCharacterSet || !String.IsNullOrWhiteSpace(characterSet))
                    this.SetCharacterSet(characterSet, enforceCharacterSet ? ConfidenceEnum.Certain : ConfidenceEnum.Tentative);
            }
        }

        /// <summary>
        /// 8.2.2.2. Determining the character encoding
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#determining-the-character-encoding
        /// </summary>
        /// <param name="context">Browsing context.</param>
        /// <returns>Information, that if we need to change the encoding, is capable of reverting the input stream to the current state.</returns>
        public override RevertInformation DetermineEncoding(ParsingContext context)
        {
            // User agents must use the following algorithm, called the encoding sniffing algorithm, 
            // to determine the character encoding to use when decoding a document in the first pass. 
            // This algorithm takes as input any out-of-band metadata available to the user agent 
            // (e.g., the Content-Type metadata of the document) and all the bytes available so far, 
            // and returns a character encoding and a confidence that is either tentative or certain.

            // 1. If the user has explicitly instructed the user agent to override the document’s character
            //    encoding with a specific encoding, optionally return that encoding with the confidence
            //    certain and abort these steps.

            // NOTE: Typically, user agents remember such user requests across sessions, and in some cases
            // apply them to documents in iframes as well.
            
            // If the user instructs us to use a specific charset, then we set the confidence to certain in the ctor.
            if (this.EncodingConfidence != ConfidenceEnum.Tentative)
                return new PrivateRevertInformation(this);

            // 2. The user agent may wait for more bytes of the resource to be available, either in this step or at
            //    any later step in this algorithm. For instance, a user agent might wait 500ms or 1024 bytes,
            //    whichever came first. In general preparsing the source to find the encoding improves performance,
            //    as it reduces the need to throw away the data structures used when parsing upon finding the encoding
            //    information. However, if the user agent delays too long to obtain data to determine the encoding,
            //    then the cost of the delay could outweigh any performance improvements from the preparse.

            // NOTE: The authoring conformance requirements for character encoding declarations limit them to only appearing 
            // in the first 1024 bytes. User agents are therefore encouraged to use the prescan algorithm below (as invoked
            // by these steps) on the first 1024 bytes, but not to stall beyond that. 

            // NB: We don't wait, because this is too much burden on the implementation.

            // 3. If the transport layer specifies a character encoding, and it is supported, return
            //    that encoding with the confidence certain, and abort these steps. 
            Encoding encoding;
            if (this.CharacterSet != null)
            {
                encoding = CharacterEncoding.GetEncoding(this.CharacterSet);
                if (encoding != null)
                {
                    this.SetEncoding(encoding);
                    this.SetCharacterSet(encoding.WebName, ConfidenceEnum.Certain);
                    return new PrivateRevertInformation(this);
                }
            }

            // 4. Optionally prescan the byte stream to determine its encoding. The end condition is that the user agent
            //    decides that scanning further bytes would not be efficient. User agents are encouraged to only prescan
            //    the first 1024 bytes. User agents may decide that scanning any bytes is not efficient, in which case these
            //    substeps are entirely skipped.

            // The aforementioned algorithm either aborts unsuccessfully or returns a character encoding. If it returns a
            // character encoding, then this algorithm must be aborted, returning the same encoding, with confidence tentative.
            encoding = CharacterEncoding.PrescanHelper.DetermineEncoding(this.Stream);
            if (encoding != null)
            {
                this.SetEncoding(encoding);
                this.SetCharacterSet(encoding.WebName, ConfidenceEnum.Tentative);
                return new PrivateRevertInformation(this);
            }

            // 5. If the HTML parser for which this algorithm is being run is associated with a Document that is itself in a
            //    nested browsing context, run these substeps:

            // 1. Let new document be the Document with which the HTML parser is associated.
            // 2. Let parent document be the Document through which new document is nested (the active document of the parent browsing context of new document). 
            // 3. If parent document’s origin is not the same origin as new document’s origin, then abort these substeps.
            // 4. If parent document’s character encoding is not an ASCII - compatible encoding, then abort these substeps. 
            // 5. Return parent document’s character encoding, with the confidence tentative, and abort the encoding sniffing algorithm’s steps. 

            // We don't support FRAME / IFRAME or similar parent / child contexts.
            FutureVersions.CurrentlyIrrelevant();

            // 6. Otherwise, if the user agent has information on the likely encoding for this page, e.g., based on the encoding
            //    of the page when it was last visited, then return that encoding, with the confidence tentative, and abort these steps. 
            FutureVersions.CurrentlyIrrelevant();

            // 7. The user agent may attempt to autodetect the character encoding from applying frequency analysis or other algorithms to the data stream. 
            //    Such algorithms may use information about the resource other than the resource’s contents, including the address of the resource.
            //    If autodetection succeeds in determining a character encoding, and that encoding is a supported encoding, then return that encoding,
            //    with the confidence tentative, and abort these steps

            // No auto-detection. The HTML5 standard does not recommend this.

            // 8. Otherwise, return an implementation-defined or user-specified default character encoding, with the confidence tentative.
            encoding = CharacterEncoding.GetDefaultEncoding();
            this.SetEncoding(encoding);
            this.SetCharacterSet(encoding.WebName, ConfidenceEnum.Tentative);
            return new PrivateRevertInformation(this);
        }

        private class PrivateRevertInformation : RevertInformation
        {
            private readonly StreamHtmlStream Owner;
            private readonly long Position;

            public PrivateRevertInformation(StreamHtmlStream owner)
            {
                this.Owner = owner;
                this.Position = owner.Stream.Position;
            }

            public override void Revert()
            {
                this.Owner.ResetBuffers(this.Position);
            }
        }

        /// <summary>
        /// 8.2.2.4. Changing the encoding while parsing
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#changing-the-encoding-while-parsing
        /// </summary>
        internal override void ChangeEncoding(Encoding encoding)
        {
            Contract.RequiresNotNull(encoding, nameof(encoding));
            if (this.Encoding == null)
                throw new InvalidOperationException("Cannot call ChangeEncoding before DetermineEncoding");
            

            // NOTE: This algorithm is only invoked when a new encoding is found declared on a meta element. 

            // When the parser requires the user agent to change the encoding, it must run the following steps. 
            // This might happen if the encoding sniffing algorithm described above failed to find a character encoding,
            // or if it found a character encoding that was not the actual encoding of the file.

            // 1. If the encoding that is already being used to interpret the input stream is a UTF-16 encoding, 
            //    then set the confidence to certain and abort these steps. The new encoding is ignored; 
            //    if it was anything but the same encoding, then it would be clearly incorrect.
            string name = this.Encoding.WebName;
            if (name.IndexOf("utf-16", StringComparison.OrdinalIgnoreCase) == 0)
                return;

            // 2. If the new encoding is a UTF-16 encoding, then change it to UTF-8.
            name = encoding.WebName;
            if (name.IndexOf("utf-16", StringComparison.OrdinalIgnoreCase) == 0)
                encoding = Encoding.UTF8;

            // 3. If the new encoding is the x-user-defined encoding, then change it to Windows-1252. 
            FutureVersions.CurrentlyIrrelevant();

            // 4. If the new encoding is identical or equivalent to the encoding that is already being
            //    used to interpret the input stream, then set the confidence to certain and abort these steps.
            //    This happens when the encoding information found in the file matches what the encoding
            //    sniffing algorithm determined to be the encoding, and in the second pass through the parser
            //    if the first pass found that the encoding sniffing algorithm described in the earlier 
            //    section failed to find the right encoding.
            if (this.Encoding.WebName == encoding.WebName)
            {
                this.SetCharacterSet(this.Encoding.WebName, ConfidenceEnum.Certain);
                return;
            }

            // 5. If all the bytes up to the last byte converted by the current decoder have the same Unicode 
            //    interpretations in both the current encoding and the new encoding, and if the user agent 
            //    supports changing the converter on the fly, then the user agent may change to the new converter
            //    for the encoding on the fly. Set the document’s character encoding and the encoding used to convert
            //    the input stream to the new encoding, set the confidence to certain, and abort these steps.
            if (!this.HasEncounteredNonAsciiCharacters)
            {
                this.SetEncoding(encoding);
                this.SetCharacterSet(this.Encoding.WebName, ConfidenceEnum.Certain);
                return;
            }

            // 6. Otherwise, navigate to the document again, with replacement enabled, and using the same source 
            //    browsing context, but this time skip the encoding sniffing algorithm and instead just set the 
            //    encoding to the new encoding and the confidence to certain.Whenever possible, this should be 
            //    done without actually contacting the network layer(the bytes should be re - parsed from memory), 
            //    even if, e.g., the document is marked as not being cacheable. If this is not possible and contacting
            //    the network layer would involve repeating a request that uses a method other than GET), then instead
            //    set the confidence to certain and ignore the new encoding.The resource will be misinterpreted. 
            //    User agents may notify the user of the situation, to aid in application development. 
            this.SetEncoding(encoding);
            this.SetCharacterSet(encoding.WebName, ConfidenceEnum.Certain);
            // Throw an exception that the encoding has changed. The parser handles this and re-parses the document.
            throw new EncodingChangedException();
        }

        private void SetEncoding(Encoding encoding)
        {
            this.ResetBuffers(null);
            this.Encoding = encoding;
            this.Decoder = encoding.GetDecoder();
        }
        
        public override char ReadChar()
        {
            if (this.CharBufferIndex >= this.CharBufferLimit)
            {
                this.FillBuffers();

                if (this.CharBufferIndex >= this.CharBufferLimit)
                    return Characters.EOF;
            }
            
            char ch = this.CharBuffer[this.CharBufferIndex];
            this.CharBufferIndex++;
            if (ch == Characters.EOF)
                ch = Characters.ReplacementCharacter; // U+FFFF is not allowed character

            // If we encounter any character >= 0x80, set HasEncounteredNonAsciiCharacters to true.
            if ((ch >= '\u0080') && !this.HasEncounteredNonAsciiCharacters)
                this.HasEncounteredNonAsciiCharacters = true;

            return ch;
        }

        private void FillBuffers()
        {
            if (this.ReadIntoByteBuffer)
            {
                // Read at most the number of bytes that will fit in the input buffer. The 
                // return value is the actual number of bytes read, or zero if no bytes remain. 
                this.ByteBufferLimit = this.Stream.Read(this.ByteBuffer, 0, this.ByteBuffer.Length);
                this.ByteBufferIndex = 0;
            }

            // If this is the last input data, flush the decoder's internal buffer and state.
            int bytesUsed;
            bool flush = (this.ByteBufferLimit == 0);
            this.Decoder.Convert(this.ByteBuffer, this.ByteBufferIndex, this.ByteBufferLimit - this.ByteBufferIndex,
                this.CharBuffer, 0, this.CharBuffer.Length, flush, out bytesUsed, out this.CharBufferLimit, out this.ReadIntoByteBuffer);
            this.CharBufferIndex = 0;
            // Increment byteIndex to the next block of bytes in the input buffer, if any, to convert.
            this.ByteBufferIndex = this.ByteBufferIndex + bytesUsed;
        }

        private void ResetBuffers(long? position)
        {
            if (position == null)
            {
                // NB: This needs to be called with the existing decoding for things to work.
                if (this.Decoder != null)
                {
                    int remainingBytes = this.ByteBufferLimit - this.ByteBufferIndex;
                    int remainingCharBytes = this.Encoding.GetByteCount(this.CharBuffer, this.CharBufferIndex, this.CharBufferLimit - this.CharBufferIndex);
                    // Seek back in the stream with a delta equals to whatever was left in the buffers.
                    long pos = this.Stream.Position - (remainingBytes + remainingCharBytes);
                    this.Stream.Position = pos;
                }
            }
            else
            {
                this.Stream.Position = position.Value;
            }

            this.ReadIntoByteBuffer = true;
            this.ByteBufferLimit = 0;
            this.ByteBufferIndex = 0;
            this.CharBufferLimit = 0;
            this.CharBufferIndex = 0;
            this.Decoder?.Reset();
        }
    }
}
