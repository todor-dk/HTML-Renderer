using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-element
    public interface Element : Node, ParentNode, NonDocumentTypeChildNode, ChildNode
    {
        string NamespaceUri { get; }

        string Prefix { get; }

        string LocalName { get; }

        string TagName { get; }

        string Id { get; set; }

        string ClassName { get; set; }

        DomTokenList ClassList { get; }

        AttrCollection Attributes { get; }

        string GetAttribute(string name);

        string GetAttributeNS(string @namespace, string localName);

        void SetAttribute(string name, string value);

        void SetAttributeNS(string @namespace, string name, string value);

        void RemoveAttribute(string name);

        void RemoveAttributeNS(string @namespace, string localName);

        bool HasAttribute(string name);

        bool HasAttributeNS(string @namespace, string localName);

        HtmlCollection GetElementsByTagName(string localName);

        HtmlCollection GetElementsByTagNameNS(string @namespace, string localName);

        HtmlCollection GetElementsByClassName(string classNames);
    }
}
