using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceAttr
    {
        public ReferenceAttr()
        {

        }

        public ReferenceAttr(string namespaceUri, string prefix, string localName, string name, string value)
        {
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

        public bool CompareWith(ReferenceAttr other, CompareContext context)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;

            if (this.LocalName != other.LocalName)
                return false;
            if (this.Name != other.Name)
                return false;
            if (this.NamespaceUri != other.NamespaceUri)
                return false;
            if (!this.OwnerElement.Compare(other.OwnerElement, context))
                return false;
            if (this.Prefix != other.Prefix)
                return false;
            if (this.Value != other.Value)
                return false;

            return true;
        }
    }
}
