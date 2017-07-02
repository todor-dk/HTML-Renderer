using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class DocumentType : Node, Dom.DocumentType
    {
        public DocumentType(Document document, string name, string publicId, string systemId)
            : base(document)
        {
            this.Name = name;
            this.PublicId = publicId;
            this.SystemId = systemId;
        }

        #region Node interface overrides

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return this.Name; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.DocumentType; }
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

        #region DocumentType interface

        public string Name { get; private set; }

        public string PublicId { get; private set; }

        public string SystemId { get; private set; }

        #endregion

        #region ChildNode interface

        /// <summary>
        /// Removes this node from its parent children list.
        /// </summary>
        public void Remove()
        {
            this._ParentNode?.RemoveNode(this);
        }

        #endregion
    }
}
