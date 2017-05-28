using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// Indicates a type of parse error that may be detected during parsing.
    /// </summary>
    public enum ParseError
    {
        InvalidDocType,

        InvalidTag,

        InvalidScript,

        InvalidCharacterReference,

        InvalidAttribute,

        InvalidComment,

        NullCharacter,

        PrematureEndOfFile,

        InvalidMarkup,

        UnexpectedTag,

        UnexpectedStartTag,

        UnexpectedEndTag,

        UnexpectedDocType,

        WrongNamespace
    }
}
