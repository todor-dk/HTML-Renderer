using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-745549614
    public class Element : Node
    {
        public string TagName { get; private set; }

        public string NamespaceUri { get; private set; }

        public override string NodeName
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

        public override NodeType NodeType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public void SetAttribute(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAttribute(string name)
        {
            throw new NotImplementedException();
        }
    }
}
