﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal interface IDomParserClient
    {
        void ParseError(ParseError error);
    }
}
