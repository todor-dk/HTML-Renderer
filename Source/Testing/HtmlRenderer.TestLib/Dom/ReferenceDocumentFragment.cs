using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocumentFragment : ReferenceParentNode, DocumentFragment
    {
        public ReferenceDocumentFragment(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
        }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitDocumentFragment(this);
        }
        
        #region DocumentType interface

        Element NonElementParentNode.GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
