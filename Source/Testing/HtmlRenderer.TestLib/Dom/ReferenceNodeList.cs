using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceNodeList : List<ReferenceNode>, NodeList
    {
        Node IReadOnlyList<Node>.this[int index] => this[index];

        public int Length => this.Count;

        public Node Item(int index)
        {
            if ((index < 0) || (index >= this.Count))
                return null;
            return this[index];
        }

        IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
