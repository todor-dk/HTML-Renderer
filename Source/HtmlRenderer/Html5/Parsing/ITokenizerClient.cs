using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal interface ITokenizerClient
    {
        void ParseError(ParseErrors error);

        void ReceiveCharacter(char character);

        void ReceiveDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks);
    }
}
