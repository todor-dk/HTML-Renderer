using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceComment : ReferenceCharacterData
    {
        public ReferenceComment(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
        }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitComment(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithComment(other as ReferenceComment, context);
        }

        internal bool CompareWithComment(ReferenceComment other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithCharacterData(other, context))
                return false;

            return true;
        }

        public override bool CompareWith(Node other, CompareContext context)
        {
            return this.CompareWithComment(other as Comment, context);
        }

        internal bool CompareWithComment(Comment other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithCharacterData(other, context))
                return false;

            return true;
        }
    }
}
