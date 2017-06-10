using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom.Persisting
{
    public abstract class ReaderBase : IReader
    {
        protected readonly Dictionary<int, ReferenceNode> NodeMap = new Dictionary<int, ReferenceNode>();

        protected readonly List<Tuple<int, Action<ReferenceNode>>> ResolveActions = new List<Tuple<int, Action<ReferenceNode>>>();

        protected readonly List<Tuple<int[], Action<ReferenceNode[]>>> ResolveListActions = new List<Tuple<int[], Action<ReferenceNode[]>>>();

        protected ReferenceNode Root;

        public abstract ReferenceAttr ReadAttrib();

        public void ReadElement(string name, Action<ReferenceElement> setter)
        {
            this.ReadNode(name, node => setter((ReferenceElement)node));
        }

        public abstract int ReadInt(string name);
        public abstract void ReadNode(string name, Action<ReferenceNode> setter);
        public abstract void ReadNodeList(string name, Action<ReferenceNode[]> setter);
        public abstract int? ReadNullableInt(string name);
        public abstract string ReadString(string name);
        public abstract string[] ReadStringList(string name);

        protected ReferenceNode ReadNode(int type)
        {
            if (type == 1)
                return new ReferenceElement(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.Element);
            else if (type == 3)
                return new ReferenceText(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.Text);
            else if (type == 8)
                return new ReferenceComment(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.Comment);
            else if (type == 9)
                return new ReferenceDocument(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.Document);
            else if (type == 10)
                return new ReferenceDocumentType(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.DocumentType);
            else if (type == 11)
                return new ReferenceDocumentFragment(this, TheArtOfDev.HtmlRenderer.Dom.NodeType.DocumentFragment);
            else
                throw new NotImplementedException();
        }

        protected void ResolveNode(int? id, Action<ReferenceNode> setter)
        {
            if (setter == null)
                return;

            if (id == null)
            {
                setter?.Invoke(null);
            }
            else
            {
                ReferenceNode node;
                this.NodeMap.TryGetValue(id.Value, out node);
                if (node != null)
                    setter(node);
                else
                    this.ResolveActions.Add(new Tuple<int, Action<ReferenceNode>>(id.Value, setter));
            }
        }

        protected void ResolveNodes()
        {
            foreach (Tuple<int, Action<ReferenceNode>> tuple in this.ResolveActions)
            {
                ReferenceNode node = this.NodeMap[tuple.Item1];
                tuple.Item2(node);
            }

            foreach (Tuple<int[], Action<ReferenceNode[]>> tuple in this.ResolveListActions)
            {
                ReferenceNode[] nodes = tuple.Item1.Select(id => this.NodeMap[id]).ToArray();
                tuple.Item2(nodes);
            }
        }

    }
}
