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
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-nonelementparentnode
    /// </summary>
    public interface ParentNode
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// Returns the child elements.
        /// The children attribute returns an HTMLCollection collection rooted at the context object matching only element children.
        /// </summary>
        HtmlCollection Children { get; }

        /// <summary>
        /// Returns the first child that is an element, and null otherwise.
        /// </summary>
        Element FirstElementChild { get; }

        /// <summary>
        /// Returns the last child that is an element, and null otherwise.
        /// </summary>
        Element LastElementChild { get; }

        /// <summary>
        /// Returns the number of children of the context object that are elements.
        /// </summary>
        int ChildElementCount { get; }
    }
}
