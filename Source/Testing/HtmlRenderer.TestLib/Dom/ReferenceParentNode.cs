using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceParentNode : ReferenceNode
    {
        public ReferenceParentNode()
        {
            this.Children = new ReferenceHtmlCollection();
        }

        public ReferenceParentNode(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.Children = new ReferenceHtmlCollection();
            this.ChildElementCount = reader.ReadNullableInt("ChildElementCount");
            reader.ReadElement("FirstElementChild", elem => this.FirstElementChild = elem);
            reader.ReadElement("LastElementChild", elem => this.LastElementChild = elem);
            reader.ReadNodeList("Children", list => this.Children.AddRange((list ?? Array.Empty<ReferenceElement>()).OfType<ReferenceElement>()));
        }

        public ReferenceHtmlCollection Children { get; private set; }

        public ReferenceElement FirstElementChild { get; private set; }

        public ReferenceElement LastElementChild { get; private set; }

        public int? ChildElementCount { get; private set; }

        internal bool CompareWithParentNode(ReferenceParentNode other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithNode(other, context))
                return false;

            if (this.ChildElementCount != other.ChildElementCount)
                return false;
            if (!this.Children.CompareReferenceCollection(other.Children, context))
                return false;
            if (!this.FirstElementChild.CompareReference(other.FirstElementChild, context))
                return false;
            if (!this.LastElementChild.CompareReference(other.LastElementChild, context))
                return false;

            return true;
        }

        internal bool CompareWithParentNode(ParentNode other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithNode((Node)other, context))
                return false;

            if (!context.IgnoreChildrenPropertiesExceptForElement || (this is ReferenceElement))
            {
                if (this.ChildElementCount != other.ChildElementCount)
                    return false;
                if (!this.Children.CompareDomCollection(other.Children, context))
                    return false;
                if (!this.FirstElementChild.CompareDom(other.FirstElementChild, context))
                    return false;
                if (!this.LastElementChild.CompareDom(other.LastElementChild, context))
                    return false;
            }

            return true;
        }
    }
}
