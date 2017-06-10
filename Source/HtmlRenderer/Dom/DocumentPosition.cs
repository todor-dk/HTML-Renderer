using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-node
    /// </summary>
    [Flags]
    public enum DocumentPosition
    {
        None = 0,

        /// <summary>
        /// Set when node and other are not in the same tree.
        /// </summary>
        Disconnected = 1,

        /// <summary>
        /// Set when other is preceding node.
        /// </summary>
        Preceding = 2,

        /// <summary>
        /// Set when other is following node.
        /// </summary>
        Following = 4,

        /// <summary>
        /// Set when other is an ancestor of node.
        /// </summary>
        Contains = 8,

        /// <summary>
        /// Set when other is a descendant of node.
        /// </summary>
        ContainedBy = 16,

        ImplementationSpecific = 32
    }
}
