using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceCharacterData : ReferenceNode
    {
        public ReferenceCharacterData(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
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
            if (!this.NextElementSibling.CompareReference(other.NextElementSibling, context))
                return false;
            if (!this.PreviousElementSibling.CompareReference(other.PreviousElementSibling, context))
                return false;

            return true;
        }

        internal bool CompareWithCharacterData(CharacterData other, CompareContext context)
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
            if (!context.IgnoreChildrenPropertiesExceptForElement)
            {
                if (!this.NextElementSibling.CompareDom(other.NextElementSibling, context))
                    return false;
                if (!this.PreviousElementSibling.CompareDom(other.PreviousElementSibling, context))
                    return false;
            }

            return true;
        }
    }
}
