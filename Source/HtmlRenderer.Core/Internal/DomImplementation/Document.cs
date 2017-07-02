using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;
using Scientia.HtmlRenderer.Html5;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class Document : ParentNode, Dom.Document
    {
        private readonly BrowsingContext BrowsingContext;

        private readonly string Loction;

        internal Document(BrowsingContext browsingContext, string location, string characterSet)
            : base(null)
        {
            Contract.RequiresNotNull(browsingContext, nameof(browsingContext));
            Contract.RequiresNotEmptyOrWhiteSpace(characterSet, nameof(characterSet));

            this.QuirksMode = QuirksMode.NoQuirks;
            this.BrowsingContext = browsingContext;
            this.Loction = location;
            this.CharacterSet = characterSet;
            this.Implementation = new DomImplementation(this);
        }

        #region Node interface overrides

        /// <summary>
        /// Returns the absolute base URL of a node.
        /// </summary>
        /// <remarks>
        /// The base URL of a document defaults to the document's address.
        /// The base URL of an element in HTML normally equals the base URL of the document the node is in.
        /// <para/>
        /// If the document contains xml:base attributes (which you shouldn't do in HTML documents),
        /// the element.baseURI takes the xml:base attributes of element's parents into account when
        /// computing the base URL.
        /// NB: We do not support xml:base
        /// </remarks>
        public override string BaseUri
        {
            get { return this.Loction; }
        }

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return "#document"; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.Document; }
        }

        /// <summary>
        /// Returns a duplicate of this node.
        /// </summary>
        /// <param name="deep">True if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>A new node that is a clone this node.</returns>
        public override Dom.Node CloneNode(bool deep = false)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Document interface

        public QuirksMode QuirksMode { get; internal set; }

        /// <summary>
        /// Returns the character encoding of the current document.
        /// </summary>
        public string CharacterSet { get; private set; }

        /// <summary>
        /// Return "BackCompat" if the document is in quirks mode, and "CSS1Compat" otherwise.
        /// </summary>
        public string CompatMode
        {
            get
            {
                // "BackCompat" if the document is in quirks mode;
                // "CSS1Compat" if the document is in no - quirks(also known as "standards")
                //      mode or limited - quirks(also known as "almost standards") mode.
                return (this.QuirksMode == QuirksMode.Quirks) ? "BackCompat" : "CSS1Compat";
            }
        }

        /// <summary>
        /// Returns the documents content type. See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-contenttype
        /// </summary>
        public abstract string ContentType { get; }

        /// <summary>
        /// Returns the child of the document that is a <see cref="DocumentType"/>, and null otherwise.
        /// </summary>
        public Dom.DocumentType DocType
        {
            get
            {
                // Not very efficient, but this works.
                return this.OfType<Dom.DocumentType>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the Element that is the root element of the document (for example, the <html> element for HTML documents).
        /// </summary>
        public Dom.Element DocumentElement
        {
            get
            {
                // There is only one element child. See: http://www.w3.org/TR/2015/REC-dom-20151119/#node-tree
                return this.FirstElementChild;
            }
        }

        /// <summary>
        /// Returns the document location URL.
        /// This is the same as <see cref="Url"/>.
        /// </summary>
        public string DocumentUri
        {
            get { return this.Loction; }
        }

        /// <summary>
        /// Returns the <see cref="DomImplementation"/> associated with this document.
        /// </summary>
        public Dom.DomImplementation Implementation { get; private set; }

        /// <summary>
        /// Not Implemented. See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-origin
        /// </summary>
        public string Origin
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the document location URL.
        /// This is the same as <see cref="DocumentUri"/>.
        /// </summary>
        public string Url
        {
            get { return this.Loction; }
        }

        /// <summary>
        /// Adopts a node. The node and its subtree is removed from the document it's in (if any), and its ownerDocument
        /// is changed to the current document. The node can then be inserted into the current document.
        /// </summary>
        /// <param name="node">A node from another document to be adopted.</param>
        /// <returns>
        /// The adopted node that now has this document as its <see cref="Node.OwnerDocument"/>. The node's
        /// <see cref="Node.ParentNode"/> is null, since it has not yet been inserted into the document tree.
        /// Note that the return value is the same as the given <paramref name="node"/>.
        /// </returns>
        public Dom.Node AdoptNode(Dom.Node node)
        {
            Contract.RequiresNotNull(node, nameof(node));

            // 1. If node is a document, throw a "NotSupportedError" exception.
            if (node is Document)
                throw new Dom.Exceptions.NotSupportedException();

            // 2. Adopt node into the context object.
            ((Node)node).Adopt(this);

            // 3. Return node.
            return node;
        }

        /// <summary>
        /// Creates a new comment node, and returns it.
        /// </summary>
        /// <param name="data">Data to be added to the Comment.</param>
        /// <returns>A new Comment node with the given data.</returns>
        public Dom.Comment CreateComment(string data)
        {
            Contract.RequiresNotNull(data, nameof(data));

            return new Comment(this, data);
        }

        /// <summary>
        /// Creates a new empty DocumentFragment.
        /// </summary>
        /// <returns>A new empty DocumentFragment.</returns>
        public Dom.DocumentFragment CreateDocumentFragment()
        {
            return new DocumentFragment(this);
        }

        /// <summary>
        /// Creates the HTML element specified by <paramref name="localName"/>.
        /// </summary>
        /// <param name="localName">Specifies the type of element to be created. This is converted to lowercase.</param>
        /// <returns>The new Element.</returns>
        public Dom.Element CreateElement(string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-createelement

            // 1. If localName does not match the Name production, throw an "InvalidCharacterError" exception.
            if (!localName.IsName())
                throw new Dom.Exceptions.InvalidCharacterException();

            // 2. If the context object is an HTML document, let localName be converted to ASCII lowercase.
            if (this is HtmlDocument)
                localName = localName.ToLower();

            // 3. Let interface be the element interface for localName and the HTML namespace.
            // 4. Return a new element that implements interface, with no attributes, namespace set to the HTML namespace,
            //      local name set to localName, and node document set to the context object.
            return this.CreateElement(Html5.Namespaces.Html, null, localName);
        }

        /// <summary>
        /// Creates an element with the specified namespace URI and qualified name.
        /// </summary>
        /// <param name="namespace">The namespace URI to associate with the element.</param>
        /// <param name="qualifiedName">A string that specifies the type of element to be created.</param>
        /// <returns>The new Element.</returns>
        public Dom.Element CreateElementNS(string @namespace, string qualifiedName)
        {
            Contract.RequiresNotNull(qualifiedName, nameof(qualifiedName));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-createelementns

            // The createElementNS(namespace, qualifiedName) method must run these steps:
            // 1. If namespace is the empty string, set it to null.
            if ((@namespace != null) && (@namespace.Length == 0))
                @namespace = null;

            // 2. If qualifiedName does not match the Name production, throw an "InvalidCharacterError" exception.
            if (!qualifiedName.IsName())
                throw new Dom.Exceptions.InvalidCharacterException();

            // 3. If qualifiedName does not match the QName production, throw a "NamespaceError" exception.
            if (!qualifiedName.IsQName())
                throw new Dom.Exceptions.NamespaceException();

            // 4. If qualifiedName contains a ":" (U+003E), then split the string on it and let prefix be the part
            //    before and localName the part after.Otherwise, let prefix be null and localName be qualifiedName.
            string prefix;
            string localName;
            int idx = qualifiedName.IndexOf(':');
            if (idx != -1)
            {
                prefix = qualifiedName.Substring(0, idx);
                localName = qualifiedName.Substring(idx + 1);
            }
            else
            {
                prefix = null;
                localName = qualifiedName;
            }

            // 5. If prefix is not null and namespace is null, throw a "NamespaceError" exception.
            if ((prefix != null) && (@namespace == null))
                throw new Dom.Exceptions.NamespaceException();

            // 6. If prefix is "xml" and namespace is not the XML namespace, throw a "NamespaceError" exception.
            if ((prefix == "xml") && (@namespace != Namespaces.Xml))
                throw new Dom.Exceptions.NamespaceException();

            // 7. If qualifiedName or prefix is "xmlns" and namespace is not the XMLNS namespace, throw a "NamespaceError" exception.
            if (((qualifiedName == "xmlns") || (prefix == "xmlns")) && (@namespace != Namespaces.Xml))
                throw new Dom.Exceptions.NamespaceException();

            // 8. If namespace is the XMLNS namespace and neither qualifiedName nor prefix is "xmlns", throw a "NamespaceError" exception.
            if ((@namespace == Namespaces.Xml) && ((qualifiedName == "xmlns") || (prefix == "xmlns")))
                throw new Dom.Exceptions.NamespaceException();

            // 9. Let interface be the element interface for localName and namespace.
            // 10. Return a new element that implements interface, with no attributes, namespace set to namespace, namespace
            //     prefix set to prefix, local name set to localName, and node document set to the context object.
            return this.CreateElement(@namespace, prefix, localName);
        }

        protected abstract Element CreateElement(string namespaceUri, string prefix, string localName);

        /// <summary>
        /// Creates a new processing instruction node, and returns it.
        /// </summary>
        /// <param name="target">The target part of the processing instruction node.</param>
        /// <param name="data">The data to be added to the data within the node.</param>
        /// <returns>A new processing instruction node.</returns>
        public ProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            Contract.RequiresNotNull(target, nameof(target));
            Contract.RequiresNotNull(data, nameof(data));

            throw new NotImplementedException(); // We don't need this for HTML.
        }

        /// <summary>
        /// Creates a new Text node.
        /// </summary>
        /// <param name="data">Data to be put in the text node.</param>
        /// <returns>A new Text node with the given data.</returns>
        public Dom.Text CreateTextNode(string data)
        {
            Contract.RequiresNotNull(data, nameof(data));

            return new Text(this, data);
        }

        /// <summary>
        /// Returns the first element within node's descendants whose ID is elementId.
        /// </summary>
        /// <param name="elementId">The requested element's ID.</param>
        /// <returns>
        /// The getElementById(elementId) method must return the first element, in tree order,
        /// within context object's descendants, whose ID is elementId, and null if there is no such element otherwise.
        /// </returns>
        public Dom.Element GetElementById(string elementId)
        {
            if (String.IsNullOrEmpty(elementId))
                return null;

            return this.FindElement(elem => elem.Id == elementId);
        }

        public HtmlCollection GetElementsByClassName(string className)
        {
            Contract.RequiresNotNull(className, nameof(className));

            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagName(string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a copy of a node from an external document that can be inserted into the current document.
        /// </summary>
        /// <param name="node">The node from another document to be imported.</param>
        /// <param name="deep"> boolean, indicating whether the descendants of the imported node need to be imported.</param>
        /// <returns>
        /// The new node that is imported into the document. The new node's parentNode is null,
        /// since it has not yet been inserted into the document tree.
        /// </returns>
        public Dom.Node ImportNode(Dom.Node node, bool deep = false)
        {
            Contract.RequiresNotNull(node, nameof(node));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-document-importnode

            // The importNode(node, deep) method must run these steps:
            // 1. If node is a document, throw a "NotSupportedError" exception.
            // 2. Return a clone of node, with context object and the clone children flag set if deep is true.
            throw new NotImplementedException();
        }

        #endregion
    }
}
