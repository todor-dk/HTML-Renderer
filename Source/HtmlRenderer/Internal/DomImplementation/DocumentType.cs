using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class DocumentType : Node, Dom.DocumentType
    {
        public DocumentType(Document document)
            : base(document)
        {
        }

        public string Name { get; private set; }

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

        public string PublicId { get; private set; }

        public string SystemId { get; private set; }

        public void Remove()
        {
            throw new NotImplementedException();
        }
    }
}
