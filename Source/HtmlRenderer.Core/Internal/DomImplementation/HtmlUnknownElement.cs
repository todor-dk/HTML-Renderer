using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class HtmlUnknownElement : HtmlElement
    {
        public HtmlUnknownElement(Document document, string localName)
            : base(document)
        {
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
    }
}
