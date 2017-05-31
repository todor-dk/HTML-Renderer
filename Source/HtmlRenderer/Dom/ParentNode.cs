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
    public interface ParentNode
    {
        /// <summary>
        /// Returns the child elements.
        /// The children attribute must return an HTMLCollection collection rooted at the context object matching only element children.
        /// </summary>
        HtmlCollection Children { get; }

        /// <summary>
        /// The firstElementChild attribute must return the first child that is an element, and null otherwise.
        /// </summary>
        Element FirstElementChild { get; }

        /// <summary>
        /// The lastElementChild attribute must return the last child that is an element, and null otherwise.
        /// </summary>
        Element LastElementChild { get; }

        /// <summary>
        /// The childElementCount attribute must return the number of children of the context object that are elements.
        /// </summary>
        int ChildElementCount { get; }
    }
}
