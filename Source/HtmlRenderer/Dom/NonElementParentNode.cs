using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-nonelementparentnode
    /// </summary>
    public interface NonElementParentNode
    {
        /// <summary>
        /// Returns the first element within node's descendants whose ID is elementId.
        /// </summary>
        /// <param name="elementId">The requested element's ID.</param>
        /// <returns>
        /// The getElementById(elementId) method must return the first element, in tree order,
        /// within context object's descendants, whose ID is elementId, and null if there is no such element otherwise.
        /// </returns>
        Element GetElementById(string elementId);
    }
}
