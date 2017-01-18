using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-1950641247
    public abstract class Node
    {
        public abstract string NodeName { get; }

        public abstract string NodeValue { get; }

        public abstract NodeType NodeType { get; }

        public Node ParentNode { get; private set; }

        public readonly IReadOnlyList<Node> ChildNodes = new List<Node>();

        public Node FirstChild { get; private set; }

        public Node LastChild { get; private set; }

        public Node PreviousSibling { get; private set; }

        public Node NextSibling { get; private set; }

        public readonly IReadOnlyList<Attr> Attributes = new List<Attr>();

        public Document OwnerDocument { get; private set; }

        public Node InsertBefore(Node newChild, Node referenceChild)
        {
            throw new NotImplementedException();
        }

        public Node ReplaceChild(Node newChild, Node oldChild)
        {
            throw new NotImplementedException();
        }

        public Node RemoveChild(Node oldChild)
        {
            throw new NotImplementedException();
        }

        public Node AppendChild(Node newChild)
        {
            throw new NotImplementedException();
        }

        public bool HasChildNodes()
        {
            return this.ChildNodes.Count != 0;
        }

        public void Normalize()
        {
            throw new NotImplementedException();
        }
    }
}
