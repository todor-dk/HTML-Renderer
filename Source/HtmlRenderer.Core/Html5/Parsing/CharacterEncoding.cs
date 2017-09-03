using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    public static class CharacterEncoding
    {
        public class PrescanHelper
        {
            public static Encoding DetermineEncoding(Stream stream)
            {
                Contract.RequiresNotNull(stream, nameof(stream));

                PrescanHelper helper = new PrescanHelper(stream);
                return helper.Prescan();
            }

            private PrescanHelper(Stream stream)
            {
                // Recommended by http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#ref-for-prescan-the-byte-stream-to-determine-its-encoding-1
                byte[] buffer = new byte[1024];
                long pos = stream.Position;
                int size = stream.Read(buffer, 0, buffer.Length);
                stream.Position = pos;
                if (size == buffer.Length)
                {
                    this.Buffer = buffer;
                }
                else
                {
                    this.Buffer = new byte[size];
                    Array.Copy(buffer, this.Buffer, size);
                }
                this.Position = 0;

            }

            private int Position;

            private readonly byte[] Buffer;

            private bool StartsWith(params byte[] bytes)
            {
                if ((this.Position + bytes.Length) > this.Buffer.Length)
                    throw new EndOfStreamException();

                for (int i = 0; i < bytes.Length; i++)
                {
                    if (this.Buffer[this.Position + i] != bytes[i])
                        return false;
                }

                return true;
            }

            private bool StartsWith(params ulong[] bytes)
            {
                if ((this.Position + bytes.Length) > this.Buffer.Length)
                    throw new EndOfStreamException();

                for (int i = 0; i < bytes.Length; i++)
                {
                    ulong b = this.Buffer[this.Position + i];
                    ulong val = bytes[i];
                    while(val != 0)
                    {
                        if (b == (val & 0xFF))
                            break;
                        val = val >> 8;
                    }

                    if (val == 0)
                        return false;
                }

                return true;
            }

            private byte GetValue()
            {
                if (this.Position >= this.Buffer.Length)
                    throw new EndOfStreamException();

                return this.Buffer[this.Position];
            }

            private bool IsStartTag()
            {
                // A sequence of bytes starting with a 0x3C byte (ASCII <), optionally a 0x2F byte (ASCII /), 
                //   and finally a byte in the range 0x41-0x5A or 0x61-0x7A (an ASCII letter)

                if ((this.Position + 2) > this.Buffer.Length)
                    throw new EndOfStreamException();

                byte b = this.Buffer[this.Position];
                if (b != 0x3C)
                    return false;

                b = this.Buffer[this.Position + 1];
                if (((0x41 <= b) && (b <= 0x5A)) || ((0x61 <= b) && (b <= 0x7A)))
                    return true;

                if (b != 0x2F)
                    return false;
                if ((this.Position + 3) > this.Buffer.Length)
                    throw new EndOfStreamException();
                b = this.Buffer[this.Position + 2];
                if (((0x41 <= b) && (b <= 0x5A)) || ((0x61 <= b) && (b <= 0x7A)))
                    return true;

                return false;
            }

            private bool StartsWithRange(byte start, byte end)
            {
                if (this.Position >= this.Buffer.Length)
                    throw new EndOfStreamException();

                byte b = this.Buffer[this.Position];
                return (start <= b) && (b <= end);
            }

            private void AdvanceTo(params byte[] bytes)
            {
                while (!this.StartsWith(bytes))
                    this.Position++;
            }

            private void AdvanceTo(params ulong[] bytes)
            {
                while (!this.StartsWith(bytes))
                    this.Position++;
            }

            /// <summary>
            /// Prescan a byte stream to determine its encoding.
            /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#prescan-the-byte-stream-to-determine-its-encoding
            /// </summary>
            private Encoding Prescan()
            {
                try
                {
                    return this.PrescanWorker();
                }
                catch(EndOfStreamException)
                {
                    return null;
                }
            }

            /// <summary>
            /// Prescan a byte stream to determine its encoding.
            /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#prescan-the-byte-stream-to-determine-its-encoding
            /// </summary>
            private Encoding PrescanWorker()
            {
                // 1. Let position be a pointer to a byte in the input byte stream, initially pointing at the first byte.
                this.Position = 0;

                // 2. Loop: If position points to:
                while (true)
                {
                    // * A sequence of bytes starting with: 0x3C 0x21 0x2D 0x2D(ASCII '<!--')
                    if (this.StartsWith(0x3C, 0x21, 0x2D, 0x2D))
                    {
                        // Advance the position pointer so that it points at the first 0x3E byte which is preceded by
                        // two 0x2D bytes(i.e., at the end of an ASCII '-->' sequence) and comes after the 0x3C byte that was found.
                        // (The two 0x2D bytes can be the same as those in the '<!--' sequence.)
                        this.Position++; // Skip the 0x3C
                        this.AdvanceTo(0x2D, 0x2D, 0x3E);
                    }

                    // * A sequence of bytes starting with: 0x3C, 0x4D or 0x6D, 0x45 or 0x65, 0x54 or 0x74, 0x41 or 0x61, and
                    //   one of 0x09, 0x0A, 0x0C, 0x0D, 0x20, 0x2F(case-insensitive ASCII '<meta' followed by a space or slash)
                    else if (this.StartsWith(0x3C, 0x4D6D, 0x4565, 0x5474, 0x4161, 0x090A0C0D202F))
                    {
                        // 1. Advance the position pointer so that it points at the next 0x09, 0x0A, 0x0C, 0x0D, 0x20, or 0x2F byte
                        //    (the one in sequence of characters matched above).
                        this.AdvanceTo(0x090A0C0D202F);

                        // 2. Let attribute list be an empty list of strings. 
                        List<Attribute> attributeList = new List<Attribute>();

                        // 3. Let got pragma be false.
                        bool gotPragma = false;

                        // 4. Let need pragma be null.
                        bool? needPragma = null;

                        // 5. Let charset be the null value (which, for the purposes of this algorithm, is distinct from an unrecognized 
                        //    encoding or the empty string).
                        string charset = null;

                        // 6. Attributes: Get an attribute and its value. If no attribute was computed, then jump to the processing step below.
                        Attributes:
                        Attribute attribute = this.GetAttribute();
                        if (attribute == null)
                            goto Processing;

                        // 7. If the attribute's name is already in attribute list, then return to the step labeled attributes. 
                        if (attributeList.Any(attr => attr.Name == attribute.Name))
                            goto Attributes;

                        // 8. Add the attribute's name to attribute list.
                        attributeList.Add(attribute);

                        // 9. Run the appropriate step from the following list, if one applies:

                        // * If the attribute's name is "http-equiv" 
                        if (attribute.Name == "http-equiv")
                        {
                            // If the attribute's value is "content-type", then set got pragma to true. 
                            if (attribute.Value == "content-type")
                                gotPragma = true;
                        }

                        // * If the attribute's name is "content" 
                        else if (attribute.Name == "content")
                        {
                            // Apply the algorithm for extracting a character encoding from a meta element, 
                            // giving the attribute's value as the string to parse. If a character encoding is returned, 
                            // and if charset is still set to null, let charset be the encoding returned, 
                            // and set need pragma to true. 
                            string enc = ExtractCharacterEncodingFromMeta(attribute.Value);
                            if ((charset == null) && (enc != null))
                            {
                                charset = enc;
                                needPragma = true;
                            }
                        }

                        // * If the attribute's name is "charset" 
                        else if (attribute.Name == "charset")
                        {
                            // Let charset be the result of getting an encoding from the attribute's value, and set need pragma to false. 
                            charset = attribute.Value;
                            needPragma = false;
                        }

                        // 10. Return to the step labeled attributes.
                        goto Attributes;

                        // 11. Processing: If need pragma is null, then jump to the step below labeled next byte.
                        Processing:
                        if (needPragma == null)
                            goto NextByte;

                        // 12. If need pragma is true but got pragma is false, then jump to the step below labeled next byte.
                        if (needPragma.Value && !gotPragma)
                            goto NextByte;

                        // 13. If charset is failure, then jump to the step below labeled next byte.
                        Encoding encoding = GetEncoding(charset);
                        if (encoding == null)
                            goto NextByte;

                        // 14. If charset is a UTF-16 encoding, then set charset to UTF-8.
                        if (charset == "utf-16")
                            encoding = GetEncoding("utf-8");

                        // 15. If charset is x-user-defined, then set charset to windows-1252.
                        if (charset == "x-user-defined")
                            encoding = GetEncoding("windows-1252");

                        // 16. Abort the prescan a byte stream to determine its encoding algorithm, 
                        //     returning the encoding given by charset.
                        return encoding;
                        
                    }

                    // * A sequence of bytes starting with a 0x3C byte (ASCII <), optionally a 0x2F byte (ASCII /), 
                    //   and finally a byte in the range 0x41-0x5A or 0x61-0x7A (an ASCII letter) 
                    else if (this.IsStartTag())
                    {
                        // 1. Advance the position pointer so that it points at the next 0x09 (ASCII TAB), 0x0A (ASCII LF), 
                        //    0x0C (ASCII FF), 0x0D (ASCII CR), 0x20 (ASCII space), or 0x3E (ASCII >) byte. 
                        this.Position++; // Skip the 0x3C
                        this.AdvanceTo(0x090A0C0D203E);

                        // 2. Repeatedly get an attribute until no further attributes can be found, 
                        //    then jump to the step below labeled next byte.
                        while(this.GetAttribute() != null)
                        {
                        }
                    }

                    // * A sequence of bytes starting with: 0x3C 0x21 (ASCII '<!') 
                    // * A sequence of bytes starting with: 0x3C 0x2F(ASCII '</')
                    // * A sequence of bytes starting with: 0x3C 0x3F(ASCII '<?')
                    else if (this.StartsWith(0x3C, 0x21) || this.StartsWith(0x3C, 0x2F) || this.StartsWith(0x3C, 0x3F))
                    {
                        // Advance the position pointer so that it points at the first 0x3E byte (ASCII >)
                        // that comes after the 0x3C byte that was found.
                        this.Position++; // Skip the 0x3C
                        this.AdvanceTo(0x3E);
                    }

                    // * Any other byte 
                    else
                    {
                        // Do nothing with that byte.
                    }

                    // 3. Next byte: Move position so it points at the next byte in the input byte stream, 
                    //    and return to the step above labeled loop.
                    NextByte:
                    this.Position++;
                }
            }

            private Attribute GetAttribute()
            {
                // When the prescan a byte stream to determine its encoding algorithm says to get an attribute, it means doing this:
                // See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#get-an-attribute

                try
                {
                    return this.GetAttributeWorker();
                }
                catch (EndOfStreamException)
                {
                    return null;
                }
            }

            private Attribute GetAttributeWorker()
            {
                // When the prescan a byte stream to determine its encoding algorithm says to get an attribute, it means doing this:
                // See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#get-an-attribute


                // 1. If the byte at position is one of 0x09(ASCII TAB), 0x0A(ASCII LF), 0x0C(ASCII FF), 0x0D(ASCII CR), 
                //    0x20(ASCII space), or 0x2F(ASCII /) then advance position to the next byte and redo this step.
                while (this.StartsWith(0x090A0C0D202F))
                {
                    this.Position++;
                }

                // 2.If the byte at position is 0x3E(ASCII >), then abort the get an attribute algorithm. There isn't one. 
                if (this.StartsWith(0x3E))
                    return null;

                // 3.Otherwise, the byte at position is the start of the attribute name. 
                //   Let attribute name and attribute value be the empty string.
                StringBuilder attributeName = new StringBuilder();
                StringBuilder attributeValue = new StringBuilder();

                // 4.Process the byte at position as follows: 
                ProcessName:
                // * If it is 0x3D(ASCII =), and the attribute name is longer than the empty string
                if (this.StartsWith(0x3D) && (attributeName.Length != 0))
                {
                    // Advance position to the next byte and jump to the step below labeled value.
                    this.Position++;
                    goto Value;
                }

                // *  If it is 0x09(ASCII TAB), 0x0A(ASCII LF), 0x0C(ASCII FF), 0x0D(ASCII CR), or 0x20(ASCII space)
                else if (this.StartsWith(0x090A0C0D20))
                {
                    // Jump to the step below labeled spaces.
                    goto Spaces;
                }

                // * If it is 0x2F(ASCII /) or 0x3E(ASCII >)
                else if (this.StartsWith(0x2F3E))
                {
                    //  Abort the get an attribute algorithm. The attribute's name is the value
                    // of attribute name, its value is the empty string.
                    return new Attribute(attributeName.ToString(), String.Empty);
                }

                // * If it is in the range 0x41(ASCII A) to 0x5A(ASCII Z)
                else if (this.StartsWithRange(0x41, 0x5A))
                {
                    // Append the Unicode character with code point b+0x20 to attribute name (where b is
                    // the value of the byte at position). (This converts the input to lowercase.)
                    byte b = this.GetValue();
                    attributeName.Append((char)(b + 0x20));
                }

                // * Anything else 
                else
                {
                    // Append the Unicode character with the same code point as the value of the byte at position to 
                    // attribute name. (It doesn't actually matter how bytes outside the ASCII range are handled here, 
                    // since only ASCII characters can contribute to the detection of a character encoding.) 
                    byte b = this.GetValue();
                    attributeName.Append((char)b);
                }

                // 5. Advance position to the next byte and return to the previous step.
                this.Position++;
                goto ProcessName;

                // 6.Spaces: If the byte at position is one of 0x09(ASCII TAB), 0x0A(ASCII LF), 0x0C(ASCII FF), 0x0D(ASCII CR), 
                //   or 0x20(ASCII space) then advance position to the next byte, then, repeat this step.
                Spaces:
                while(this.StartsWith(0x090A0C0D20))
                {
                    this.Position++;
                }

                // 7. If the byte at position is not 0x3D(ASCII =), abort the get an attribute algorithm. 
                // The attribute's name is the value of attribute name, its value is the empty string.
                if (!this.StartsWith(0x3D))
                    return new Attribute(attributeName.ToString(), String.Empty);

                // 8. Advance position past the 0x3D(ASCII =) byte.
                this.Position++;

                // 9. Value: If the byte at position is one of 0x09(ASCII TAB), 0x0A(ASCII LF), 0x0C(ASCII FF), 0x0D(ASCII CR), 
                //    or 0x20(ASCII space) then advance position to the next byte, then, repeat this step.
                Value:
                while (this.StartsWith(0x090A0C0D20))
                {
                    this.Position++;
                }

                // 10.Process the byte at position as follows: 
                ProcessValue:

                // * If it is 0x22(ASCII ") or 0x27 (ASCII ') 
                if (this.StartsWith(0x2227))
                {
                    // 1. Let b be the value of the byte at position.
                    byte b = this.GetValue();

                    // 2. Quote loop: Advance position to the next byte.
                    QuoteLoop:
                    this.Position++;

                    // 3. If the value of the byte at position is the value of b, then advance position to the next byte 
                    //    and abort the "get an attribute" algorithm. The attribute's name is the value of attribute name, 
                    //    and its value is the value of attribute value.
                    if (this.GetValue() == b)
                    {
                        this.Position++;
                        return new Attribute(attributeName.ToString(), attributeValue.ToString());
                    }

                    // 4. Otherwise, if the value of the byte at position is in the range 0x41(ASCII A) to 0x5A(ASCII Z), 
                    //    then append a Unicode character to attribute value whose code point is 0x20 more than the value of the byte at position.
                    else if (this.StartsWithRange(0x41, 0x5A))
                        attributeValue.Append((char)(this.GetValue() + 0x20));

                    // 5. Otherwise, append a Unicode character to attribute value whose code point is the same as the value of the byte at position.
                    else
                        attributeValue.Append((char)this.GetValue());

                    // 6. Return to the step above labeled quote loop.
                    goto QuoteLoop;
                }

                // * If it is 0x3E(ASCII >)
                else if (this.StartsWith(0x3E))
                {
                    // Abort the get an attribute algorithm. The attribute's name is the value of attribute name,
                    // its value is the empty string.
                    return new Attribute(attributeName.ToString(), String.Empty);
                }

                // * If it is in the range 0x41(ASCII A) to 0x5A(ASCII Z)
                else if (this.StartsWithRange(0x41, 0x5A))
                {
                    // Append the Unicode character with code point b+0x20 to attribute value (where b is the 
                    // value of the byte at position).Advance position to the next byte.
                    byte b = this.GetValue();
                    attributeValue.Append((char)(b + 0x20));
                }

                // * Anything else 
                else
                {
                    // Append the Unicode character with the same code point as the value of the byte at
                    // position to attribute value. Advance position to the next byte.
                    byte b = this.GetValue();
                    attributeValue.Append((char)b);
                }

                // 11. Process the byte at position as follows: 

                // * If it is 0x09(ASCII TAB), 0x0A(ASCII LF), 0x0C(ASCII FF), 0x0D(ASCII CR), 0x20(ASCII space), or 0x3E(ASCII >)
                if (this.StartsWith(0x090A0C0D203E))
                {
                    // Abort the get an attribute algorithm. The attribute's name is the value of attribute name and 
                    // its value is the value of attribute value.
                    return new Attribute(attributeName.ToString(), attributeValue.ToString());
                }

                // * If it is in the range 0x41(ASCII A) to 0x5A(ASCII Z)
                else if (this.StartsWithRange(0x41, 0x5A))
                {
                    // Append the Unicode character with code point b+0x20 to attribute 
                    //value (where b is the value of the byte at position).
                    byte b = this.GetValue();
                    attributeValue.Append((char)(b + 0x20));
                }

                // * Anything else 
                else
                {
                    // Append the Unicode character with the same code point as the value
                    // of the byte at position to attribute value. 
                    byte b = this.GetValue();
                    attributeValue.Append((char)b);
                }

                // 12. Advance position to the next byte and return to the previous step.
                this.Position++;
                goto ProcessValue;


            }

            private class Attribute
            {
                public readonly string Name;
                public readonly string Value;

                public Attribute(string name, string value)
                {
                    this.Name = name ?? String.Empty;
                    this.Value = value ?? String.Empty;
                }
            }
        }

        public static string ExtractCharacterEncodingFromMeta(string s)
        {
            // The algorithm for extracting a character encoding from a meta element, given a string s, is as follows. 
            // It either returns a character encoding or nothing.
            // See: http://www.w3.org/TR/2017/CR-html51-20170620/infrastructure.html#algorithm-for-extracting-a-character-encoding-from-a-meta-element
            s = s ?? String.Empty;

            // 1. Let position be a pointer into s, initially pointing at the start of the string.
            int position = 0;

            // 2. Loop: Find the first seven characters in s after position that are an ASCII case-insensitive 
            //    match for the word "charset".If no such match is found, return nothing and abort these steps.
            Loop:
            int idx = s.IndexOf("charset", position, StringComparison.Ordinal);
            if (idx == -1)
                return null;

            // 3. Skip any space characters that immediately follow the word "charset" (there might not be any).
            position = idx + 7;
            while ((position < s.Length) && s[position].IsSpaceCharacter())
            {
                position++;
            }

            // 4. If the next character is not a U + 003D EQUALS SIGN(=), then move position to 
            //    point just before that next character, and jump back to the step labeled loop.
            if ((position >= s.Length) || (s[position] != '='))
                goto Loop;

            // 5. Skip any space characters that immediately follow the equals sign(there might not be any).
            position++;
            while ((position < s.Length) && s[position].IsSpaceCharacter())
            {
                position++;
            }

            // 6. Process the next character as follows:
            char ch = (position < s.Length) ? s[position] : Characters.EOF;

            // * If it is a U + 0022 QUOTATION MARK character(") and there is a later U+0022 QUOTATION MARK character (") in s
            // * If it is a U + 0027 APOSTROPHE character(') and there is a later U+0027 APOSTROPHE character (') in s
            if (((ch == '\u0022') || (ch == '\u0027')) && (s.IndexOf(ch, position + 1) != -1))
            {
                // Return the result of getting an encoding from the substring that is between 
                // this character and the next earliest occurrence of this character.
                idx = s.IndexOf(ch, position + 1);
                string encoding = s.Substring(position + 1, idx - position - 1);
                return encoding;
            }

            // * If it is an unmatched U + 0022 QUOTATION MARK character(") 
            // * If it is an unmatched U + 0027 APOSTROPHE character(') 
            // * If there is no next character
            else if ((ch == '\u0022') || (ch == '\u0027') || (ch == Characters.EOF))
            {
                // Return nothing.
                return null;
            }

            // * Otherwise
            else
            {
                // Return the result of getting an encoding from the substring that consists of 
                // this character up to but not including the first space character or U + 003B SEMICOLON 
                // character(;), or the end of s, whichever comes first.
                for (int i = position; i < s.Length; i++)
                {
                    ch = s[i];
                    if (ch.IsSpaceCharacter() || (ch == ';'))
                    {
                        string encoding = s.Substring(position, i - position);
                        return encoding;
                    }
                }

                return s.Substring(position);
            }
        }

        public static Encoding GetEncoding(string name)
        {
            // Logic is defined here: http://www.w3.org/TR/encoding/#concept-encoding-get

            name = (name ?? String.Empty).Trim();
            string encodingName;
            EncodingAliasNameMap.TryGetValue(name, out encodingName);
            if (encodingName == null)
                return null;
            Encoding encoding;
            EncodingMap.TryGetValue(encodingName, out encoding);
            return encoding;
        }

        private static readonly Dictionary<string, string> EncodingAliasNameMap;

        private static readonly Dictionary<string, Encoding> EncodingMap;

        public static Encoding GetDefaultEncoding()
        {
            // See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#determining-the-character-encoding

            // In other environments, the default encoding is typically dependent on the user’s locale 
            // (an approximation of the languages, and thus often encodings, of the pages that the user is likely to frequent). 
            // The following table gives suggested defaults based on the user’s locale, for compatibility with legacy content. 
            // Locales are identified by BCP 47 language tags
            string name = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            string longName = System.Globalization.CultureInfo.CurrentCulture.Name;
            string encoding = null;

            // Arabic
            if (name == "ar")
                encoding = "windows-1256";
            // Bashkir
            else if (name == "ba")
                encoding = "windows-1251";
            // Belarusian
            else if (name == "be")
                encoding = "windows-1251";
            // Bulgarian
            else if (name == "bg")
                encoding = "windows-1251";
            // Czech
            else if (name == "cs")
                encoding = "windows-1250";
            // Greek
            else if (name == "el")
                encoding = "ISO-8859-7";
            // Estonian
            else if (name == "et")
                encoding = "windows-1257";
            // Persian
            else if (name == "fa")
                encoding = "windows-1256";
            // Hebrew
            else if (name == "he")
                encoding = "windows-1255";
            // Croatian
            else if (name == "hr")
                encoding = "windows-1250";
            // Hungarian
            else if (name == "hu")
                encoding = "ISO-8859-2";
            // Japanese
            else if (name == "ja")
                encoding = "Shift_JIS";
            // Kazakh
            else if (name == "kk")
                encoding = "windows-1251";
            // Korean
            else if (name == "ko")
                encoding = "euc-kr";
            // Kurdish
            else if (name == "ku")
                encoding = "windows-1254";
            // Kyrgyz
            else if (name == "ky")
                encoding = "windows-1251";
            // Lithuanian
            else if (name == "lt")
                encoding = "windows-1257";
            // Latvian
            else if (name == "lv")
                encoding = "windows-1257";
            // Macedonian
            else if (name == "mk")
                encoding = "windows-1251";
            // Polish
            else if (name == "pl")
                encoding = "ISO-8859-2";
            // Russian
            else if (name == "ru")
                encoding = "windows-1251";
            // Yakut
            else if (name == "sah")
                encoding = "windows-1251";
            // Slovak
            else if (name == "sk")
                encoding = "windows-1250";
            // Slovenian
            else if (name == "sl")
                encoding = "ISO-8859-2";
            // Serbian
            else if (name == "sr")
                encoding = "windows-1251";
            // Tajik
            else if (name == "tg")
                encoding = "windows-1251";
            // Thai
            else if (name == "th")
                encoding = "windows-874";
            // Turkish
            else if (name == "tr")
                encoding = "windows-1254";
            // Tatar
            else if (name == "tt")
                encoding = "windows-1251";
            // Ukrainian
            else if (name == "uk")
                encoding = "windows-1251";
            // Vietnamese
            else if (name == "vi")
                encoding = "windows-1258";
            // Chinese (People’s Republic of China)
            else if (longName == "zh-CN")
                encoding = "GB18030";
            // Chinese (Taiwan)
            else if (longName == "zh-TW")
                encoding = "Big5";
            // All other locales
            else 
                encoding = "windows-1252";

            return Encoding.GetEncoding(encoding);

        }

        public static Encoding GetEncodingForBom(Stream stream)
        {
            // NOTE: A leading Byte Order Mark (BOM) causes the character
            // encoding argument to be ignored and will itself be skipped.

            long pos = stream.Position;
            try
            {
                byte[] buffer = new byte[3];
                int len = stream.Read(buffer, 0, buffer.Length);

                // UTF-8: EF BB BF
                if ((len >= 3) && (buffer[0] == 0xEF) && (buffer[1] == 0xBB) && (buffer[2] == 0xBF))
                {
                    pos = pos + 3;
                    return Encoding.UTF8;
                }

                // UTF-16(BE): FE FF
                if ((len >= 2) && (buffer[0] == 0xFE) && (buffer[1] == 0xFF))
                {
                    pos = pos + 2;
                    return Encoding.BigEndianUnicode;
                }

                // UTF-16(LE): FF FE
                if ((len >= 2) && (buffer[0] == 0xFF) && (buffer[1] == 0xFE))
                {
                    pos = pos + 2;
                    return Encoding.Unicode;
                }

                return null; // No BOM
            }
            finally
            {
                stream.Position = pos;
            }
        }

        static CharacterEncoding()
        {
            // See: http://www.w3.org/TR/encoding/#concept-encoding-get
            EncodingAliasNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // The Encoding
            EncodingAliasNameMap.Add("unicode-1-1-utf-8", "UTF-8");
            EncodingAliasNameMap.Add("utf-8", "UTF-8");
            EncodingAliasNameMap.Add("utf8", "UTF-8");
            // Legacy single-byte encodings
            EncodingAliasNameMap.Add("866", "IBM866");
            EncodingAliasNameMap.Add("cp866", "IBM866");
            EncodingAliasNameMap.Add("csibm866", "IBM866");
            EncodingAliasNameMap.Add("ibm866", "IBM866");
            EncodingAliasNameMap.Add("csisolatin2", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso-8859-2", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso-ir-101", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso8859-2", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso88592", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso_8859-2", "ISO-8859-2");
            EncodingAliasNameMap.Add("iso_8859-2:1987", "ISO-8859-2");
            EncodingAliasNameMap.Add("l2", "ISO-8859-2");
            EncodingAliasNameMap.Add("latin2", "ISO-8859-2");
            EncodingAliasNameMap.Add("csisolatin3", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso-8859-3", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso-ir-109", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso8859-3", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso88593", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso_8859-3", "ISO-8859-3");
            EncodingAliasNameMap.Add("iso_8859-3:1988", "ISO-8859-3");
            EncodingAliasNameMap.Add("l3", "ISO-8859-3");
            EncodingAliasNameMap.Add("latin3", "ISO-8859-3");
            EncodingAliasNameMap.Add("csisolatin4", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso-8859-4", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso-ir-110", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso8859-4", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso88594", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso_8859-4", "ISO-8859-4");
            EncodingAliasNameMap.Add("iso_8859-4:1988", "ISO-8859-4");
            EncodingAliasNameMap.Add("l4", "ISO-8859-4");
            EncodingAliasNameMap.Add("latin4", "ISO-8859-4");
            EncodingAliasNameMap.Add("csisolatincyrillic", "ISO-8859-5");
            EncodingAliasNameMap.Add("cyrillic", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso-8859-5", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso-ir-144", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso8859-5", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso88595", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso_8859-5", "ISO-8859-5");
            EncodingAliasNameMap.Add("iso_8859-5:1988", "ISO-8859-5");
            EncodingAliasNameMap.Add("arabic", "ISO-8859-6");
            EncodingAliasNameMap.Add("asmo-708", "ISO-8859-6");
            EncodingAliasNameMap.Add("csiso88596e", "ISO-8859-6");
            EncodingAliasNameMap.Add("csiso88596i", "ISO-8859-6");
            EncodingAliasNameMap.Add("csisolatinarabic", "ISO-8859-6");
            EncodingAliasNameMap.Add("ecma-114", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso-8859-6", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso-8859-6-e", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso-8859-6-i", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso-ir-127", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso8859-6", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso88596", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso_8859-6", "ISO-8859-6");
            EncodingAliasNameMap.Add("iso_8859-6:1987", "ISO-8859-6");
            EncodingAliasNameMap.Add("csisolatingreek", "ISO-8859-7");
            EncodingAliasNameMap.Add("ecma-118", "ISO-8859-7");
            EncodingAliasNameMap.Add("elot_928", "ISO-8859-7");
            EncodingAliasNameMap.Add("greek", "ISO-8859-7");
            EncodingAliasNameMap.Add("greek8", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso-8859-7", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso-ir-126", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso8859-7", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso88597", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso_8859-7", "ISO-8859-7");
            EncodingAliasNameMap.Add("iso_8859-7:1987", "ISO-8859-7");
            EncodingAliasNameMap.Add("sun_eu_greek", "ISO-8859-7");
            EncodingAliasNameMap.Add("csiso88598e", "ISO-8859-8");
            EncodingAliasNameMap.Add("csisolatinhebrew", "ISO-8859-8");
            EncodingAliasNameMap.Add("hebrew", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso-8859-8", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso-8859-8-e", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso-ir-138", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso8859-8", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso88598", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso_8859-8", "ISO-8859-8");
            EncodingAliasNameMap.Add("iso_8859-8:1988", "ISO-8859-8");
            EncodingAliasNameMap.Add("visual", "ISO-8859-8");
            EncodingAliasNameMap.Add("csiso88598i", "ISO-8859-8-I");
            EncodingAliasNameMap.Add("iso-8859-8-i", "ISO-8859-8-I");
            EncodingAliasNameMap.Add("logical", "ISO-8859-8-I");
            EncodingAliasNameMap.Add("csisolatin6", "ISO-8859-10");
            EncodingAliasNameMap.Add("iso-8859-10", "ISO-8859-10");
            EncodingAliasNameMap.Add("iso-ir-157", "ISO-8859-10");
            EncodingAliasNameMap.Add("iso8859-10", "ISO-8859-10");
            EncodingAliasNameMap.Add("iso885910", "ISO-8859-10");
            EncodingAliasNameMap.Add("l6", "ISO-8859-10");
            EncodingAliasNameMap.Add("latin6", "ISO-8859-10");
            EncodingAliasNameMap.Add("iso-8859-13", "ISO-8859-13");
            EncodingAliasNameMap.Add("iso8859-13", "ISO-8859-13");
            EncodingAliasNameMap.Add("iso885913", "ISO-8859-13");
            EncodingAliasNameMap.Add("iso-8859-14", "ISO-8859-14");
            EncodingAliasNameMap.Add("iso8859-14", "ISO-8859-14");
            EncodingAliasNameMap.Add("iso885914", "ISO-8859-14");
            EncodingAliasNameMap.Add("csisolatin9", "ISO-8859-15");
            EncodingAliasNameMap.Add("iso-8859-15", "ISO-8859-15");
            EncodingAliasNameMap.Add("iso8859-15", "ISO-8859-15");
            EncodingAliasNameMap.Add("iso885915", "ISO-8859-15");
            EncodingAliasNameMap.Add("iso_8859-15", "ISO-8859-15");
            EncodingAliasNameMap.Add("l9", "ISO-8859-15");
            EncodingAliasNameMap.Add("iso-8859-16", "ISO-8859-16");
            EncodingAliasNameMap.Add("cskoi8r", "KOI8-R");
            EncodingAliasNameMap.Add("koi", "KOI8-R");
            EncodingAliasNameMap.Add("koi8", "KOI8-R");
            EncodingAliasNameMap.Add("koi8-r", "KOI8-R");
            EncodingAliasNameMap.Add("koi8_r", "KOI8-R");
            EncodingAliasNameMap.Add("koi8-ru", "KOI8-U");
            EncodingAliasNameMap.Add("koi8-u", "KOI8-U");
            EncodingAliasNameMap.Add("csmacintosh", "macintosh");
            EncodingAliasNameMap.Add("mac", "macintosh");
            EncodingAliasNameMap.Add("macintosh", "macintosh");
            EncodingAliasNameMap.Add("x-mac-roman", "macintosh");
            EncodingAliasNameMap.Add("dos-874", "windows-874");
            EncodingAliasNameMap.Add("iso-8859-11", "windows-874");
            EncodingAliasNameMap.Add("iso8859-11", "windows-874");
            EncodingAliasNameMap.Add("iso885911", "windows-874");
            EncodingAliasNameMap.Add("tis-620", "windows-874");
            EncodingAliasNameMap.Add("windows-874", "windows-874");
            EncodingAliasNameMap.Add("cp1250", "windows-1250");
            EncodingAliasNameMap.Add("windows-1250", "windows-1250");
            EncodingAliasNameMap.Add("x-cp1250", "windows-1250");
            EncodingAliasNameMap.Add("cp1251", "windows-1251");
            EncodingAliasNameMap.Add("windows-1251", "windows-1251");
            EncodingAliasNameMap.Add("x-cp1251", "windows-1251");
            EncodingAliasNameMap.Add("ansi_x3.4-1968", "windows-1252");
            EncodingAliasNameMap.Add("ascii", "windows-1252");
            EncodingAliasNameMap.Add("cp1252", "windows-1252");
            EncodingAliasNameMap.Add("cp819", "windows-1252");
            EncodingAliasNameMap.Add("csisolatin1", "windows-1252");
            EncodingAliasNameMap.Add("ibm819", "windows-1252");
            EncodingAliasNameMap.Add("iso-8859-1", "windows-1252");
            EncodingAliasNameMap.Add("iso-ir-100", "windows-1252");
            EncodingAliasNameMap.Add("iso8859-1", "windows-1252");
            EncodingAliasNameMap.Add("iso88591", "windows-1252");
            EncodingAliasNameMap.Add("iso_8859-1", "windows-1252");
            EncodingAliasNameMap.Add("iso_8859-1:1987", "windows-1252");
            EncodingAliasNameMap.Add("l1", "windows-1252");
            EncodingAliasNameMap.Add("latin1", "windows-1252");
            EncodingAliasNameMap.Add("us-ascii", "windows-1252");
            EncodingAliasNameMap.Add("windows-1252", "windows-1252");
            EncodingAliasNameMap.Add("x-cp1252", "windows-1252");
            EncodingAliasNameMap.Add("cp1253", "windows-1253");
            EncodingAliasNameMap.Add("windows-1253", "windows-1253");
            EncodingAliasNameMap.Add("x-cp1253", "windows-1253");
            EncodingAliasNameMap.Add("cp1254", "windows-1254");
            EncodingAliasNameMap.Add("csisolatin5", "windows-1254");
            EncodingAliasNameMap.Add("iso-8859-9", "windows-1254");
            EncodingAliasNameMap.Add("iso-ir-148", "windows-1254");
            EncodingAliasNameMap.Add("iso8859-9", "windows-1254");
            EncodingAliasNameMap.Add("iso88599", "windows-1254");
            EncodingAliasNameMap.Add("iso_8859-9", "windows-1254");
            EncodingAliasNameMap.Add("iso_8859-9:1989", "windows-1254");
            EncodingAliasNameMap.Add("l5", "windows-1254");
            EncodingAliasNameMap.Add("latin5", "windows-1254");
            EncodingAliasNameMap.Add("windows-1254", "windows-1254");
            EncodingAliasNameMap.Add("x-cp1254", "windows-1254");
            EncodingAliasNameMap.Add("cp1255", "windows-1255");
            EncodingAliasNameMap.Add("windows-1255", "windows-1255");
            EncodingAliasNameMap.Add("x-cp1255", "windows-1255");
            EncodingAliasNameMap.Add("cp1256", "windows-1256");
            EncodingAliasNameMap.Add("windows-1256", "windows-1256");
            EncodingAliasNameMap.Add("x-cp1256", "windows-1256");
            EncodingAliasNameMap.Add("cp1257", "windows-1257");
            EncodingAliasNameMap.Add("windows-1257", "windows-1257");
            EncodingAliasNameMap.Add("x-cp1257", "windows-1257");
            EncodingAliasNameMap.Add("cp1258", "windows-1258");
            EncodingAliasNameMap.Add("windows-1258", "windows-1258");
            EncodingAliasNameMap.Add("x-cp1258", "windows-1258");
            EncodingAliasNameMap.Add("x-mac-cyrillic", "x-mac-cyrillic");
            EncodingAliasNameMap.Add("x-mac-ukrainian", "x-mac-cyrillic");
            // Legacy multi-byte Chinese (simplified) encodings
            EncodingAliasNameMap.Add("chinese", "GBK");
            EncodingAliasNameMap.Add("csgb2312", "GBK");
            EncodingAliasNameMap.Add("csiso58gb231280", "GBK");
            EncodingAliasNameMap.Add("gb2312", "GBK");
            EncodingAliasNameMap.Add("gb_2312", "GBK");
            EncodingAliasNameMap.Add("gb_2312-80", "GBK");
            EncodingAliasNameMap.Add("gbk", "GBK");
            EncodingAliasNameMap.Add("iso-ir-58", "GBK");
            EncodingAliasNameMap.Add("x-gbk", "GBK");
            EncodingAliasNameMap.Add("gb18030", "gb18030");
            // Legacy multi-byte Chinese (traditional) encodings
            EncodingAliasNameMap.Add("big5", "Big5");
            EncodingAliasNameMap.Add("big5-hkscs", "Big5");
            EncodingAliasNameMap.Add("cn-big5", "Big5");
            EncodingAliasNameMap.Add("csbig5", "Big5");
            EncodingAliasNameMap.Add("x-x-big5", "Big5");
            // Legacy multi-byte Japanese encodings
            EncodingAliasNameMap.Add("cseucpkdfmtjapanese", "EUC-JP");
            EncodingAliasNameMap.Add("euc-jp", "EUC-JP");
            EncodingAliasNameMap.Add("x-euc-jp", "EUC-JP");
            EncodingAliasNameMap.Add("csiso2022jp", "ISO-2022-JP");
            EncodingAliasNameMap.Add("iso-2022-jp", "ISO-2022-JP");
            EncodingAliasNameMap.Add("csshiftjis", "Shift_JIS");
            EncodingAliasNameMap.Add("ms932", "Shift_JIS");
            EncodingAliasNameMap.Add("ms_kanji", "Shift_JIS");
            EncodingAliasNameMap.Add("shift-jis", "Shift_JIS");
            EncodingAliasNameMap.Add("shift_jis", "Shift_JIS");
            EncodingAliasNameMap.Add("sjis", "Shift_JIS");
            EncodingAliasNameMap.Add("windows-31j", "Shift_JIS");
            EncodingAliasNameMap.Add("x-sjis", "Shift_JIS");
            // Legacy multi-byte Korean encodings
            EncodingAliasNameMap.Add("cseuckr", "EUC-KR");
            EncodingAliasNameMap.Add("csksc56011987", "EUC-KR");
            EncodingAliasNameMap.Add("euc-kr", "EUC-KR");
            EncodingAliasNameMap.Add("iso-ir-149", "EUC-KR");
            EncodingAliasNameMap.Add("korean", "EUC-KR");
            EncodingAliasNameMap.Add("ks_c_5601-1987", "EUC-KR");
            EncodingAliasNameMap.Add("ks_c_5601-1989", "EUC-KR");
            EncodingAliasNameMap.Add("ksc5601", "EUC-KR");
            EncodingAliasNameMap.Add("ksc_5601", "EUC-KR");
            EncodingAliasNameMap.Add("windows-949", "EUC-KR");
            // Legacy miscellaneous encodings
            EncodingAliasNameMap.Add("csiso2022kr", "replacement");
            EncodingAliasNameMap.Add("hz-gb-2312", "replacement");
            EncodingAliasNameMap.Add("iso-2022-cn", "replacement");
            EncodingAliasNameMap.Add("iso-2022-cn-ext", "replacement");
            EncodingAliasNameMap.Add("iso-2022-kr", "replacement");
            EncodingAliasNameMap.Add("utf-16be", "UTF-16BE");
            EncodingAliasNameMap.Add("utf-16", "UTF-16LE");
            EncodingAliasNameMap.Add("utf-16le", "UTF-16LE");
            EncodingAliasNameMap.Add("x-user-defined", "x-user-defined");


            // Now, figure out what this system supports.
            EncodingMap = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase);
            foreach(KeyValuePair<string, string> pair in EncodingAliasNameMap)
            {
                if (!EncodingMap.ContainsKey(pair.Value))
                {
                    try
                    {
                        Encoding encoding = Encoding.GetEncoding(pair.Value);
                        EncodingMap[pair.Value] = encoding;
                    }
                    catch (ArgumentException)
                    {
                        EncodingMap[pair.Value] = null;
                    }
                }
            }
        }
    }
}
