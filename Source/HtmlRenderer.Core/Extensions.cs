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

namespace Scientia.HtmlRenderer
{
    internal static class Extensions
    {
        public static TItem[] With<TItem>(this TItem[] self, params TItem[] elements)
        {
            int len = (self?.Length ?? 0) + (elements?.Length ?? 0);
            TItem[] result = new TItem[len];

            int j = 0;

            if ((self != null) && (self.Length != 0))
            {
                for (int i = 0; i < self.Length; i++)
                {
                    result[j] = self[i];
                    j++;
                }
            }

            if ((elements != null) && (elements.Length != 0))
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    result[j] = elements[i];
                    j++;
                }
            }

            return result;
        }

        public static TItem[] FailIfNull<TItem>(this TItem[] self)
        {
            if (self == null)
                throw new NullReferenceException();

            return self;
        }
    }
}
