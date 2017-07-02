using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface AttrCollection : NamedNodeMap, IEnumerable<Attr>
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        int Count { get; }
    }
}
