using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#i-Document
    public class Document : Node
    {
        public DocumentType DocumentType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Element DocumentElement
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public Attr CreateAttribute(string name)
        {
            throw new NotImplementedException();
        }

        public object CreateCDataSection(string data)
        {
            throw new NotImplementedException();
        }

        public Comment CreateComment(string data)
        {
            throw new NotImplementedException();
        }

        public Element CreateElement(string tagName)
        {
            throw new NotImplementedException();
        }

        public Text CreateTextNode(string data)
        {
            throw new NotImplementedException();
        }

        public Element GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }

        
    }
}
