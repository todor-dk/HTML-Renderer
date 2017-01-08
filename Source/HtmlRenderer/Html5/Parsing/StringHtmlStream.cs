﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public sealed class StringHtmlStream : HtmlStream
    {
        private readonly string Html;

        private int Index;

        private readonly int Limit;

        public StringHtmlStream(string html)
        {
            Contract.RequiresNotNull(html, nameof(html));
            this.Html = html;
            this.Index = 0;
            this.Limit = html.Length;
        }

        public override char ReadChar()
        {
            if (this.Index >= this.Limit)
                return Characters.EOF;
            char ch = this.Html[this.Index];
            this.Index++;
            if (ch == Characters.EOF)
                ch = Characters.ReplacementCharacter; // U+FFFF is not allowed character
            return ch;

            // TO-DO
            /*
            8.2.2.5 Preprocessing the input stream

            The input stream consists of the characters pushed into it as the input byte stream is
            decoded or from the various APIs that directly manipulate the input stream.

            One leading U+FEFF BYTE ORDER MARK character must be ignored if any are present in the input stream.

            NOTE: The requirement to strip a U+FEFF BYTE ORDER MARK character regardless of whether that character
            was used to determine the byte order is a willful violation of Unicode, motivated by a desire to increase the
            resilience of user agents in the face of naïve transcoders.

            Any occurrences of any characters in the ranges U+0001 to U+0008, U+000E to U+001F, U+007F to U+009F,
            U+FDD0 to U+FDEF, and characters U+000B, U+FFFE, U+FFFF, U+1FFFE, U+1FFFF, U+2FFFE, U+2FFFF, U+3FFFE,
            U+3FFFF, U+4FFFE, U+4FFFF, U+5FFFE, U+5FFFF, U+6FFFE, U+6FFFF, U+7FFFE, U+7FFFF, U+8FFFE, U+8FFFF,
            U+9FFFE, U+9FFFF, U+AFFFE, U+AFFFF, U+BFFFE, U+BFFFF, U+CFFFE, U+CFFFF, U+DFFFE, U+DFFFF, U+EFFFE,
            U+EFFFF, U+FFFFE, U+FFFFF, U+10FFFE, and U+10FFFF are parse errors. These are all control characters or
            permanently undefined Unicode characters (noncharacters).

            Any character that is a not a Unicode character, i.e. any isolated surrogate, is a parse error.
            (These can only find their way into the input stream via script APIs such as document.write().)

            "CR" (U+000D) characters and "LF" (U+000A) characters are treated specially. All CR characters must be converted
            to LF characters, and any LF characters that immediately follow a CR character must be ignored. Thus, newlines in
            HTML DOMs are represented by LF characters, and there are never any CR characters in the input to the tokenization stage.
            */
        }
    }
}
