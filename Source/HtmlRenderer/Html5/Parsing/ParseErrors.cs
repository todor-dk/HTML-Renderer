﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public enum ParseErrors
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

        UnespectedStartTag,

        UnespectedEndTag,
    }
}
