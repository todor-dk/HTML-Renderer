/*
 * **************************************************************************
 *
 * Copyright (c) Todor Todorov / Scientia Software. 
 *
 * This source code is subject to terms and conditions of the 
 * license agreement found in the project directory. 
 * See: $(ProjectDir)\LICENSE.txt ... in the root of this project.
 * By using this source code in any fashion, you are agreeing 
 * to be bound by the terms of the license agreement.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Dom
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
