using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-text
    public interface Text : CharacterData
    {
        Text SplitText(int offset);

        /// <summary>
        /// Returns the full text of all Text nodes logically adjacent to this node.
        /// The text is concatenated in document order.  This allows to specify any
        /// text node and obtain all adjacent text as a single string.
        /// </summary>
        string WholeText { get; }
    }
}
