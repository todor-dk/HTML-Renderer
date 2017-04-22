using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// Contains event data with information about a parse error.
    /// </summary>
    public class ParseErrorEventArgs : EventArgs
    {
        public ParseError ParseError { get; private set; }

        public ParseErrorEventArgs(ParseError parseError)
        {
            this.ParseError = parseError;
        }
    }
}
