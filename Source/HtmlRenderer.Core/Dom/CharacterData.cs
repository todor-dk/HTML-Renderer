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
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-characterdata
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface CharacterData : Node, NonDocumentTypeChildNode, ChildNode
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// The textual data contained in this object
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// The size of the string contained in <see cref="Data"/>.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Returns a string containing the part of <see cref="Data"/> of the specified
        /// <paramref name="length"/> and starting at the specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">Specifies where to begin extracting characters. Start value starts at zero.</param>
        /// <param name="length">Specifies how many characters to extract.</param>
        /// <returns></returns>
        string SubstringData(int offset, int length);

        /// <summary>
        /// Appends the given string to the <see cref="Data"/> string;
        /// when this method returns, <see cref="Data"/> contains the concatenated string.
        /// </summary>
        /// <param name="data">The string to add to the comment node.</param>
        void AppendData(string data);

        /// <summary>
        /// Inserts the specified characters, at the specified <paramref name="offset"/>, in the
        /// <see cref="Data"/> string; when this method returns, data contains the modified string.
        /// </summary>
        /// <param name="offset">Specifies where to begin inserting characters. Start value starts at zero.</param>
        /// <param name="data">Specifies the string to insert.</param>
        void InsertData(int offset, string data);

        /// <summary>
        /// Removes the specified <paramref name="count"/> of characters, starting at the
        /// specified <paramref name="offset"/>, from the <see cref="Data"/> string;
        /// when this method returns, <see cref="Data"/> contains the shortened string.
        /// </summary>
        /// <param name="offset">Specifies where to begin removing characters. Start value starts at zero.</param>
        /// <param name="count">Specifies how many characters to delete.</param>
        void DeleteData(int offset, int count);

        /// <summary>
        /// Replaces the specified <paramref name="count"/> of characters, starting at the specified
        /// <paramref name="offset"/>, with the specified string; when this method returns,
        /// <see cref="Data"/> contains the modified string.
        /// </summary>
        /// <param name="offset">Specifies where to begin replacing characters. Start value starts at zero.</param>
        /// <param name="count">Specifies how many characters to replace.</param>
        /// <param name="data">Specifies the string to insert.</param>
        void ReplaceData(int offset, int count, string data);
    }
}
