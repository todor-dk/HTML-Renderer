using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-637646024
    public class Attr : Node
    {
        public string Name { get; private set; }

        public override string NodeName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override NodeType NodeType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string NodeValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Node ParentNode
        {
            get { return null; }
        }

        public override Node PreviousSibling
        {
            get { return null; }
        }

        public override Node NextSibling
        {
            get { return null; }
        }

        public bool Specified { get; private set; }

        public string Value { get; private set; }

        public Element OwnerElement { get; private set; }
    }
}
