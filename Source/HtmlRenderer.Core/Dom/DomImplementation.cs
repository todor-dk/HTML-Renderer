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

namespace Scientia.HtmlRenderer.Dom
{
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-domimplementation
    /// </summary>
    public interface DomImplementation
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Returns a DocumentType object that can be added to a <see cref="Document"/>.
        /// </summary>
        /// <param name="qualifiedName">The qualified name, like "html".</param>
        /// <param name="publicId">The PUBLIC identifier.</param>
        /// <param name="systemId">The SYSTEM identifier.</param>
        /// <returns>A new DocumentType associated with the Document of this DomImplementation.</returns>
        DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId);

        Document CreateHtmlDocument(string title = null);
    }
}
