using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Html5;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class HtmlElement : Element
    {
        public HtmlElement(Document document)
            : base(document)
        {
        }

        /// <summary>
        /// Returns the namespace URI of the element, or null if the element is not in a namespace.
        /// </summary>
        public override string NamespaceUri
        {
            get { return Namespaces.Html; }
        }

        /// <summary>
        /// Returns the namespace prefix of the specified element, or null if no prefix is specified.
        /// </summary>
        public override string Prefix
        {
            get { return null; }
        }
    }
}
