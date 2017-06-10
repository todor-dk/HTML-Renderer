using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocumentFragment : ReferenceParentNode
    {
        public ReferenceDocumentFragment(Persisting.IReader reader, TheArtOfDev.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
        }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitDocumentFragment(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithDocumentFragment(other as ReferenceDocumentFragment, context);
        }

        internal bool CompareWithDocumentFragment(ReferenceDocumentFragment other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithParentNode(other, context))
                return false;

            return true;
        }
    }
}
