using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class DomImplementation : Dom.DomImplementation
    {
        private readonly Document Document;

        internal DomImplementation(Document document)
        {
            Contract.RequiresNotNull(document, nameof(document));
            this.Document = document;
        }

        /// <summary>
        /// Returns a DocumentType object that can be added to a <see cref="Document"/>.
        /// </summary>
        /// <param name="qualifiedName">The qualified name, like "html".</param>
        /// <param name="publicId">The PUBLIC identifier.</param>
        /// <param name="systemId">The SYSTEM identifier.</param>
        /// <returns>A new DocumentType associated with the Document of this DomImplementation.</returns>
        public Dom.DocumentType CreateDocumentType(string qualifiedName, string publicId, string systemId)
        {
            Contract.RequiresNotNull(qualifiedName, nameof(qualifiedName));
            Contract.RequiresNotNull(publicId, nameof(publicId));
            Contract.RequiresNotNull(systemId, nameof(systemId));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-domimplementation-createdocumenttype

            // 1. If qualifiedName does not match the Name production, throw an "InvalidCharacterError" exception.
            if (!qualifiedName.IsName())
                throw new Dom.Exceptions.InvalidCharacterException();

            // 2. If qualifiedName does not match the QName production, throw a "NamespaceError" exception.
            if (!qualifiedName.IsQName())
                throw new Dom.Exceptions.NamespaceException();

            // 3. Return a new doctype, with qualifiedName as its name, publicId as its public ID, and systemId
            //    as its system ID, and with its node document set to the associated document of the context object.
            // NOTE: No check is performed that publicId matches the PublicChar production or that systemId does
            //       not contain both a '"' and "'".
            return new DocumentType(this.Document, qualifiedName, publicId, systemId);
        }

        public Dom.Document CreateHtmlDocument(string title = null)
        {
            throw new NotImplementedException();
        }
    }
}
