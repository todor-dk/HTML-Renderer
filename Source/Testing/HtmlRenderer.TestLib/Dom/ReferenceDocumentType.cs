using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocumentType : ReferenceNode, DocumentType
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
        
        #region DocumentType interface

        void ChildNode.Remove()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
