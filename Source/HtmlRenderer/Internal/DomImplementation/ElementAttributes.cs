using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal struct ElementAttributes
    {
        private object Content;

        internal int GetCount()
        {
            return 0;
        }

        internal IEnumerator<Dom.Attr> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
