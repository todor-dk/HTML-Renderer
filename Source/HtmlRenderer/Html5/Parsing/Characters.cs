using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal static class Characters
    {
        public const char EOF = '\uFFFF';

        public const char Null = '\u0000';

        public const char Tab = '\u0009';

        public const char Space = '\u0020';

        public const char Lf = '\u000A';

        public const char Ff = '\u000C';

        public const char Cr = '\u000D';

        public const char Ampersand = '\u0026';

        public const char ReplacementCharacter = '\uFFFD';

        public static bool IsSpaceCharacter(this char ch)
        {
            // The space characters, for the purposes of this specification, are:
            // U +0020 SPACE, "tab" (U+0009), "LF" (U+000A), "FF" (U+000C), and "CR" (U+000D).
            return (ch == Space) || (ch == Tab) || (ch == Lf) || (ch == Ff) || (ch == Cr);
        }

        /// <summary>
        /// One of: "tab" (U+0009), "LF" (U+000A), "FF" (U+000C), U+0020 SPACE.
        /// </summary>
        public static bool IsWhiteSpaceCharacter(this char ch)
        {
            // The White_Space characters are those that have the Unicode property "White_Space"
            // in the Unicode PropList.txt data file (ftp://www.unicode.org/Public/9.0.0/ucd/PropList.txt)
            // 0009..000D    ; White_Space # Cc   [5] <control-0009>..<control-000D>
            // 0020          ; White_Space # Zs       SPACE
            // 0085          ; White_Space # Cc       <control-0085>
            // 00A0          ; White_Space # Zs       NO-BREAK SPACE
            // 1680          ; White_Space # Zs       OGHAM SPACE MARK
            // 2000..200A    ; White_Space # Zs  [11] EN QUAD..HAIR SPACE
            // 2028          ; White_Space # Zl       LINE SEPARATOR
            // 2029          ; White_Space # Zp       PARAGRAPH SEPARATOR
            // 202F          ; White_Space # Zs       NARROW NO-BREAK SPACE
            // 205F          ; White_Space # Zs       MEDIUM MATHEMATICAL SPACE
            // 3000          ; White_Space # Zs       IDEOGRAPHIC SPACE
            for (int i = 0; i < WhiteSpaceCharacters.Length; i++)
            {
                if (ch == WhiteSpaceCharacters[i])
                    return true;
            }

            return false;
        }

        private static readonly char[] WhiteSpaceCharacters = new char[]
        {
            '\u0020', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0085', '\u00A0', '\u1680',
            '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A',
            '\u2028', '\u2029', '\u202F', '\u205F', '\u3000'
        };

        public static bool IsControlCharacter(this char ch)
        {
            // The control characters are those whose Unicode "General_Category" property has the value
            // "Cc" in the Unicode UnicodeData.txt data file. (ftp://www.unicode.org/Public/9.0.0/ucd/UnicodeData.txt)
            for (int i = 0; i < ControlCharacters.Length; i++)
            {
                if (ch == ControlCharacters[i])
                    return true;
            }

            return false;
        }

        private static readonly char[] ControlCharacters = new char[]
        {
            '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
            '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
            '\u007F', '\u0080', '\u0081', '\u0082', '\u0083', '\u0084', '\u0085', '\u0086',
            '\u0087', '\u0088', '\u0089', '\u008A', '\u008B', '\u008C', '\u008D', '\u008E',
            '\u008F', '\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', '\u0096',
            '\u0097', '\u0098', '\u0099', '\u009A', '\u009B', '\u009C', '\u009D', '\u009E',
            '\u009F'
        };

        public static bool IsUppercaseAsciiLetter(this char ch)
        {
            return ('A' <= ch) && (ch <= 'Z');
        }

        public static char ToLowercaseAsciiLetter(this char ch)
        {
            if ((ch < 'A') || ('Z' > ch))
                return ch;

            // Add 0x0020 to the character's code point
            return (char)(ch + '\u0020');
        }

        public static bool IsLowercaseAsciiLetter(this char ch)
        {
            return ('a' <= ch) && (ch <= 'z');
        }

        public static bool IsAsciiLetter(this char ch)
        {
            return (('a' <= ch) && (ch <= 'z')) || (('A' <= ch) && (ch <= 'Z'));
        }

        public static bool IsAsciiDigit(this char ch)
        {
            return ('0' <= ch) && (ch <= '9');
        }

        public static bool IsAlphaNumericAsciiCharacter(this char ch)
        {
            // The alphanumeric ASCII characters are those that are either uppercase ASCII letters, lowercase ASCII letters, or ASCII digits.
            return IsLowercaseAsciiLetter(ch) || IsUppercaseAsciiLetter(ch) || IsAsciiDigit(ch);
        }

        public static bool IsAsciiHexDigit(this char ch)
        {
            // The ASCII hex digits are the characters in the ranges ASCII digits,
            // U +0041 LATIN CAPITAL LETTER A to U+0046 LATIN CAPITAL LETTER F,
            // and U+0061 LATIN SMALL LETTER A to U+0066 LATIN SMALL LETTER F.
            return IsAsciiDigit(ch) || (('A' <= ch) && (ch <= 'F')) || (('a' <= ch) && (ch <= 'f'));
        }

        public static bool IsUppercaseAsciiHexDigit(this char ch)
        {
            // The uppercase ASCII hex digits are the characters in the ranges ASCII digits
            // and U+0041 LATIN CAPITAL LETTER A to U+0046 LATIN CAPITAL LETTER F only.
            return IsAsciiDigit(ch) || (('A' <= ch) && (ch <= 'F'));
        }

        public static bool IsLowercaseAsciiHexDigit(this char ch)
        {
            // The lowercase ASCII hex digits are the characters in the ranges ASCII digits
            // and U+0061 LATIN SMALL LETTER A to U+0066 LATIN SMALL LETTER F only.
            return IsAsciiDigit(ch) || (('a' <= ch) && (ch <= 'f'));
        }

        public static bool IsInRange(this char ch, char start, char end)
        {
            return (start <= ch) && (ch <= end);
        }

        public static bool IsInRange(this int ch, int start, int end)
        {
            return (start <= ch) && (ch <= end);
        }

        public static bool IsOneOf(this char ch, params char[] characters)
        {
            if (characters == null)
                return false;

            for (int i = 0; i < characters.Length; i++)
            {
                if (ch == characters[i])
                    return true;
            }

            return false;
        }

        public static bool IsOneOf(this int ch, params int[] characters)
        {
            if (characters == null)
                return false;

            for (int i = 0; i < characters.Length; i++)
            {
                if (ch == characters[i])
                    return true;
            }

            return false;
        }

        public static bool IsOneOf(this char ch, string characters)
        {
            if (characters == null)
                return false;

            for (int i = 0; i < characters.Length; i++)
            {
                if (ch == characters[i])
                    return true;
            }

            return false;
        }
    }
}
