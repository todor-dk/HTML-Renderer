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
    public static class Extensions
    {
        /// <summary>
        /// Visit the given node <paramref name="self"/> and all its descendant nodes in tree order.
        /// </summary>
        /// <param name="self">The object that accepts the visitor.</param>
        /// <param name="visitor">Visitor function that is visited by each object. If this returns true, visiting stops.</param>
        public static bool Accept(this Node self, Func<Node, bool> visitor)
        {
            Contract.RequiresNotNull(visitor, nameof(visitor));

            if (self == null)
                return false;

            if (visitor(self))
                return true;

            if (self.FirstChild.Accept(visitor))
                return true;

            return self.NextSibling.Accept(visitor);
        }

        /// <summary>
        /// Visit the given element <paramref name="self"/> and all its descendant element in tree order.
        /// </summary>
        /// <param name="self">The object that accepts the visitor.</param>
        /// <param name="visitor">Visitor function that is visited by each object. If this returns true, visiting stops.</param>
        public static bool Accept(this Element self, Func<Element, bool> visitor)
        {
            Contract.RequiresNotNull(visitor, nameof(visitor));

            if (self == null)
                return false;

            if (visitor(self))
                return true;

            if (self.FirstElementChild.Accept(visitor))
                return true;

            return self.NextElementSibling.Accept(visitor);
        }

        /// <summary>
        /// Visit in tree order all the descendant element of the given node <paramref name="self"/>.
        /// </summary>
        /// <param name="self">The object that accepts the visitor.</param>
        /// <param name="visitor">Visitor function that is visited by each object. If this returns true, visiting stops.</param>
        public static bool Accept(this ParentNode self, Func<Element, bool> visitor)
        {
            Contract.RequiresNotNull(self, nameof(self));
            Contract.RequiresNotNull(visitor, nameof(visitor));

            Element elem = self.FirstElementChild;
            while (elem != null)
            {
                if (elem.Accept(visitor))
                    return true;
                elem = elem.NextElementSibling;
            }

            return false;
        }

        /// <summary>
        /// Visit the given node <paramref name="self"/> and all its descendant nodes in tree order
        /// invoking the <paramref name="predicate"/> delegate and returning the first node where
        /// the predicate evaluates to true.
        /// </summary>
        /// <param name="self">The root node.</param>
        /// <param name="predicate">Predicate to evaluate for each node.</param>
        /// <returns>The first node in tree order where <paramref name="predicate"/> evaluates to true or null otherwise.</returns>
        public static Node FindNode(this Node self, Predicate<Node> predicate)
        {
            Contract.RequiresNotNull(predicate, nameof(predicate));

            if (self == null)
                return null;

            if (predicate(self))
                return self;

            return self.FirstChild.FindNode(predicate) ?? self.NextSibling.FindNode(predicate);
        }

        /// <summary>
        /// Visit the given element <paramref name="self"/> and all its descendant element in tree order
        /// invoking the <paramref name="predicate"/> delegate and returning the first element where
        /// the predicate evaluates to true.
        /// </summary>
        /// <param name="self">The root element.</param>
        /// <param name="predicate">Predicate to evaluate for each element.</param>
        /// <returns>The first element in tree order where <paramref name="predicate"/> evaluates to true or null otherwise.</returns>
        public static Element FindElement(this Element self, Predicate<Element> predicate)
        {
            Contract.RequiresNotNull(predicate, nameof(predicate));

            if (self == null)
                return null;

            if (predicate(self))
                return self;

            return self.FirstElementChild.FindElement(predicate) ?? self.NextElementSibling.FindElement(predicate);
        }

        /// <summary>
        /// Visit in tree order all the descendant element of the given element <paramref name="self"/>
        /// invoking the <paramref name="predicate"/> delegate and returning the first element where
        /// the predicate evaluates to true.
        /// </summary>
        /// <param name="self">The root element.</param>
        /// <param name="predicate">Predicate to evaluate for each element.</param>
        /// <returns>The first element in tree order where <paramref name="predicate"/> evaluates to true or null otherwise.</returns>
        public static Element FindElement(this ParentNode self, Predicate<Element> predicate)
        {
            Contract.RequiresNotNull(self, nameof(self));
            Contract.RequiresNotNull(predicate, nameof(predicate));

            Element elem = self.FirstElementChild;
            while (elem != null)
            {
                Element result = elem.FindElement(predicate);
                if (result != null)
                    return result;
                elem = elem.NextElementSibling;
            }

            return null;
        }
    }
}
