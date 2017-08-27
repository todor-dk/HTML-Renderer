using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceAttr: Attr
    {
        public ReferenceAttr(ReferenceElement owner, string namespaceUri, string prefix, string localName, string name, string value)
        {
            this.OwnerElement = owner;
            this.NamespaceUri = namespaceUri;
            this.Prefix = prefix;
            this.LocalName = localName;
            this.Name = name;
            this.Value = value;
        }

        public string NamespaceUri { get; private set; }

        public string Prefix { get; private set; }

        public string LocalName { get; private set; }

        public string Name { get; private set; }

        public string Value { get; private set; }

        public ReferenceElement OwnerElement { get; private set; }

        #region Attr interface

        Element Attr.OwnerElement => this.OwnerElement;

        string Attr.Value { get => this.Value; set => throw new NotImplementedException(); }

        #endregion
    }
}
