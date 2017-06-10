﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceElement : ReferenceParentNode
    {
        public ReferenceElement()
        {
            this.Attributes = new ReferenceAttrList();
        }

        public ReferenceElement(Persisting.IReader reader, TheArtOfDev.HtmlRenderer.Dom.NodeType type)
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
            this.ClassList = reader.ReadStringList("ClassList");

            int attribs = reader.ReadInt("Attributes");
            for (int i = 0; i < attribs; i++)
                this.Attributes.Add(reader.ReadAttrib());
        }

        public string NamespaceUri { get; private set; }

        public string Prefix { get; private set; }

        public string LocalName { get; private set; }

        public string TagName { get; private set; }

        public string Id { get; private set; }

        public string ClassName { get; private set; }

        public string[] ClassList { get; private set; }

        public ReferenceAttrList Attributes { get; private set; }

        public ReferenceElement PreviousElementSibling { get; private set; }

        public ReferenceElement NextElementSibling { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitElement(this);
        }

        public override bool CompareWith(ReferenceNode other, CompareContext context)
        {
            return this.CompareWithElement(other as ReferenceElement, context);
        }

        internal bool CompareWithElement(ReferenceElement other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (!this.CompareWithParentNode(other, context))
                return false;

            if (!this.Attributes.CompareCollection(other.Attributes, context))
                return false;
            if (this.ClassList.ArraysEquals(other.ClassList))
                return false;
            if (this.ClassName != other.ClassName)
                return false;
            if (this.Id != other.Id)
                return false;
            if (this.LocalName != other.LocalName)
                return false;
            if (this.NamespaceUri != other.NamespaceUri)
                return false;
            if (!this.NextElementSibling.Compare(other.NextElementSibling, context))
                return false;
            if (this.Prefix != other.Prefix)
                return false;
            if (!this.PreviousElementSibling.Compare(other.PreviousElementSibling, context))
                return false;
            if (this.TagName != other.TagName)
                return false;

            return true;
        }
    }
}
