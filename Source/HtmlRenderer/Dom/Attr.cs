using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-attr
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1302 // Interface names must begin with I
    public interface Attr
#pragma warning restore SA1302 // Interface names must begin with I
#pragma warning restore IDE1006 // Naming Styles
    {
        /// <summary>
        /// The namespace URI of the attribute, or null if there is no namespace.
        /// </summary>
        string NamespaceUri { get; }

        /// <summary>
        /// The namespace prefix of the attribute, or null if no prefix is specified.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// The local part of the qualified name of the attribute.
        /// </summary>
        string LocalName { get; }

        /// <summary>
        /// The attribute's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The element holding the attribute.
        /// </summary>
        Element OwnerElement { get; }

        /// <summary>
        /// The attribute's value.
        /// </summary>
        string Value { get; set; }
    }
}
