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

namespace Scientia.HtmlRenderer.Html5
{
    // See: http://www.w3.org/TR/html51/infrastructure.html#svg-namespace
    internal static class Namespaces
    {
        public const string Html = "http://www.w3.org/1999/xhtml";

        public const string MathMl = "http://www.w3.org/1998/Math/MathML";

        public const string Svg = "http://www.w3.org/2000/svg";

        public const string Xlink = "http://www.w3.org/1999/xlink";

        public const string Xml = "http://www.w3.org/XML/1998/namespace";

        public const string Xmlns = "http://www.w3.org/2000/xmlns/";
    }
}
