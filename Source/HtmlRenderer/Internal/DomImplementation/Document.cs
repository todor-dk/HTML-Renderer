using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class Document : ParentNode, Dom.Document
    {
        private readonly BrowsingContext BrowsingContext;

        public Document(BrowsingContext browsingContext, string baseUri)
            : base(null)
        {
            Contract.RequiresNotNull(browsingContext, nameof(browsingContext));

            this.BrowsingContext = browsingContext;
            this._BaseUri = baseUri;
            this.DomImplementation = new DomImplementation(this);
        }

        private string _BaseUri;

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
            get { return this._BaseUri; }
        }

        public string CharacterSet { get; private set; }

        public string CompatMode { get; private set; }

        public string ContentType { get; private set; }

        public Dom.DocumentType DocType { get; private set; }

        public Dom.Element DocumentElement { get; private set; }

        public string DocumentUri { get; private set; }

        public Dom.DomImplementation DomImplementation { get; private set; }

        #region Node interface overrides

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

        public string Origin { get; private set; }

        public QuirksMode QuirksMode { get; private set; }

        public string Url { get; private set; }

        public Dom.Node AdoptNode(Dom.Node node)
        {
            // 1. If node is a document, throw a "NotSupportedError" exception.
            if (node is Document)
                throw new Dom.Exceptions.NotSupportedException();

            // 2. Adopt node into the context object.
            ((Node)node).Adopt(this);

            // 3. Return node.
            return node;
        }

        public Dom.Comment CreateComment(string data)
        {
            return new Comment(this, data);
        }

        public Dom.DocumentFragment CreateDocumentFragment()
        {
            throw new NotImplementedException();
        }

        public Dom.Element CreateElement(string localName)
        {
            throw new NotImplementedException();
        }

        public Dom.Element CreateElementNS(string @namespace, string qualifiedName)
        {
            throw new NotImplementedException();
        }

        public ProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            throw new NotImplementedException();
        }

        public Dom.Text CreateTextNode(string data)
        {
            return new Text(this, data);
        }

        public Dom.Element GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByClassName(string className)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagName(string localName)
        {
            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        public Dom.Node ImportNode(Dom.Node node, bool deep = false)
        {
            throw new NotImplementedException();
        }
    }
}
