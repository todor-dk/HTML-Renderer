using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocumentType : ReferenceNode
    {
        public ReferenceDocumentType(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.Name = reader.ReadString("Name");
            this.PublicId = reader.ReadString("PublicId");
            this.SystemId = reader.ReadString("SystemId");
        }

        public string Name { get; private set; }

        public string PublicId { get; private set; }

        public string SystemId { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitDocumentType(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithDocumentType(other as ReferenceDocumentType, context);
        }

        internal bool CompareWithDocumentType(ReferenceDocumentType other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithNode(other, context))
                return false;

            if (this.Name != other.Name)
                return false;
            if (this.PublicId != other.PublicId)
                return false;
            if (this.SystemId != other.SystemId)
                return false;

            return true;
        }

        public override bool CompareWith(Node other, CompareContext context)
        {
            return this.CompareWithDocumentType(other as DocumentType, context);
        }

        internal bool CompareWithDocumentType(DocumentType other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithNode(other, context))
                return false;

            if (this.Name != other.Name)
                return false;
            if (this.PublicId != other.PublicId)
                return false;
            if (this.SystemId != other.SystemId)
                return false;

            return true;
        }
    }
}
