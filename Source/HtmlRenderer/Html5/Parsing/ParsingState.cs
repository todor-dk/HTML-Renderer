using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    public class ParsingState
    {
        public QuirksMode QuirksMode;

        #region 8.2.3.4. The element pointers See: http://www.w3.org/TR/html5/syntax.html#the-element-pointers

        public Element Html;

        /// <summary>
        /// Initially, the head element is null. Once a head element has been parsed (whether
        /// implicitly or explicitly) the head element gets set to point to this node.
        /// </summary>
        public Element Head;

        /// <summary>
        /// Initially, the form element is null. The form element points to the last form element
        /// that was opened and whose end tag has not yet been seen. It is used to make form controls
        /// associate with forms in the face of dramatically bad markup, for historical reasons.
        /// It is ignored inside template elements.
        /// </summary>
        public Element Form;

        #endregion

        #region 8.2.3.5. Other parsing state flags. See: http://www.w3.org/TR/html5/syntax.html#other-parsing-state-flags

        // The frameset-ok flag is set to "ok" when the parser is created.
        // It is set to "not ok" after certain tokens are seen.
        public bool FramesetOk { get; internal set; } = true;

        #endregion
    }
}
