using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class FullAttr : Attr
    {
        private string _Name;

        /// <summary>
        /// The attribute's name.
        /// </summary>
        public override string Name
        {
            get { return this._Name; }
        }

        private string _NamespaceUri;

        /// <summary>
        /// The namespace URI of the attribute, or null if there is no namespace.
        /// </summary>
        public override string NamespaceUri
        {
            get { return this._NamespaceUri; }
        }

        private string _Prefix;

        /// <summary>
        /// The namespace prefix of the attribute, or null if no prefix is specified.
        /// </summary>
        public override string Prefix
        {
            get { return this._Prefix; }
        }

        internal FullAttr(string namespaceUri, string prefix, string localName, string name, string value)
            : base(localName, value)
        {
            this._NamespaceUri = namespaceUri;
            this._Prefix = prefix;
            this._Name = name;
        }
    }
}
