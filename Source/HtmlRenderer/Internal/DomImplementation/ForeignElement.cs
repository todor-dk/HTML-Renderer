using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class ForeignElement : Element
    {
        public ForeignElement(Document document, string namespaceUri, string prefix, string localName)
            : base(document)
        {
            this._NamespaceUri = namespaceUri;
            this._Prefix = prefix;
            this._LocalName = localName;
        }

        private readonly string _LocalName;

        /// <summary>
        /// Returns the local part of the qualified name of an element.
        /// </summary>
        public override string LocalName
        {
            get { return this._LocalName; }
        }

        private readonly string _NamespaceUri;

        /// <summary>
        /// Returns the namespace URI of the element, or null if the element is not in a namespace.
        /// </summary>
        public override string NamespaceUri
        {
            get { return this._NamespaceUri; }
        }

        private readonly string _Prefix;

        /// <summary>
        /// Returns the namespace prefix of the specified element, or null if no prefix is specified.
        /// </summary>
        public override string Prefix
        {
            get { return this._Prefix; }
        }
    }
}
