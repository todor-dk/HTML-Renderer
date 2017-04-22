using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// Indicates the typeof token that <see cref="TokenData"/> contains.
    /// </summary>
    public enum TokenType : byte
    {
        Unknown = 0,

        Character,

        EndOfFile,

        DocType,

        Comment,

        StartTag,

        EndTag
    }
}
