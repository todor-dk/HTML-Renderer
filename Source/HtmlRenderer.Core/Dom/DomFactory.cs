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
using Scientia.HtmlRenderer.Html5.Parsing;

namespace Scientia.HtmlRenderer.Dom
{
    internal abstract class DomFactory
    {
        public abstract void SetQuirksMode(Document document, QuirksMode mode);

        /// <summary>
        /// Append a DocumentType node to the Document node.
        /// Associate the DocumentType node with the Document object so that it is
        /// returned as the value of the doctype attribute of the Document object.
        /// </summary>
        public void AppendDocType(Document document, string name, string publicIdentifier, string systemIdentifier)
        {
            DocumentType doctype = document.Implementation.CreateDocumentType(name, publicIdentifier, systemIdentifier);
            document.AppendChild(doctype);
        }

        public Element CreateElement(Document document, string namespaceUri, string tagName, Html5.Parsing.Attribute[] attributes)
        {
            Element element = document.CreateElementNS(namespaceUri, tagName);

            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    Html5.Parsing.Attribute attr = attributes[i];
                    element.SetAttribute(attr.Name, attr.Value);
                }
            }

            return element;
        }

        public void AppendElement(Node parentElement, Element element)
        {
            parentElement.AppendChild(element);
        }

        public void InsertElementBefore(Element parentElement, Element element, Element referenceElement)
        {
            parentElement.InsertBefore(element, referenceElement);
        }

        public void AppendChildNode(Element parentElement, Node child, bool unparentIfNeeded)
        {
            Contract.ParameterNotUsed(unparentIfNeeded); // AppendChild does the automatic un-parenting.

            parentElement.AppendChild(child);
        }

        public void InsertText(DomParser.AdjustedInsertLocation location, string data)
        {
            Text textNode = location.ParentElement.OwnerDocument.CreateTextNode(data);
            if (location.BeforeSibling != null)
                location.ParentElement.InsertBefore(textNode, location.BeforeSibling);
            else
                location.ParentElement.AppendChild(textNode);
        }

        public Comment CreateAndAppendComment(Document parent, string data)
        {
            Comment comment = parent.CreateComment(data);
            parent.AppendChild(comment);
            return comment;
        }

        public Comment CreateAndAppendComment(Element parent, Element beforeSibling, string data)
        {
            Comment comment = parent.OwnerDocument.CreateComment(data);
            if (beforeSibling == null)
                parent.AppendChild(comment);
            else
                parent.InsertBefore(comment, beforeSibling);
            return comment;
        }

        public void AssociateWithForm(Element element, Element form)
        {
            // Nothing here.
        }

        public void InvokeResetAlgorithm(Element element)
        {
            // Nothing here.

            // See: http://www.w3.org/TR/html51/sec-forms.html#reset-algorithm

            // This resets <input>, <keygen>, <output>, <select> and <textarea>.
            // The alg. is element specific. We don't need to implement this, because we have no interactivity.
        }

        public void StopParsing()
        {
            // Nothing here.

            // See: http://www.w3.org/TR/html51/syntax.html#stopped

            // There are a lot of steps to make the doc interactive. But this is irrelevant for us!
        }
    }
}
