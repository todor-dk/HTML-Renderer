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
