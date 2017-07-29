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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer
{
    // TO-DO
    internal static class FutureVersions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CurrentlyIrrelevant()
        {
            // Features in the specification that we've decided not to implement.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImplementDomObservers()
        {
            // Purpose of this is to implement observer events.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PatchExistingRanges()
        {
            // The purpose of this is to patch existing Range objects that may be alive and in use.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TemplatesNotImplemented()
        {
            // The <TEMPLATE> feature is currently not implemented.
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ScriptingNotImplemented()
        {
            // The <SCRIPT> tags are currently not implemented.
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FramesetsNotImplemented()
        {
            // The <FRAMESET> tags are currently not implemented.
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MathmlNotImplemented()
        {
            // The MathML tags are currently not implemented.
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SvgNotImplemented()
        {
            // The SVG tags are currently not implemented.
            throw new NotImplementedException();
        }
    }
}
