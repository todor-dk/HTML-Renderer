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

        #region Node interface overrides

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

        public Dom.Element GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }
    }
}
