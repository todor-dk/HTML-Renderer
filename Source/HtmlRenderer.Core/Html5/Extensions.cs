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
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Html5
{
    internal static class Extensions
    {
        public static bool IsInSameSubtree(this Node self, Node node)
        {
            // A node’s home subtree is the subtree rooted at that node’s root element.
            // When a node is in a Document, its home subtree is that Document's tree.
            return self.GetRoot() == node.GetRoot();
        }

        public static Node GetRoot(this Node self)
        {
            Contract.RequiresNotNull(self, nameof(self));

            Node node = self;
            while (true)
            {
                Node parent = node.ParentNode;
                if (parent == null)
                    return node;
                node = parent;
            }
        }

        public static string GetTagName(this Element self)
        {
            Contract.RequiresNotNull(self, nameof(self));

            string qualifiedName;
            if (self.Prefix != null)
                qualifiedName = self.Prefix + ":" + self.LocalName;
            else
                qualifiedName = self.LocalName;

            return qualifiedName;
        }
    }
}
