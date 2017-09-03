/*
 * **************************************************************************
 *
 * Copyright (c) Todor Todorov / Scientia Software. 
 *
 * This source code is subject to terms and conditions of the 
 * license agreement found in the project directory. 
 * See: $(ProjectDir)\LICENSE.txt ... in the root of this project.
 * By using this source code in any fashion, you are agreeing 
 * to be bound by the terms of the license agreement.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    public sealed class StringHtmlStream : HtmlStream
    {
        private readonly string Html;

        private int Index;

        private readonly int Limit;

        public StringHtmlStream(string html, string characterSet = null)
        {
            // NOTE: Some algorithms feed the parser by directly adding characters to 
            // the input stream rather than adding bytes to the input byte stream. 

            Contract.RequiresNotNull(html, nameof(html));
            this.Html = html;
            this.Index = 0;
            this.Limit = html.Length;
            if (characterSet != null)
                this.SetCharacterSet(characterSet, ConfidenceEnum.Irrelevant);
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

        /// <summary>
        /// 8.2.2.2. Determining the character encoding
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#determining-the-character-encoding
        /// </summary>
        /// <param name="context">Browsing context.</param>
        /// <returns>Information, that if we need to change the encoding, is capable of reverting the input stream to the current state.</returns>
        public override RevertInformation DetermineEncoding(ParsingContext context)
        {
            // NOTE: Some algorithms feed the parser by directly adding characters to 
            // the input stream rather than adding bytes to the input byte stream. 
            this.SetCharacterSet(this.CharacterSet ?? "utf-8", ConfidenceEnum.Irrelevant);
            return new PrivateRevertInformation(this, this.Index);
        }

        /// <summary>
        /// 8.2.2.4. Changing the encoding while parsing
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#changing-the-encoding-while-parsing
        /// </summary>
        internal override void ChangeEncoding(Encoding encoding)
        {
            // We are working on a memory string (not stream). There is no encoding.
            throw new NotSupportedException();
        }

        private class PrivateRevertInformation : RevertInformation
        {
            private readonly StringHtmlStream Owner;
            private readonly int Index;
            public PrivateRevertInformation(StringHtmlStream owner, int index)
            {
                this.Owner = owner;
                this.Index = index;
            }

            public override void Revert()
            {
                this.Owner.Index = this.Index;
            }
        }
    }
}
