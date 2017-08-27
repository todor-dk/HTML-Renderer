using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceParentNode : ReferenceNode, ParentNode
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

        #region ParentNode interface

        HtmlCollection ParentNode.Children => this.Children;

        Element ParentNode.FirstElementChild => this.FirstElementChild;

        Element ParentNode.LastElementChild => this.LastElementChild;

        int ParentNode.ChildElementCount => this.ChildElementCount ?? -1;

        #endregion
    }
}
