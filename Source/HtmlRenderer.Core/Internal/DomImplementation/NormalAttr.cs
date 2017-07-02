using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class NormalAttr : Attr
    {
        internal NormalAttr(string localName, string value)
            : base(localName, value)
        {
        }

        /// <summary>
        /// The attribute's name.
        /// </summary>
        public override string Name
        {
            get { return this.LocalName; }
        }

        /// <summary>
        /// The namespace URI of the attribute, or null if there is no namespace.
        /// </summary>
        public override string NamespaceUri
        {
            get { return null; }
        }

        /// <summary>
        /// The namespace prefix of the attribute, or null if no prefix is specified.
        /// </summary>
        public override string Prefix
        {
            get { return null; }
        }
    }
}
