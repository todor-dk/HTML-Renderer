using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceHtmlCollection : List<ReferenceElement>, HtmlCollection
    {
        Element IReadOnlyList<Element>.this[int index] => this[index];

        public int Length => this.Count;

        public Element Item(int index)
        {
            if ((index < 0) || (index >= this.Count))
                return null;
            return this[index];
        }

        public Element NamedItem(string name)
        {
            throw new NotImplementedException();
        }

        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
