using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal sealed class Text : CharacterData, Dom.Text
    {
        public Text(Document document, string baseUri)
            : base(document)
        {
        }

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return "#text"; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.Text; }
        }

        public string WholeText
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Text SplitText(int offset)
        {
            throw new NotImplementedException();
        }
    }
}
