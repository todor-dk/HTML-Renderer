using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    public interface AttrCollection : NamedNodeMap, IEnumerable<Attr>
    {
        int Count { get; }
    }
}
