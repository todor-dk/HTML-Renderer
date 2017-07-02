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
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-htmlcollection
    /// </summary>
    public interface HtmlCollection : IReadOnlyList<Element>
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Returns the number of elements in the collection.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Returns the element with index index from the collection. The elements are sorted in tree order.
        /// </summary>
        /// <param name="index">The requested index.</param>
        /// <returns>Returns the element with index index from the collection or null if there is no index'th element in the collection.</returns>
        Element Item(int index);

        /// <summary>
        /// Returns the first element with ID or name name from the collection.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Element NamedItem(string name);
    }
}
