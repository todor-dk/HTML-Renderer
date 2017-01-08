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

        void ReceiveEndOfFile();

        void ReceiveDocType(string name, string publicIdentifier, string systemIdentifier, bool forceQuirks);

        void ReceiveComment(Func<string> data);

        void ReceiveStartTag(string tagName, bool isSelfClosing, Attribute[] attributes);

        void ReceiveEndTag(string tagName, bool isSelfClosing, Attribute[] attributes);
    }
}
