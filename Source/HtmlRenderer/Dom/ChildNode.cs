using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-childnode
    /// </summary>
    public interface ChildNode
    {
        /// <summary>
        /// Removes node.
        /// </summary>
        void Remove();
    }
}
