using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceCharacterData : ReferenceNode
    {
        public ReferenceCharacterData(Persisting.IReader reader, TheArtOfDev.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            reader.ReadElement("PreviousElementSibling", elem => this.PreviousElementSibling = elem);
            reader.ReadElement("NextElementSibling", elem => this.NextElementSibling = elem);
            this.Data = reader.ReadString("Data");
            this.Length = reader.ReadInt("Length");
        }

        public string Data { get; private set; }

        public int Length { get; private set; }
        
        public ReferenceElement PreviousElementSibling { get; private set; }
        
        public ReferenceElement NextElementSibling { get; private set; }

        internal bool CompareWithCharacterData(ReferenceCharacterData other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithNode(other, context))
                return false;

            if (this.Data != other.Data)
                return false;
            if (this.Length != other.Length)
                return false;
            if (!this.NextElementSibling.Compare(other.NextElementSibling, context))
                return false;
            if (!this.PreviousElementSibling.Compare(other.PreviousElementSibling, context))
                return false;

            return true;
        }
    }
}
