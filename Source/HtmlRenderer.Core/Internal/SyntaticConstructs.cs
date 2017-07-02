using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal
{
    /// <summary>
    /// See: http://www.w3.org/TR/xml/#sec-common-syn
    /// </summary>
    internal static class SyntaticConstructs
    {
        #region Names and Tokens

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(this char ch, char start, char end)
        {
            return (start <= ch) && (ch <= end);
        }

        public static bool IsNameStartChar(this char ch)
        {
            // NameStartChar ::= ":" | [A - Z] | "_" | [a - z] | [#xC0-#xD6] |
            //                  [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] |
            //                  [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] |
            //                  [#x2C00-#x2FEF] | [#x3001-#xD7FF] | [#xF900-#xFDCF] |
            //                  [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
            return (ch == ':') || ch.IsInRange('A', 'Z') || (ch == '_') || ch.IsInRange('a', 'z') || ch.IsInRange('\u00C0', '\u00D6') ||
                ch.IsInRange('\u00D8', '\u00F6') || ch.IsInRange('\u00F8', '\u02FF') || ch.IsInRange('\u0370', '\u037D') ||
                ch.IsInRange('\u037f', '\u1FFF') || ch.IsInRange('\u200C', '\u200D') || ch.IsInRange('\u2070', '\u218F') ||
                ch.IsInRange('\u2C00', '\u2FEF') || ch.IsInRange('\u3001', '\uD7FF') || ch.IsInRange('\uF900', '\uFDCF') ||
                ch.IsInRange('\uFDF0', '\uFFFD') || Char.IsLowSurrogate(ch) || Char.IsHighSurrogate(ch);
        }

        public static bool IsNameChar(this char ch)
        {
            // NameChar ::= NameStartChar | "-" | "." | [0 - 9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
            return ch.IsNameStartChar() || (ch == '-') || (ch == '.') || ch.IsInRange('0', '9') ||
                (ch == '\u00B7') || ch.IsInRange('\u0300', '\u036F') || ch.IsInRange('\u203F', '\u2040');
        }

        public static bool IsName(this string str)
        {
            // Name ::= NameStartChar (NameChar) *
            if (String.IsNullOrEmpty(str))
                return false;
            if (!str[0].IsNameStartChar())
                return false;
            for (int i = 1; i < str.Length; i++)
            {
                if (!str[i].IsNameChar())
                    return false;
            }

            return true;
        }

        #endregion

        public static bool IsQName(this string str)
        {
            // See: http://www.w3.org/TR/xml-names/#NT-QName
            // TODO
            return str.IsName();
        }
    }
}
