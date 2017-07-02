using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class IdAttr : NormalAttr
    {
        public IdAttr(string localName, string value)
            : base(localName, value)
        {
        }
    }
}
