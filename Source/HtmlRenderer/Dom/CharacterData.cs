using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-characterdata
    public interface CharacterData : Node, NonDocumentTypeChildNode, ChildNode
    {
        string Data { get; set; }

        int Length { get; }

        string SubstringData(int offset, int count);

        void AppendData(string data);

        void InsertData(int offset, string data);

        void DeleteData(int offset, int count);

        void ReplaceData(int offset, int count, string data);
    }
}
