using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceAttrList : List<ReferenceAttr>, AttrCollection
    {
        public int Length => this.Count;

        public Attr GetNamedItem(string name)
        {
            throw new NotImplementedException();
        }

        public Attr GetNamedItemNS(string namespaceUri, string localName)
        {
            throw new NotImplementedException();
        }

        public Attr Item(int index)
        {
            if ((index < 0) || (index >= this.Count))
                return null;
            return this[index];
        }

        IEnumerator<Attr> IEnumerable<Attr>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
