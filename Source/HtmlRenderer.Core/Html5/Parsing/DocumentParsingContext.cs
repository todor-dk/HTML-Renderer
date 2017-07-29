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

namespace Scientia.HtmlRenderer.Html5.Parsing
{
    public class DocumentParsingContext : ParsingContext
    {
        public override bool IsFragmentParsing
        {
            get { return false; }
        }

        public override bool IsIFrameSource
        {
            get { return false; }
        }

        public override Element FragmentContextElement
        {
            get { return null; }
        }

        public DocumentParsingContext(string url, string characterSet)
            : base(url, characterSet)
        {
        }
    }
}
