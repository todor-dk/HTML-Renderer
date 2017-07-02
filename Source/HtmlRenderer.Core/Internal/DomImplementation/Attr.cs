using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class Attr : Dom.Attr
    {
        #region Attr interface implementation

        /// <summary>
        /// The local part of the qualified name of the attribute.
        /// </summary>
        public string LocalName { get; private set; }

        /// <summary>
        /// The attribute's name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The namespace URI of the attribute, or null if there is no namespace.
        /// </summary>
        public abstract string NamespaceUri { get; }

        /// <summary>
        /// The namespace prefix of the attribute, or null if no prefix is specified.
        /// </summary>
        public abstract string Prefix { get; }

        /// <summary>
        /// The element holding the attribute.
        /// </summary>
        public Element OwnerElement { get; internal set; }

        Dom.Element Dom.Attr.OwnerElement
        {
            get { return this.OwnerElement; }
        }

        private string _Value;

        /// <summary>
        /// The attribute's value.
        /// </summary>
        public string Value
        {
            get { return this._Value; }
            set { this.Change(value ?? String.Empty); }
        }

        #endregion

        #region Internal Helpers

        protected Attr(string localName, string value)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            this.LocalName = localName;
            this.Value = value;
        }

        internal void Change(string value)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-element-attributes-change
            // To change an attribute attribute from an element element to value, run these steps:

            // 1. Queue a mutation record of "attributes" for element with name attribute's local name,
            //    namespace attribute's namespace, and oldValue attribute's value.
            // TODO: Implement observers

            // 2. Set attribute's value to value.
            this._Value = value;

            // 3. An attribute is set and an attribute is changed.
            this.OwnerElement?.AttributeIsSetAndChanged(this);
        }

        #endregion
    }
}
