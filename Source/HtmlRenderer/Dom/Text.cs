using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2000/REC-DOM-Level-2-Core-20001113/core.html#ID-1312295772
    public class Text : CharacterData
    {
        public string Data { get; private set; }

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
    }
}
