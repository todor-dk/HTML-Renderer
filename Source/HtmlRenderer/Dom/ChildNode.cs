using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-childnode
    /// </summary>
    public interface ChildNode
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Removes this node from its parent children list.
        /// </summary>
        void Remove();
    }
}
