using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceText : ReferenceCharacterData
    {
        public ReferenceText(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.WholeText = reader.ReadString("WholeText");
        }

        public string WholeText { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitText(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithText(other as ReferenceText, context);
        }


        internal bool CompareWithText(ReferenceText other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithCharacterData(other, context))
                return false;

            if (this.WholeText != other.WholeText)
                return false;

            return true;
        }

        public override bool CompareWith(Node other, CompareContext context)
        {
            return this.CompareWithText(other as Text, context);
        }


        internal bool CompareWithText(Text other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithCharacterData(other, context))
                return false;

            if (this.WholeText != other.WholeText)
                return false;

            return true;
        }
    }
}
