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
