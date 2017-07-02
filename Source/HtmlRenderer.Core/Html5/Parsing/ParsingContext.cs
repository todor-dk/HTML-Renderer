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
        public ParsingContext(string url)
        {
            Contract.RequiresNotEmptyOrWhiteSpace(url, nameof(url));
            this.Url = url;
        }

        public string Url { get; private set; }

        // http://www.w3.org/TR/html51/semantics-embedded-content.html#iframe-iframe-srcdoc-document
        public bool IsIFrameSource
        {
            get { return false; }
        }

        // http://www.w3.org/TR/html51/syntax.html#parsing-html-fragments
        public bool IsFragmentParsing
        {
            get { return false; }
        }

        public Element FragmentContextElement
        {
            get { return null; }
        }

        internal abstract DomFactory GetDomFactory();
    }
}
