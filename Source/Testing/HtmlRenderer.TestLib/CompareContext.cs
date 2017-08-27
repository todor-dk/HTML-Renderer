using HtmlRenderer.TestLib.Dom;
using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib
{
    public class CompareContext
    {
        public CompareResult IgnoredCompareResult;

        #region Comparing

        public virtual CompareResult Compare(Node self, Node other)
        {
            if ((self == null) && (other == null))
                return CompareResult.Equal;

            CompareResult result = this.CompareWithNode(self, other);

            if (self is CharacterData characterData)
                result = result | this.CompareWithCharacterData(characterData, other as CharacterData);
            if (self is ChildNode childNode)
                result = result | this.CompareWithChildNode(childNode, other as ChildNode);
            if (self is Comment comment)
                result = result | this.CompareWithComment(comment, other as Comment);
            if (self is Document document)
                result = result | this.CompareWithDocument(document, other as Document);
            if (self is DocumentFragment documentFragment)
                result = result | this.CompareWithDocumentFragment(documentFragment, other as DocumentFragment);
            if (self is DocumentType documentType)
                result = result | this.CompareWithDocumentType(documentType, other as DocumentType);
            if (self is Element element)
                result = result | this.CompareWithElement(element, other as Element);
            if (self is NonDocumentTypeChildNode nonDocumentTypeChildNode)
                result = result | this.CompareWithNonDocumentTypeChildNode(nonDocumentTypeChildNode, other as NonDocumentTypeChildNode);
            if (self is NonElementParentNode nonElementParentNode)
                result = result | this.CompareWithNonElementParentNode(nonElementParentNode, other as NonElementParentNode);
            if (self is ParentNode parentNode)
                result = result | this.CompareWithParentNode(parentNode, other as ParentNode);
            if (self is ProcessingInstruction processingInstruction)
                result = result | this.CompareWithProcessingInstruction(processingInstruction, other as ProcessingInstruction);
            if (self is Text text)
                result = result | this.CompareWithText(text, other as Text);

            result = result & ~this.IgnoredCompareResult;

            return result;
        }

        public virtual CompareResult CompareRecursive(Node self, Node other)
        {
            if ((self == null) && (other == null))
                return CompareResult.Equal;

            CompareResult result = this.Compare(self, other);

            if ((self != null) && (other != null))
            {
                if (self.ChildNodes.Count != other.ChildNodes.Count)
                {
                    result = result | CompareResult.ChildCountMismatch;
                }
                else
                {
                    for (int i = 0; i < self.ChildNodes.Count; i++)
                    {
                        Node leftChild = self.ChildNodes[i];
                        Node rightChild = other.ChildNodes[i];
                        CompareResult childResult = this.CompareRecursive(leftChild, rightChild);
                        if (childResult != CompareResult.Equal)
                            result = result | CompareResult.InvalidChild; ;
                    }
                }
            }

            result = result & ~this.IgnoredCompareResult;

            return result;
        }

        public virtual CompareResult CompareWithNode(Node self, Node other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.BaseUri != other.BaseUri)
                result = result | CompareResult.Node_BaseUri;

            if (self.NodeName != other.NodeName)
                result = result | CompareResult.Node_NodeName;

            if (self.NodeType != other.NodeType)
                result = result | CompareResult.Node_NodeType;

            if (self.NodeValue != other.NodeValue)
                result = result | CompareResult.Node_NodeValue;

            if (self.TextContent != other.TextContent)
                result = result | CompareResult.Node_TextContent;

            return result;
        }

        public virtual CompareResult CompareWithCharacterData(CharacterData self, CharacterData other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.Data != other.Data)
                result = result | CompareResult.CharacterData_Data;

            if (self.Length != other.Length)
                result = result | CompareResult.CharacterData_Length;

            return result;
        }

        public virtual CompareResult CompareWithComment(Comment self, Comment other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            return result;
        }

        public virtual CompareResult CompareWithProcessingInstruction(ProcessingInstruction self, ProcessingInstruction other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.Target != other.Target)
                result = result | CompareResult.ProcessingInstruction_Target;

            return result;
        }

        public virtual CompareResult CompareWithText(Text self, Text other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.WholeText != other.WholeText)
                result = result | CompareResult.Text_WholeText;

            return result;
        }

        public virtual CompareResult CompareWithParentNode(ParentNode self, ParentNode other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            return result;
        }

        public virtual CompareResult CompareWithChildNode(ChildNode self, ChildNode other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            return result;
        }

        public virtual CompareResult CompareWithNonElementParentNode(NonElementParentNode self, NonElementParentNode other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            return result;
        }

        public virtual CompareResult CompareWithNonDocumentTypeChildNode(NonDocumentTypeChildNode self, NonDocumentTypeChildNode other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            // PreviousElementSibling, NextElementSibling

            return result;
        }

        public virtual CompareResult CompareWithDocumentFragment(DocumentFragment self, DocumentFragment other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            return result;
        }

        public virtual CompareResult CompareWithDocument(Document self, Document other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.CharacterSet != other.CharacterSet)
                result = result | CompareResult.Document_CharacterSet;

            if (self.CompatMode != other.CompatMode)
                result = result | CompareResult.Document_CompatMode;

            if (self.ContentType != other.ContentType)
                result = result | CompareResult.Document_ContentType;

            if (this.Compare(self.DocumentElement, other.DocumentElement) != CompareResult.Equal)
                result = result | CompareResult.Document_DocumentElement;

            if (this.Compare(self.DocType, other.DocType) != CompareResult.Equal)
                result = result | CompareResult.Document_DocumentType;

            if (self.DocumentUri != other.DocumentUri)
                result = result | CompareResult.Document_DocumentUri;

            if (self.Origin != other.Origin)
                result = result | CompareResult.Document_Origin;

            if (self.QuirksMode != other.QuirksMode)
                result = result | CompareResult.Document_QuirksMode;

            if (self.Url != other.Url)
                result = result | CompareResult.Document_Url;

            return result;
        }

        public virtual CompareResult CompareWithElement(Element self, Element other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.Attributes.Count != other.Attributes.Count)
            {
                result = result | CompareResult.Element_AttributeCount;
            }
            else
            {
                List<Attr> attrs = new List<Attr>(other.Attributes);

                foreach (Attr attr in self.Attributes)
                {
                    int idx = attrs.FindIndex(at => at.CompareName(attr));
                    if (idx != -1)
                    {
                        Attr at = attrs[idx];
                        attrs.RemoveAt(idx);
                        if (attr.Value == at.Value)
                            continue;
                    }

                    result = result | CompareResult.Element_AttributeValue;
                    break;
                }
            }

            if (self.ClassName != other.ClassName)
                result = result | CompareResult.Element_ClassName;

            //if (!self.ClassList.ListsEquals(other.ClassList))
            //    result = result | CompareResult.Element_ClassList;

            if (self.Id != other.Id)
                result = result | CompareResult.Element_Id;

            if (self.LocalName != other.LocalName)
                result = result | CompareResult.Element_LocalName;

            if (self.NamespaceUri != other.NamespaceUri)
                result = result | CompareResult.Element_NamespaceUri;

            if (self.Prefix != other.Prefix)
                result = result | CompareResult.Element_Prefix;

            if (self.TagName != other.TagName)
                result = result | CompareResult.Element_TagName;

            return result;
        }

        public virtual CompareResult CompareWithDocumentType(DocumentType self, DocumentType other)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            if (other == null)
                return CompareResult.NodeTypeMismatch;

            CompareResult result = CompareResult.Equal;

            if (self.Name != other.Name)
                result = result | CompareResult.DocumentType_Name;

            if (self.PublicId != other.PublicId)
                result = result | CompareResult.DocumentType_PublicId;

            if (self.SystemId != other.SystemId)
                result = result | CompareResult.DocumentType_SystemId;

            return result;
        }

        #endregion

        #region Validation

        public HierarchyResult Validate(Document self)
        {
            return this.Validate(self, null, null, null, null, null, null);
        }

        public HierarchyResult Validate(Node self, Document ownerDocument, Node parentNode, Node previousSibling, Node nextSibling, Element previousElementSibling, Element nextElementSibling)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            HierarchyResult result = this.ValidateNode(self, ownerDocument, parentNode, previousSibling, nextSibling);

            if (self is NonDocumentTypeChildNode nonDocumentTypeChildNode)
                result = result | this.ValidateNonDocumentTypeChildNode(nonDocumentTypeChildNode, previousElementSibling, nextElementSibling);
            if (self is Element element)
                result = result | this.ValidateElement(element, previousElementSibling, nextElementSibling);
            if (self is Document document)
                result = result | this.ValidateDocument(document);
            if (self is ParentNode pn)
                result = result | this.ValidateParentNode(pn);

            return result;
        }

        public virtual HierarchyResult ValidateRecursive(Document self)
        {
            return this.ValidateRecursive(self, null, null, null, null, null, null);
        }

        public virtual HierarchyResult ValidateRecursive(Node self, Document ownerDocument, Node parentNode, Node previousSibling, Node nextSibling, Element previousElementSibling, Element nextElementSibling)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            HierarchyResult result = this.Validate(self, ownerDocument, parentNode, previousSibling, nextSibling, previousElementSibling, nextElementSibling);

            // Document nodes have the OwnerDocument set to null, but for all children, this is the document.
            ownerDocument = ownerDocument ?? self as Document;

            Node[] children = self.ChildNodes.ToArray();
            Element[] prevElements = new Element[children.Length];
            Element[] nextElements = new Element[children.Length];
            Element elem = null;
            for (int i = 0; i < children.Length; i++)
            {
                prevElements[i] = elem;
                elem = (children[i] as Element) ?? elem;
            }

            elem = null;
            for (int i = children.Length - 1; i >= 0; i--)
            {
                nextElements[i] = elem;
                elem = (children[i] as Element) ?? elem;
            }


            for (int i = 0; i < children.Length; i++)
            {
                Node child = children[i];
                Node ps = (i == 0) ? null : children[i - 1];
                Node ns = ((i + 1) == children.Length) ? null : children[i + 1];
                HierarchyResult childResult = this.ValidateRecursive(child, ownerDocument, self, ps, ns, prevElements[i], nextElements[i]);
                if (childResult != HierarchyResult.Valid)
                    result = result | HierarchyResult.InvalidChild;
            }

            return result;
        }

        public virtual HierarchyResult ValidateNode(Node self, Document ownerDocument, Node parentNode, Node previousSibling, Node nextSibling)
        {
            HierarchyResult result = HierarchyResult.Valid;

            if (ownerDocument == null)
            {
                if (self.OwnerDocument != null)
                    result = result | HierarchyResult.Node_OwnerDocument_MustBeNull;
            }
            else
            {
                if (self.OwnerDocument == null)
                    result = result | HierarchyResult.Node_OwnerDocument_Invalid;
            }

            if (parentNode == null)
            {
                if (self.ParentNode != null)
                    result = result | HierarchyResult.Node_ParentNode_MustBeNull;
                if (self.ParentElement != null)
                    result = result | HierarchyResult.Node_ParentElement_MustBeNull;
            }
            else
            {
                if (self.ParentNode != parentNode)
                    result = result | HierarchyResult.Node_ParentNode_Invalid;
                Element parentElement = parentNode as Element;
                if (self.ParentElement != parentElement)
                    result = result | HierarchyResult.Node_ParentElement_Invalid;
            }

            if (self.PreviousSibling != previousSibling)
                result = result | HierarchyResult.Node_PreviousSibling_Invalid;
            if (self.NextSibling != nextSibling)
                result = result | HierarchyResult.Node_NextSibling_Invalid;

            NodeList children = self.ChildNodes;
            if (children.Count == 0)
            {
                if (self.FirstChild != null)
                    result = result | HierarchyResult.Node_FirstChild_Invalid;
                if (self.LastChild != null)
                    result = result | HierarchyResult.Node_LastChild_Invalid;
            }
            else
            {
                if (self.FirstChild != children[0])
                    result = result | HierarchyResult.Node_FirstChild_Invalid;
                if (self.LastChild != children[children.Count - 1])
                    result = result | HierarchyResult.Node_LastChild_Invalid;

                if (children.Count != children.Distinct().Count())
                    result = result | HierarchyResult.Node_ChildNodes_NotUnique;

                if (!(self is ParentNode))
                    result = result | HierarchyResult.Node_ChildNodes_NotEmptyForNonParent;
            }

            return result;
        }

        public virtual HierarchyResult ValidateNonDocumentTypeChildNode(NonDocumentTypeChildNode self, Element previousElementSibling, Element nextElementSibling)
        {
            HierarchyResult result = HierarchyResult.Valid;

            if (self.PreviousElementSibling != previousElementSibling)
                result = result | HierarchyResult.NonDocumentTypeChildNode_PreviousElementSibling_Invalid;
            if (self.NextElementSibling != nextElementSibling)
                result = result | HierarchyResult.NonDocumentTypeChildNode_NextElementSibling_Invalid;

            return result;
        }

        public virtual HierarchyResult ValidateElement(Element self, Element previousElementSibling, Element nextElementSibling)
        {
            HierarchyResult result = HierarchyResult.Valid;

            if (self.PreviousElementSibling != previousElementSibling)
                result = result | HierarchyResult.Element_PreviousElementSibling_Invalid;
            if (self.NextElementSibling != nextElementSibling)
                result = result | HierarchyResult.Element_NextElementSibling_Invalid;

            return result;
        }

        public virtual HierarchyResult ValidateDocument(Document self)
        {
            HierarchyResult result = HierarchyResult.Valid;

            // Skip comments.
            Node[] nodes = self.ChildNodes.Where(n => !(n is Comment)).ToArray();
            Node[] expected = new Node[] { self.DocType, self.DocumentElement }.Where(n => n != null).ToArray();

            if (!nodes.ArraysEquals(expected, Object.ReferenceEquals))
                result = result | HierarchyResult.Document_DocTypeOrElement_Invalid;

            return result;
        }

        public virtual HierarchyResult ValidateParentNode(ParentNode self)
        {
            HierarchyResult result = HierarchyResult.Valid;

            HtmlCollection children = self.Children;
            if (children.Count == 0)
            {
                if (self.FirstElementChild != null)
                    result = result | HierarchyResult.ParentNode_FirstChild_Invalid;
                if (self.LastElementChild != null)
                    result = result | HierarchyResult.ParentNode_LastChild_Invalid;
            }
            else
            {
                if (self.FirstElementChild != children[0])
                    result = result | HierarchyResult.ParentNode_FirstChild_Invalid;
                if (self.LastElementChild != children[children.Count - 1])
                    result = result | HierarchyResult.ParentNode_LastChild_Invalid;

                if (children.Count != children.Distinct().Count())
                    result = result | HierarchyResult.ParentNode_Children_NotUnique;
            }

            if (self.ChildElementCount != children.Count)
                result = result | HierarchyResult.ParentNode_ChildElementCount_Invalid;

            Element[] elements = children.ToArray();
            Element[] expected = ((Node)self).ChildNodes.OfType<Element>().ToArray();

            if (!elements.ArraysEquals(expected, Object.ReferenceEquals))
                result = result | HierarchyResult.ParentNode_Children_Inconsistent;

            return result;
        }

        #endregion
    }
}
