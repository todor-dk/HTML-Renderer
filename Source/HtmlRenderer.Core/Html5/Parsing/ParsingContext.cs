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
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    public abstract class ParsingContext
    {
        public ParsingContext(string url, string characterSet)
        {
            Contract.RequiresNotEmptyOrWhiteSpace(url, nameof(url));
            Contract.RequiresNotEmptyOrWhiteSpace(characterSet, nameof(characterSet));

            this.Url = url;
            this.CharacterSet = characterSet;
        }

        public string Url { get; private set; }

        public string CharacterSet { get; private set; }

        // http://www.w3.org/TR/html51/semantics-embedded-content.html#iframe-iframe-srcdoc-document
        public abstract bool IsIFrameSource { get; }

        // http://www.w3.org/TR/html51/syntax.html#parsing-html-fragments
        public abstract bool IsFragmentParsing { get; }

        public abstract Element FragmentContextElement { get; }

        /// <summary>
        /// Raised when a parse error occurs during tokanization or parsing of the HTML stream.
        /// </summary>
        public event EventHandler<ParseErrorEventArgs> ParseError;

        internal void OnParseError(ParseErrorEventArgs args)
        {
            this.ParseError?.Invoke(this, args);
        }

        internal void OnParseError(ParseError error)
        {
            this.ParseError?.Invoke(this, new ParseErrorEventArgs(error));
        }
    }
}
