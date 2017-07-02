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
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-nodelist
    /// </summary>
    public interface NodeList : IReadOnlyList<Node>
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Returns the number of nodes in the collection.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Returns the node with index index from the collection. The nodes are sorted in tree order.
        /// </summary>
        /// <param name="index">The requested index.</param>
        /// <returns>Returns the node with index index from the collection or null if there is no index'th node in the collection.</returns>
        Node Item(int index);
    }
}
