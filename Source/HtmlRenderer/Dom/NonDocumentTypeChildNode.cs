using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-nondocumenttypechildnode
    /// </summary>
    public interface NonDocumentTypeChildNode
    {
        /// <summary>
        /// Returns the first preceding sibling that is an element, and null otherwise.
        /// </summary>
        Element PreviousElementSibling { get; }

        /// <summary>
        /// Returns the first following sibling that is an element, and null otherwise.
        /// </summary>
        Element NextElementSibling { get; }
    }
}
