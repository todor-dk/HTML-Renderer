using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-documenttype
    public interface DocumentType : Node, ChildNode
    {
        string Name { get; }

        string PublicId { get; }

        string SystemId { get; }
    }
}
