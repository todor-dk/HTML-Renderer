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

        public Document ParseDocument(HtmlStream htmlContents, string url)
        {
            Contract.RequiresNotNull(htmlContents, nameof(htmlContents));
            Contract.RequiresNotEmptyOrWhiteSpace(url, nameof(url));

            return this.ParseDocument(htmlContents, new DocumentParsingContext(url));
        }

        public Document ParseDocument(HtmlStream htmlContents, DocumentParsingContext parsingContext)
        {
            Contract.RequiresNotNull(htmlContents, nameof(htmlContents));
            Contract.RequiresNotNull(parsingContext, nameof(parsingContext));

            // Determine the charset of the document. See: http://www.w3.org/TR/2017/CR-html51-20170620/syntax.html#determining-the-character-encoding
            HtmlStream.RevertInformation revertInfo = htmlContents.DetermineEncoding(parsingContext);

            // The HtmlStream now has a charset (or at least a suggestion to what this may be)
            Document document = this.CreateHtmlDocument(parsingContext.Url, htmlContents.CharacterSet);

            try
            {
                // Try to parse - if this hits a <META> with charset information and incompatible encoding, EncodingChangedException may be thrown.
                DomParser.ParseDocument(document, parsingContext, Internal.DomImplementation.InternalDomFactory.Current, htmlContents);
            }
            catch (HtmlStream.EncodingChangedException)
            {
                // The encoding has changed. We need to re-parser.

                // 1. Revert the HTML stream.
                revertInfo.Revert();

                // 2. Create a new document with the correct encoding.
                document = this.CreateHtmlDocument(parsingContext.Url, htmlContents.CharacterSet);

                // 3. Parse again.
                DomParser.ParseDocument(document, parsingContext, Internal.DomImplementation.InternalDomFactory.Current, htmlContents);
            }
            
            return document;
        }
    }
}
