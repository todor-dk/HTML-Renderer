using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal class IdAttr : NormalAttr
    {
        public IdAttr(string localName, string value)
            : base(localName, value)
        {
        }
    }
}
