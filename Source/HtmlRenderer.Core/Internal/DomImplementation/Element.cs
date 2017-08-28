using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;
using Scientia.HtmlRenderer.Html5;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class Element : ParentNode, Dom.Element
    {
        // This contains the attributes of the element. See ElementAttributes for further description.
        private ElementAttributes _Attributes = new ElementAttributes();

        public Element(Document document)
            : base(document)
        {
        }

        #region Node interface overrides

        /// <summary>
        /// Returns a string appropriate for the type of node.
        /// </summary>
        public override string NodeName
        {
            get { return this.TagName; }
        }

        /// <summary>
        /// Returns the type of node.
        /// </summary>
        public override NodeType NodeType
        {
            get { return NodeType.Element; }
        }

        /// <summary>
        /// Returns a duplicate of this node.
        /// </summary>
        /// <param name="deep">True if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>A new node that is a clone this node.</returns>
        public override Dom.Node CloneNode(bool deep = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.GetTextContent(); }
            set { this.SetTextContent(value); }
        }

        #endregion

        #region ChildNode interface

        /// <summary>
        /// Removes this node from its parent children list.
        /// </summary>
        public void Remove()
        {
            this._ParentNode?.RemoveNode(this);
        }

        #endregion

        #region NonDocumentTypeChildNode interface

        /// <summary>
        /// Returns the first following sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element NextElementSibling
        {
            get { return this._ParentNode?.GetNextElementSibling(this); }
        }

        /// <summary>
        /// Returns the first preceding sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element PreviousElementSibling
        {
            get { return this._ParentNode.GetPreviousElementSibling(this); }
        }

        #endregion

        #region Element interface

        /// <summary>
        /// Returns a live collection of all attribute nodes registered to this node.
        /// </summary>
        public AttrCollection Attributes
        {
            get { return new AttrCollectionImplementation(this); }
        }

        /// <summary>
        /// Return a <see cref="DomTokenList"/> object representing the <see cref="ClassName"/> value, i.e. the element's "class" attribute.
        /// </summary>
        public DomTokenList ClassList
        {
            get
            {
                ClassAttr attr = this._Attributes.GetClassAttr();
                if (attr != null)
                    return attr;

                throw new NotImplementedException();

                // attr = (ClassAttr) this.CreateAttribute(null, null, Html5.Attributes.Class, Html5.Attributes.Class, null);
                // this._Attributes.Append(this, attr);
                // return attr;
            }
        }

        /// <summary>
        /// Get or set the value of the "class" attribute of this element.
        /// </summary>
        /// <remarks>
        /// If the attribute is missing, an empty string is returned.
        /// Setting the value of this property either overwrites the existing "class" attribute or adds a new attribute.
        /// </remarks>
        public string ClassName
        {
            get { return this._Attributes.GetClassAttr()?.Value ?? String.Empty; }
            set { this.SetAttribute(Html5.Attributes.Class, value ?? String.Empty); }
        }

        /// <summary>
        /// Get or set the element's identifier, reflecting the <see cref="Attr"/> named "id".
        /// </summary>
        /// <remarks>
        /// If the attribute is missing, an empty string is returned.
        /// Setting the value of this property either overwrites the existing "id" attribute or adds a new attribute.
        /// </remarks>
        public string Id
        {
            get { return this._Attributes.GetIdAttr()?.Value ?? String.Empty; }
            set { this.SetAttribute(Html5.Attributes.Id, value ?? String.Empty); }
        }

        /// <summary>
        /// Returns the local part of the qualified name of an element.
        /// </summary>
        public abstract string LocalName { get; }

        /// <summary>
        /// Returns the namespace URI of the element, or null if the element is not in a namespace.
        /// </summary>
        public abstract string NamespaceUri { get; }

        /// <summary>
        /// Returns the namespace prefix of the specified element, or null if no prefix is specified.
        /// </summary>
        public abstract string Prefix { get; }

        /// <summary>
        /// Returns the qualified name of the element.
        /// For HTML elements in HTML documents, <see cref="TagName"/> returns the element name in the uppercase.
        /// </summary>
        public string TagName
        {
            get
            {
                // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-tagname
                // The tagName attribute must run these steps:
                // 1. If context object's namespace prefix is not null, let qualified name be its namespace prefix,
                //    followed by a ":" (U+003A), followed by its local name. Otherwise, let qualified name be its local name.
                string qualifiedName;
                if (this.Prefix != null)
                    qualifiedName = this.Prefix + ":" + this.LocalName;
                else
                    qualifiedName = this.LocalName;

                // 2. If the context object is in the HTML namespace and its node document is an HTML document,
                //    let qualified name be converted to ASCII uppercase.
                if ((this.NamespaceUri == Namespaces.Html) && (this.OwnerDocument is HtmlDocument))
                    qualifiedName = qualifiedName.ToUpper();

                // 3. Return qualified name.
                return qualifiedName;
            }
        }

        /// <summary>
        /// Returns the value of a specified attribute on the element or null if no attribute exists.
        /// </summary>
        /// <param name="name">The name of the attribute whose value you want to get</param>
        /// <returns>The value of the attribute named <paramref name="name"/> or null if no attribute exists with the given name.</returns>
        public string GetAttribute(string name)
        {
            Contract.RequiresNotNull(name, nameof(name));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-getattribute
            // The getAttribute(name) method must run these steps:

            // 1. If the context object is in the HTML namespace and its node document
            //    is an HTML document, let name be converted to ASCII lowercase.
            if ((this.NamespaceUri == Namespaces.Html) && (this.OwnerDocument is HtmlDocument))
                name = name.ToLower();

            // 2. Return the value of the first attribute in the context object's
            //    attribute list whose name is name, and null otherwise.
            return this.Attributes.FirstOrDefault(attr => attr.Name == name)?.Value;
        }

        /// <summary>
        /// Returns the value of the attribute with the specified namespace and name or null if no attribute exists.
        /// </summary>
        /// <param name="namespace">The namespace in which to look for the specified attribute.</param>
        /// <param name="localName">The local name of the attribute to look for.</param>
        /// <returns>The string value of the specified attribute. If the attribute doesn't exist, the result is null.</returns>
        public string GetAttributeNS(string @namespace, string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-getattributens
            // The getAttributeNS(namespace, localName) method must return the following steps:

            // 1. If namespace is the empty string, set it to null.
            if (@namespace == String.Empty)
                @namespace = null;

            // 2. Return getting an attribute for the context object using localName and namespace.
            //    Return the value of the attribute in element's attribute list whose namespace is
            //    namespace and local name is localName, if it has one, and null otherwise.
            return this.Attributes.FirstOrDefault(attr => (attr.NamespaceUri == @namespace) && (attr.LocalName == localName))?.Value;
        }

        public HtmlCollection GetElementsByClassName(string classNames)
        {
            Contract.RequiresNotNull(classNames, nameof(classNames));

            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagName(string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            throw new NotImplementedException();
        }

        public HtmlCollection GetElementsByTagNameNS(string @namespace, string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates whether this element has the specified attribute or not.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>True if this element has the specified attribute, otherwise false.</returns>
        public bool HasAttribute(string name)
        {
            Contract.RequiresNotNull(name, nameof(name));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-hasattribute
            // The hasAttribute(name) method must run these steps:

            // 1. If the context object is in the HTML namespace and its node document
            //    is an HTML document, let name be converted to ASCII lowercase.
            if ((this.NamespaceUri == Namespaces.Html) && (this.OwnerDocument is HtmlDocument))
                name = name.ToLower();

            // 2. Return true if the context object has an attribute whose name is name, and false otherwise.
            return this.Attributes.Any(attr => attr.Name == name);
        }

        /// <summary>
        /// Indicates whether this element has the specified attribute or not.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="localName">The local name of the attribute.</param>
        /// <returns>True if this element has the specified attribute, otherwise false.</returns>
        public bool HasAttributeNS(string @namespace, string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-hasattributens
            // The hasAttributeNS(namespace, localName) method must run these steps:

            // 1. If namespace is the empty string, set it to null.
            if (@namespace == String.Empty)
                @namespace = null;

            // 2. Return true if the context object has an attribute whose namespace is namespace
            //    and local name is localName, and false otherwise.
            return this.Attributes.Any(attr => (attr.NamespaceUri == @namespace) && (attr.LocalName == localName));
        }

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute to be removed from element.</param>
        public void RemoveAttribute(string name)
        {
            Contract.RequiresNotNull(name, nameof(name));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-removeattribute
            // The removeAttribute(name) method must run these steps:

            // 1. If the context object is in the HTML namespace and its node document
            //    is an HTML document, let name be converted to ASCII lowercase.
            if ((this.NamespaceUri == Namespaces.Html) && (this.OwnerDocument is HtmlDocument))
                name = name.ToLower();

            // 2. Remove the first attribute from the context object whose name is name, if any.
            this._Attributes.RemoveAttribute(attr => attr.Name == name);
        }

        /// <summary>
        /// Removes the specified attribute from this element.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="localName">The name of the attribute to be removed from the current node.</param>
        public void RemoveAttributeNS(string @namespace, string localName)
        {
            Contract.RequiresNotNull(localName, nameof(localName));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-removeattributens
            // The removeAttributeNS(namespace, localName) method must return the following steps:

            // 1. If namespace is the empty string, set it to null.
            if (@namespace == String.Empty)
                @namespace = null;

            // 2. Remove the attribute from the context object whose namespace is namespace and local name is localName, if any.
            this._Attributes.RemoveAttribute(attr => (attr.NamespaceUri == @namespace) && (attr.LocalName == localName));
        }

        /// <summary>
        /// Sets the value of an attribute on the specified element. If the attribute already exists,
        /// the value is updated; otherwise a new attribute is added with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the attribute whose value is to be set.</param>
        /// <param name="value">The value to assign to the attribute.</param>
        public void SetAttribute(string name, string value)
        {
            Contract.RequiresNotNull(name, nameof(name));
            Contract.RequiresNotNull(value, nameof(value));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-setattribute
            // The setAttribute(name, value) method must run these steps:

            // 1. If name does not match the Name production in XML, throw an "InvalidCharacterError" exception.
            if (!SyntaticConstructs.IsName(name))
                throw new Dom.Exceptions.InvalidCharacterException();

            // 2. If the context object is in the HTML namespace and its node document
            //    is an HTML document, let name be converted to ASCII lowercase.
            if ((this.NamespaceUri == Namespaces.Html) && (this.OwnerDocument is HtmlDocument))
                name = name.ToLower();

            // 3. Let attribute be the first attribute in the context object's attribute
            //    list whose name is name, or null if there is no such attribute.
            Attr attribute = (Attr)this.Attributes.FirstOrDefault(attr => attr.Name == name);

            // 4. If attribute is null, create an attribute whose local name is name and value is value,
            //    and then append this attribute to the context object and terminate these steps.
            if (attribute == null)
            {
                attribute = this.CreateAttribute(null, null, name, name, value);
                this._Attributes.Append(this, attribute);
                return;
            }

            // 5. Change attribute from context object to value.
            attribute.Change(value);
        }

        /// <summary>
        /// Adds a new attribute or changes the value of an attribute with the given namespace and name.
        /// </summary>
        /// <param name="namespace">The namespace of the attribute.</param>
        /// <param name="name">The attribute to be set.</param>
        /// <param name="value">Value of the new attribute.</param>
        public void SetAttributeNS(string @namespace, string name, string value)
        {
            Contract.RequiresNotNull(name, nameof(name));
            Contract.RequiresNotNull(value, nameof(value));

            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-element-setattributens
            // The setAttributeNS(namespace, name, value) method must run these steps:

            // 1. If namespace is the empty string, set it to null.
            if (@namespace == String.Empty)
                @namespace = null;

            // 2. If name does not match the Name production in XML, throw an "InvalidCharacterError" exception.
            if (!SyntaticConstructs.IsName(name))
                throw new Dom.Exceptions.InvalidCharacterException();

            // 3. If name does not match the QName production in Namespaces in XML, throw a "NamespaceError" exception.
            if (!SyntaticConstructs.IsQName(name))
                throw new Dom.Exceptions.NamespaceException();

            // 4. If name contains a ":" (U+003E), then split the string on it and let prefix be the part before
            //    and localName the part after.Otherwise, let prefix be null and localName be name.
            string prefix;
            string localName;
            int idx = name.IndexOf(':');
            if (idx != -1)
            {
                prefix = name.Substring(0, idx);
                localName = name.Substring(idx + 1);
            }
            else
            {
                prefix = null;
                localName = name;
            }

            // 5. If prefix is not null and namespace is null, throw a "NamespaceError" exception.
            if ((prefix != null) && (@namespace == null))
                throw new Dom.Exceptions.NamespaceException();

            // 6. If prefix is "xml" and namespace is not the XML namespace, throw a "NamespaceError" exception.
            if ((prefix == "xml") && (@namespace != Namespaces.Xml))
                throw new Dom.Exceptions.NamespaceException();

            // 7. If name or prefix is "xmlns" and namespace is not the XMLNS namespace, throw a "NamespaceError" exception.
            if ((prefix == "xmlns") && (@namespace != Namespaces.Xmlns))
                throw new Dom.Exceptions.NamespaceException();

            // 8. If namespace is the XMLNS namespace and neither name nor prefix is "xmlns", throw a "NamespaceError" exception.
            if ((@namespace == Namespaces.Xmlns) && (prefix != "xmlns"))
                throw new Dom.Exceptions.NamespaceException();

            // 9. Set an attribute for the context object using localName, value, and also name, prefix, and namespace.
            // a. Let attribute be the attribute in element's attribute list whose namespace is namespace
            //    and whose local name is localName, or null if there is no such attribute.
            Attr attribute = (Attr)this.Attributes.FirstOrDefault(attr => (attr.NamespaceUri == @namespace) && (attr.LocalName == localName));

            // b. If attribute is null, create an attribute whose local name is localName, value is value, name is name, namespace
            //    is namespace, and namespace prefix is prefix, and then append this attribute to element and terminate these steps.
            if (attribute == null)
            {
                attribute = this.CreateAttribute(@namespace, prefix, localName, name, value);
                this._Attributes.Append(this, attribute);
                return;
            }

            // c. Change attribute from element to value.
            attribute.Change(value);
        }

        #endregion

        #region AttrCollection interface

        private struct AttrCollectionImplementation : Dom.AttrCollection, IEquatable<AttrCollectionImplementation>
        {
            private readonly Element Owner;

            public AttrCollectionImplementation(Element owner)
            {
                this.Owner = owner;
            }


            #region Equality Members

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode()
            {
                return this.Owner.GetHashCode();
            }

            /// <summary>
            /// Determines whether the specified AttrCollectionImplementation is equal to the current AttrCollectionImplementation.
            /// </summary>
            /// <param name="other">Another AttrCollectionImplementation to compare to this AttrCollectionImplementation.</param>
            /// <returns><c>true</c> if this AttrCollectionImplementation equals the given AttrCollectionImplementation, <c>false</c> otherwise.</returns>
            public bool Equals(AttrCollectionImplementation other)
            {
                return Object.ReferenceEquals(this.Owner, other.Owner);
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is AttrCollectionImplementation))
                    return false;

                return this.Equals((AttrCollectionImplementation)obj);
            }

            /// <summary>
            /// Compares whether the left AttrCollectionImplementation operand is equal to the right AttrCollectionImplementation operand.
            /// </summary>
            /// <param name="left">The left AttrCollectionImplementation operand.</param>
            /// <param name="right">The right AttrCollectionImplementation operand.</param>
            /// <returns>The result of the equality operator.</returns>
            public static bool operator ==(AttrCollectionImplementation left, AttrCollectionImplementation right)
            {
                return left.Equals(right);
            }

            /// <summary>
            /// Compares whether the left AttrCollectionImplementation operand is not equal to the right AttrCollectionImplementation operand.
            /// </summary>
            /// <param name="left">The left AttrCollectionImplementation operand.</param>
            /// <param name="right">The right AttrCollectionImplementation operand.</param>
            /// <returns>The result of the inequality operator.</returns>
            public static bool operator !=(AttrCollectionImplementation left, AttrCollectionImplementation right)
            {
                return !left.Equals(right);
            }

            #endregion


            int AttrCollection.Count
            {
                get { return this.Owner._Attributes.GetCount(); }
            }

            int NamedNodeMap.Length
            {
                get { return this.Owner._Attributes.GetCount(); }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.Owner._Attributes.GetEnumerator();
            }

            IEnumerator<Dom.Attr> IEnumerable<Dom.Attr>.GetEnumerator()
            {
                return this.Owner._Attributes.GetEnumerator();
            }

            Dom.Attr NamedNodeMap.Item(int index)
            {
                return this.Owner._Attributes.GetItem(index);
            }

            Dom.Attr NamedNodeMap.GetNamedItem(string name)
            {
                return this.Owner.Attributes.FirstOrDefault(attr => attr.Name == name);
            }

            Dom.Attr NamedNodeMap.GetNamedItemNS(string namespaceUri, string localName)
            {
                return this.Owner.Attributes.FirstOrDefault(attr => (attr.NamespaceUri == namespaceUri) && (attr.LocalName == localName));
            }
        }

        #endregion

        #region Internal extensions

        internal bool IsNamed(string name)
        {
            // it has an ID which is key.
            // it has a name attribute whose value is key;
            return (this.Id == name) || (this.GetAttribute(Html5.Attributes.Name) == name);
        }

        private Attr CreateAttribute(string namespaceUri, string prefix, string localName, string name, string value)
        {
            // NB: Can we kill the "name" argument?
            if ((namespaceUri == null) && (prefix == null))
            {
                if (localName == Html5.Attributes.Id)
                    return new IdAttr(localName, value);
                else if (localName == Html5.Attributes.Class)
                    return new ClassAttr(localName, value);
                else
                    return new NormalAttr(localName, value);
            }

            return new FullAttr(namespaceUri, prefix, localName, name, value);
        }

        internal virtual void AttributeIsSetAndAdded(Attr attr)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#attribute-is-set
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#attribute-is-added
            // NOTHING HERE.
        }

        internal virtual void AttributeIsSetAndChanged(Attr attr)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#attribute-is-set
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#attribute-is-changed
            // NOTHING HERE.
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append(this.TagName);

            foreach (Attr attr in this.Attributes)
            {
                sb.Append(' ');
                sb.Append(attr.Name);
                sb.Append("=\"");
                sb.Append(attr.Value);
                sb.Append("\"");
            }

            if (!this.HasChildren())
                sb.Append(" /");
            sb.Append('>');
            return sb.ToString();
        }
    }
}
