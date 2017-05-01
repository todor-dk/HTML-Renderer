using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// The scripting flag is set to "enabled" if scripting was enabled for the Document with which the parser is
    /// associated when the parser was created, and "disabled" otherwise.
    /// </summary>
    /// <remarks>
    /// NOTE: The scripting flag can be enabled even when the parser was originally created for the HTML fragment
    /// parsing algorithm, even though script elements don't execute in that case.
    /// <para/>
    /// NOTE: This .Net implementation will probably NEVER implement scripting and therefore this will always be false.
    /// </remarks>
    public class ParsingContext
    {
        /// <summary>
        /// ALWAYS DISABLED. We do not support scripting!
        /// The scripting flag is set to "enabled" if scripting was enabled for the Document with
        /// which the parser is associated when the parser was created, and "disabled" otherwise.
        /// </summary>
        /// <remarks>
        /// 8.2.3.5 Other parsing state flags. See http://www.w3.org/TR/html5/syntax.html#other-parsing-state-flags
        /// </remarks>
        public bool Scripting
        {
            get { return false; }
        }

        // http://www.w3.org/TR/html5/embedded-content-0.html#an-iframe-srcdoc-document
        public bool IsIFrameSource
        {
            get { return false; }
        }

        // http://www.w3.org/TR/html51/syntax.html#parsing-html-fragments
        public bool IsFragmentParsing
        {
            get { return false; }
        }

        public Element FragmentContextElement
        {
            get { return null; }
        }

        internal DomFactory CreateDomFactory(DomParser parser)
        {
            Contract.RequiresNotNull(parser, nameof(parser));
            return new DomFactory(parser);
        }
    }
}
