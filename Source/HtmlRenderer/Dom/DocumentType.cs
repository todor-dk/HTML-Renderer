using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-documenttype
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface DocumentType : Node, ChildNode
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        string Name { get; }

        string PublicId { get; }

        string SystemId { get; }
    }
}
