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

using Scientia.HtmlRenderer.Dom;
using Scientia.HtmlRenderer.Html5.Parsing;
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

        public Document CreateHtmlDocument(string baseUri, string characterSet)
        {
            return new Internal.DomImplementation.HtmlDocument(this, baseUri, characterSet);
        }

        public Document ParseDocument(HtmlStream htmlContents, string url, string characterSet = "utf-8")
        {
            Contract.RequiresNotNull(htmlContents, nameof(htmlContents));
            Contract.RequiresNotEmptyOrWhiteSpace(url, nameof(url));
            Contract.RequiresNotEmptyOrWhiteSpace(characterSet, nameof(characterSet));

            return this.ParseDocument(htmlContents, new DocumentParsingContext(url, characterSet));
        }

        public Document ParseDocument(HtmlStream htmlContents, DocumentParsingContext parsingContext)
        {
            Contract.RequiresNotNull(htmlContents, nameof(htmlContents));
            Contract.RequiresNotNull(parsingContext, nameof(parsingContext));

            Document document = this.CreateHtmlDocument(parsingContext.Url, parsingContext.CharacterSet);
            DomParser.ParseDocument(document, parsingContext, Internal.DomImplementation.InternalDomFactory.Current, htmlContents);
            return document;
        }
    }
}
