using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceElement : ReferenceParentNode, Element
    {
        public ReferenceElement()
        {
            this.Attributes = new ReferenceAttrList();
        }

        public ReferenceElement(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.Attributes = new ReferenceAttrList();

            reader.ReadElement("PreviousElementSibling", elem => this.PreviousElementSibling = elem);
            reader.ReadElement("NextElementSibling", elem => this.NextElementSibling = elem);

            this.NamespaceUri = reader.ReadString("NamespaceURI");
            this.Prefix = reader.ReadString("Prefix");
            this.LocalName = reader.ReadString("LocalName");
            this.TagName = reader.ReadString("TagName");
            this.Id = reader.ReadString("Id");
            this.ClassName = reader.ReadString("ClassName");
            this.ClassList = ReferenceDomTokenList.FromArray(reader.ReadStringList("ClassList"));

            int attribs = reader.ReadInt("Attributes");
            for (int i = 0; i < attribs; i++)
                this.Attributes.Add(reader.ReadAttrib(this));
        }

        public string NamespaceUri { get; private set; }

        public string Prefix { get; private set; }

        public string LocalName { get; private set; }

        public string TagName { get; private set; }

        public string Id { get; private set; }

        public string ClassName { get; private set; }

        public ReferenceDomTokenList ClassList { get; private set; }

        public ReferenceAttrList Attributes { get; private set; }

        public ReferenceElement PreviousElementSibling { get; private set; }

        public ReferenceElement NextElementSibling { get; private set; }
        
        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitElement(this);
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append(this.TagName);

            foreach (var attr in this.Attributes)
            {
                sb.Append(' ');
                sb.Append(attr.Name);
                sb.Append("=\"");
                sb.Append(attr.Value);
                sb.Append("\"");
            }

            if (this.ChildNodes.Count == 0)
                sb.Append(" /");
            sb.Append('>');
            return sb.ToString();
        }
        
        #region Element interface

        string Element.Id { get => this.Id; set => throw new NotImplementedException(); }
        string Element.ClassName { get => this.ClassName; set => throw new NotImplementedException(); }

        DomTokenList Element.ClassList => this.ClassList;

        AttrCollection Element.Attributes => this.Attributes;

        Element NonDocumentTypeChildNode.PreviousElementSibling => this.PreviousElementSibling;

        Element NonDocumentTypeChildNode.NextElementSibling => this.NextElementSibling;

        string Element.GetAttribute(string name)
        {
            throw new NotImplementedException();
        }

        string Element.GetAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        void Element.SetAttribute(string name, string value)
        {
            throw new NotImplementedException();
        }

        void Element.SetAttributeNS(string @namespace, string name, string value)
        {
            throw new NotImplementedException();
        }

        void Element.RemoveAttribute(string name)
        {
            throw new NotImplementedException();
        }

        void Element.RemoveAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        bool Element.HasAttribute(string name)
        {
            throw new NotImplementedException();
        }

        bool Element.HasAttributeNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        HtmlCollection Element.GetElementsByTagName(string localName)
        {
            throw new NotImplementedException();
        }

        HtmlCollection Element.GetElementsByTagNameNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        HtmlCollection Element.GetElementsByClassName(string classNames)
        {
            throw new NotImplementedException();
        }

        void ChildNode.Remove()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
