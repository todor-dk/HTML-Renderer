using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
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

        UnexpectedDocType
    }
}
