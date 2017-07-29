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

using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class InternalDomFactory : DomFactory
    {
        public static readonly InternalDomFactory Current = new InternalDomFactory();

        public override void SetQuirksMode(Dom.Document document, QuirksMode mode)
        {
            Internal.DomImplementation.Document doc = (Internal.DomImplementation.Document)document;
            doc.QuirksMode = mode;
        }
    }
}
