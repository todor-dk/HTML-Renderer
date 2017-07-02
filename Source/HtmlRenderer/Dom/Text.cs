using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-text
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface Text : CharacterData
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
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
