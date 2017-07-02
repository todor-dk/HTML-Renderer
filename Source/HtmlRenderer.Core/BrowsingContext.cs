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

namespace Scientia.HtmlRenderer
{
    public class BrowsingContext
    {
        // See: http://www.w3.org/TR/html5/webappapis.html#enabling-and-disabling-scripting
        public bool IsScriptingEnabled
        {
            get { return false; }
        }

        public Dom.Document CreateHtmlDocument(string baseUri, string characterSet)
        {
            return new Internal.DomImplementation.HtmlDocument(this, baseUri, characterSet);
        }

        public Html5.Parsing.ParsingContext GetDocumentParsingContext(string url)
        {
            return new Internal.DomImplementation.DocumentParsingContext(this, url);
        }
    }
}
