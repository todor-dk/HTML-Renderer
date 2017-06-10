using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom.Persisting
{
    public interface IReader
    {
        int? ReadNullableInt(string name);

        int ReadInt(string name);

        string ReadString(string name);

        void ReadNode(string name, Action<ReferenceNode> setter);

        void ReadElement(string name, Action<ReferenceElement> setter);

        ReferenceAttr ReadAttrib();

        string[] ReadStringList(string name);

        void ReadNodeList(string name, Action<ReferenceNode[]> setter);
    }
}
