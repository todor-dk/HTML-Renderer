using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class DocumentFragment : ParentNode, Dom.DocumentFragment
    {
        public DocumentFragment(Document document)
            : base(document)
        {
        }

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return "#document-fragment"; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.DocumentFragment; }
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.GetTextContent(); }
            set { this.SetTextContent(value); }
        }

        public Dom.Element GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }
    }
}
