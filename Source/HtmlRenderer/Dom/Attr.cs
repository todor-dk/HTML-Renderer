using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-attr
    public interface Attr
    {
        string NamespaceUri { get; }

        string Prefix { get; }

        string LocalName { get; }

        string Name { get; }

        string Value { get; set; }

        Element OwnerElement { get; }
    }
}
