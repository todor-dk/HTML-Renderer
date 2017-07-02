using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal static class Helpers
    {
        public static string GetTextContent(this Node self)
        {
            // The concatenation of data of all the Text node descendants of the context object, in tree order.
            StringBuilder sb = new StringBuilder();
            self.Accept(
                node =>
                {
                    Text text = node as Text;
                    if (text != null)
                        sb.Append(text.Data);
                },
                true);
            return sb.ToString();
        }

        public static string SetTextContent(this Node self, string data)
        {
            if (data == null)
                data = String.Empty;

            // 1. Let node be null.
            // 2. If new value is not the empty string, set node to a new Text node whose data is new value.
            // 3. Replace all with node within the context object.
            throw new NotImplementedException();
        }

        public static string LocateNamespacePrefix(this Dom.Element elem, string @namespace)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#interface-node
            while (elem != null)
            {
                // 1. If element's namespace is namespace and its namespace prefix is not null, return its namespace prefix.
                if ((elem.NamespaceUri == @namespace) && (elem.Prefix != null))
                    return elem.Prefix;

                // 2. If, element has an attribute whose namespace prefix is "xmlns" and value is namespace,
                //    then return element's first such attribute's local name.
                foreach (Attr attr in elem.Attributes)
                {
                    if ((attr.Prefix == "xmlns") && (attr.Value == @namespace))
                        return attr.LocalName;
                }

                // 3. If element's parent element is not null, return the result of running locate a namespace
                //    prefix on that element using namespace. Otherwise, return null.
                elem = elem.ParentElement;
            }

            return null;
        }

        public static string LocateNamespaceUri(this Dom.Node self, string prefix)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#locate-a-namespace
            // To locate a namespace for a node using prefix depends on node:

            // * Element
            if (self is Element)
            {
                Element elem = (Element)self;

                // 1. If its namespace is not null and its namespace prefix is prefix, return namespace.
                if ((elem.NamespaceUri != null) && (elem.Prefix == prefix))
                    return elem.NamespaceUri;

                // 2. If it has an attribute ...
                foreach (Attr attr in elem.Attributes)
                {
                    // ... whose namespace is the XMLNS namespace, namespace prefix is "xmlns" and local name is prefix,
                    // or if prefix is null and it has an attribute whose namespace is the XMLNS namespace, namespace prefix
                    // is null and local name is "xmlns":
                    if (attr.NamespaceUri != "http://www.w3.org/2000/xmlns/")
                        continue;

                    if ((attr.Prefix == "xmlns") && (attr.LocalName == prefix))
                    {
                    }
                    else if ((prefix == null) && (attr.Prefix == null) && (attr.LocalName == "xmlns"))
                    {
                    }
                    else
                    {
                        continue;
                    }

                    // 1. Let value be its value if it is not the empty string, and null otherwise.
                    string value = attr.Value;
                    if (value == "")
                        value = null;

                    // 2. Return value.
                    return value;
                }

                // 3. If its parent element is null, return null.
                // 4. Return the result of running locate a namespace on its parent element using prefix.
                return elem.ParentElement?.LocateNamespaceUri(prefix);
            }

            // * Document
            if (self is Document)
            {
                // 1. If its document element is null, return null.
                // 2. Return the result of running locate a namespace on its document element using prefix.
                return ((Document)self).DocumentElement?.LocateNamespaceUri(prefix);
            }

            // * DocumentType
            // * DocumentFragment
            if ((self is DocumentType) || (self is DocumentFragment))
            {
                // Return null.
                return null;
            }

            // * Any other node
            // 1. If its parent element is null, return null.
            // 2. Return the result of running locate a namespace on its parent element using prefix.
            return self.ParentElement?.LocateNamespaceUri(prefix);
        }

        public static void EnsurePreInsertionValidity(this Node parent, Node node, Node child)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-ensure-pre-insertion-validity
            // To ensure pre-insertion validity of a node into a parent before a child, run these steps:

            // 1. If parent is not a Document, DocumentFragment, or Element node, throw a "HierarchyRequestError".
            if (!((parent is Element) || (parent is Document) || (parent is DocumentFragment)))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 2. If node is a "host-including inclusive ancestor" of parent, throw a "HierarchyRequestError".
            if (node.IsHostIncludingInclusiveAncestor(parent))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 3. If child is not null and its parent is not parent, throw a "NotFoundError" exception.
            if ((child != null) && (child.ParentNode != parent))
                throw new Dom.Exceptions.NotFoundException();

            // 4. If node is not a DocumentFragment, DocumentType, Element, Text, ProcessingInstruction, or Comment node, throw a "HierarchyRequestError".
            if (!((node is DocumentFragment) || (node is DocumentType) || (node is Element) || (node is Text) || (node is Comment)))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 5. If either node is a Text node and parent is a document, or node is a doctype and parent is not a document, throw a "HierarchyRequestError".
            if (((node is Text) && (parent is Document)) || ((node is DocumentType) && !(parent is Document)))
                throw new Dom.Exceptions.HierarchyRequestException();

            // 6. If parent is a document, and any of the statements below, switched on node, are true, throw a "HierarchyRequestError".
            if (parent is Document)
            {
                Document doc = (Document)parent;

                // * DocumentFragment node
                if (node is DocumentFragment)
                {
                    DocumentFragment df = (DocumentFragment)node;

                    // If node has more than one element child or has a Text node child.
                    if ((df.ChildElementCount > 1) || df.ChildNodes.Any(e => e is Text))
                        throw new Dom.Exceptions.HierarchyRequestException();

                    // Otherwise, if node has one element child and either parent has an element child,
                    // child is a doctype, or child is not null and a doctype is following child.
                    if ((df.ChildElementCount == 1) && ((doc.ChildElementCount > 0) || (child is DocumentType) || ((child != null) && (child.NextSibling is DocumentType))))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }

                // * element
                else if (node is Element)
                {
                    // parent has an element child, child is a doctype, or child is not null and a doctype is following child.
                    if ((doc.ChildElementCount > 0) || (child is DocumentType) || ((child != null) && (child.NextSibling is DocumentType)))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }

                // * doctype
                else if (node is DocumentType)
                {
                    // parent has a doctype child, an element is preceding child, or child is null and parent has an element child.
                    if (parent.ChildNodes.Any(e => e is DocumentType) || ((child != null) && (child.PreviousSibling is Element)) || ((child == null) && (doc.ChildElementCount > 0)))
                        throw new Dom.Exceptions.HierarchyRequestException();
                }
            }
        }

        public static bool IsHostIncludingInclusiveAncestor(this Node a, Node b)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-tree-host-including-inclusive-ancestor
            // An object A is a "host-including inclusive ancestor" of an object B, ...

            // ... if either A is an inclusive ancestor of B, ...
            if (a.IsInclusiveAncestor(b))
                return true;

            // ... or if B's root has an associated host and A is a host-including inclusive ancestor of B's root's host.
            // TODO: What ia a "host"??
            // var associatedHost = (b as DocumentFragment)?

            // Note: The DocumentFragment node's host concept is useful for HTML's template element and the ShadowRoot object and impacts the pre - insert and replace algorithms.
            return false;
        }

        public static bool IsInclusiveAncestor(this Dom.Node self, Dom.Node other)
        {
            if (self == null)
                return false;

            // An inclusive ancestor is an object or one of its ancestors.
            while (other != null)
            {
                if (other == self)
                    return true;

                other = other.ParentNode;
            }

            return false;
        }
    }
}
