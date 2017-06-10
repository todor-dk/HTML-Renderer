using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocument : ReferenceParentNode
    {
        public ReferenceDocument(Persisting.IReader reader, TheArtOfDev.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.Url = reader.ReadString("URL");
            this.DocumentUri = reader.ReadString("DocumentURI");
            this.Origin = reader.ReadString("Origin");
            this.CompatMode = reader.ReadString("CompatMode");
            this.CharacterSet = reader.ReadString("CharacterSet");
            this.ContentType = reader.ReadString("ContentType");

            reader.ReadNode("DocType", node => this.DocType = (ReferenceDocumentType)node);
            reader.ReadElement("DocumentElement", elem => this.DocumentElement = elem);
        }

        public QuirksMode QuirksMode { get; private set; }

        public string Url { get; private set; }

        public string DocumentUri { get; private set; }

        public string Origin { get; private set; }

        public string CompatMode { get; private set; }

        public string CharacterSet { get; private set; }

        public string ContentType { get; private set; }

        public ReferenceDocumentType DocType { get; private set; }

        public ReferenceElement DocumentElement { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitDocument(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithDocument(other as ReferenceDocument, context);
        }

        internal bool CompareWithDocument(ReferenceDocument other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithParentNode(other, context))
                return false;

            if (this.CharacterSet != other.CharacterSet)
                return false;
            if (this.CompatMode != other.CompatMode)
                return false;
            if (this.ContentType != other.ContentType)
                return false;
            if (!this.DocType.Compare(other.DocType, context))
                return false;
            if (!this.DocumentElement.Compare(other.DocumentElement, context))
                return false;
            if (this.DocumentUri != other.DocumentUri)
                return false;
            if (this.Origin != other.Origin)
                return false;
            if (this.QuirksMode != other.QuirksMode)
                return false;
            if (this.Url != other.Url)
                return false;

            return true;
        }
    }
}
