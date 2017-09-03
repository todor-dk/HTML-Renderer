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
    /// <summary>
    /// 8.2.2. The input byte stream
    /// http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#the-input-byte-stream
    /// <para/>
    /// This is the stream (typically coming over the network or from the local file system)
    /// that is fed into the tokenization stage of the parser.
    /// </summary>
    public abstract class HtmlStream
    {
        public string CharacterSet { get; private set; }

        /// <summary>
        /// When the HTML parser is decoding an input byte stream, it uses a character encoding and a confidence. 
        /// The confidence is either tentative, certain, or irrelevant. The encoding used, and whether the 
        /// confidence in that encoding is tentative or certain, is used during the parsing to determine whether
        /// to change the encoding. If no encoding is necessary, e.g., because the parser is operating on a
        /// Unicode stream and doesn’t have to use a character encoding at all, then the confidence is irrelevant.
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#the-input-byte-stream
        /// </summary>
        public ConfidenceEnum EncodingConfidence { get; protected set; }

        /// <summary>
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#confidence
        /// </summary>
        public enum ConfidenceEnum
        {
            Tentative,
            Certain,
            Irrelevant
        }

        public HtmlStream()
        {
            this.CharacterSet = null;
            this.EncodingConfidence = ConfidenceEnum.Tentative;
        }

        protected void SetCharacterSet(string characterSet, ConfidenceEnum confidence)
        {
            Contract.RequiresNotEmptyOrWhiteSpace(characterSet, nameof(characterSet));

            this.CharacterSet = characterSet;
            this.EncodingConfidence = confidence;
        }

        /// <summary>
        /// 8.2.2.4. Changing the encoding while parsing
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#changing-the-encoding-while-parsing
        /// </summary>
        internal abstract void ChangeEncoding(Encoding encoding);

        /// <summary>
        /// Given a character encoding, the bytes in the input byte stream must be 
        /// converted to characters for the tokenizer.
        /// </summary>
        public abstract char ReadChar();


        public abstract class RevertInformation
        {
            public abstract void Revert();
        }

        /// <summary>
        /// 8.2.2.2. Determining the character encoding
        /// See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#determining-the-character-encoding
        /// </summary>
        /// <param name="context">Browsing context.</param>
        /// <returns>Information, that if we need to change the encoding, is capable of reverting the input stream to the current state.</returns>
        public abstract RevertInformation DetermineEncoding(ParsingContext context);

        public override string ToString()
        {
            return String.Format("{0} using {1} charset {2} encoding", this.GetType().Name, this.EncodingConfidence, this.CharacterSet);
        }

        public class EncodingChangedException : Exception
        {
        }
    }
}
