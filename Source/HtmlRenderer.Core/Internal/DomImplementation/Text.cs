using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class Text : CharacterData, Dom.Text
    {
        public Text(Document document, string data = null)
            : base(document, data)
        {
        }

        #region Node interface overrides

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return "#text"; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.Text; }
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

        #region Text interface

        /// <summary>
        /// Returns the full text of all Text nodes logically adjacent to this node.
        /// The text is concatenated in document order.  This allows to specify any
        /// text node and obtain all adjacent text as a single string.
        /// </summary>
        public string WholeText
        {
            get
            {
                Text prev = this.PreviousSibling as Text;
                Text next = this.PreviousSibling as Text;
                if ((prev == null) && (next == null))
                    return this.Data;

                Text start = this;
                while (prev != null)
                {
                    start = prev;
                    prev = prev.PreviousSibling as Text;
                }

                StringBuilder sb = new StringBuilder();
                while (start != null)
                {
                    sb.Append(start.Data ?? String.Empty);
                    start = start.NextSibling as Text;
                }

                return sb.ToString();
            }
        }

        public Dom.Text SplitText(int offset)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
