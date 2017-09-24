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
