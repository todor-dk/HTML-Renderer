using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom.Persisting
{
    public class DomBinaryWriter
    {
        public static readonly byte[] Prefix = new byte[] { 0x6A, 0x4B, 0x67, 0x5B, 0xCC, 0xA9, 0x42, 0xBF, 0xA4, 0x84, 0x25, 0x72, 0x77, 0x7B, 0x78, 0xF9 };

        public static readonly byte[] Postfix = new byte[] { 0xA5, 0xCF, 0x7D, 0xB7, 0xAB, 0x5F, 0x40, 0xA0, 0xB6, 0xA2, 0x7A, 0x4A, 0xED, 0x97, 0x6C, 0x7A };

        private readonly System.IO.BinaryWriter Writer;

        private readonly Dictionary<Node, int> NodeMap = new Dictionary<Node, int>();

        public static void Save(Node root, Stream stream)
        {
            stream.Write(DomBinaryWriter.Prefix, 0, DomBinaryWriter.Prefix.Length);
            DomBinaryWriter writer = new DomBinaryWriter(stream);
            Accept(root, writer.Visit);
            Accept(root, writer.VisitConcrete);
            writer.Writer.Write(-1); // END
            stream.Write(DomBinaryWriter.Postfix, 0, DomBinaryWriter.Postfix.Length);
        }

        private DomBinaryWriter(Stream stream)
        {
            this.Writer = new System.IO.BinaryWriter(stream, System.Text.Encoding.UTF8, true);
        }

        private static void Accept(Node node, Action<Node> visitor)
        {
            if (node == null)
                return;

            visitor(node);

            Accept(node.FirstChild, visitor);
            Accept(node.NextSibling, visitor);
        }

        private void Visit(Node node)
        {
            this.NodeMap.Add(node, this.NodeMap.Count);
        }

        private void VisitConcrete(Node node)
        {
            Element elem = node as Element;
            if (elem != null)
            {
                this.VisitElement(elem);
                return;
            }
            Text text = node as Text;
            if (text != null)
            {
                this.VisitText(text);
                return;
            }
            Comment comment = node as Comment;
            if (comment != null)
            {
                this.VisitComment(comment);
                return;
            }
            Document doc = node as Document;
            if (doc != null)
            {
                this.VisitDocument(doc);
                return;
            }
            DocumentType dt = node as DocumentType;
            if (dt != null)
            {
                this.VisitDocumentType(dt);
                return;
            }
            DocumentFragment df = node as DocumentFragment;
            if (df != null)
            {
                this.VisitDocumentFragment(df);
                return;
            }
            throw new InvalidOperationException();
        }


        private void VisitNode(Node node)
        {
            int id = this.NodeMap[node];

            this.Write(id);
            this.Write((int)node.NodeType);
            this.Write(node.NodeName);

            this.Write(node.BaseUri);
            this.Write(node.OwnerDocument);
            this.Write(node.ParentNode);
            this.Write(node.ParentElement);
            this.Write(node.FirstChild);
            this.Write(node.LastChild);
            this.Write(node.PreviousSibling);
            this.Write(node.NextSibling);
            this.Write(node.NodeValue);
            this.Write(node.TextContent);

            this.Write(node.ChildNodes);
        }

        private void VisitParentNode(ParentNode node)
        {
            this.VisitNode((Node)node);

            this.Write((int?)node.ChildElementCount);
            this.Write(node.FirstElementChild);
            this.Write(node.LastElementChild);

            this.Write(node.Children);
        }

        private void VisitCharacterData(CharacterData node)
        {
            this.VisitNode(node);

            this.Write(node.PreviousElementSibling);
            this.Write(node.NextElementSibling);
            this.Write(node.Data);
            this.Write(node.Length);
        }

        public void VisitComment(Comment comment)
        {
            this.VisitCharacterData(comment);
        }

        public void VisitDocument(Document document)
        {
            this.VisitParentNode(document);

            this.Write(document.Url);
            this.Write(document.DocumentUri);
            this.Write(document.Origin);
            this.Write(document.CompatMode);
            this.Write(document.CharacterSet);
            this.Write(document.ContentType);

            this.Write(document.DocType);
            this.Write(document.DocumentElement);
        }

        public void VisitDocumentFragment(DocumentFragment fragment)
        {
            this.VisitParentNode(fragment);
        }

        public void VisitDocumentType(DocumentType doctype)
        {
            this.VisitNode(doctype);

            this.Write(doctype.Name);
            this.Write(doctype.PublicId);
            this.Write(doctype.SystemId);
        }

        public void VisitElement(Element element)
        {
            this.VisitParentNode(element);

            this.Write(element.PreviousElementSibling);
            this.Write(element.NextElementSibling);

            this.Write(element.NamespaceUri);
            this.Write(element.Prefix);
            this.Write(element.LocalName);
            this.Write(element.TagName);
            this.Write(element.Id);
            this.Write(element.ClassName);

            this.Write((IEnumerable<string>)null); //this.Write(element.ClassList);

            this.Write(element.Attributes.Count);
            foreach (Attr attr in element.Attributes)
                this.Write(attr);
        }

        public void VisitText(Text text)
        {
            this.VisitCharacterData(text);
            this.Write(text.WholeText);
        }


        private void Write(int value)
        {
            this.Writer.Write(value);
        }

        private void Write(int? value)
        {
            if (value.HasValue)
            {
                this.Writer.Write(false);
                this.Writer.Write(value.Value);
            }
            else
            {
                this.Writer.Write(true);
            }
        }

        private void Write(string value)
        {
            if (value == null)
            {
                this.Writer.Write(true);
            }
            else
            {
                this.Writer.Write(false);
                this.Writer.Write(value);
            }
        }

        private void Write(Node node)
        {
            if (node == null)
                this.Write(-1);
            else
                this.Write(this.NodeMap[node]);
        }

        private void Write(IEnumerable<Node> nodes)
        {
            if (nodes == null)
            {
                this.Write(-1);
                return;
            }

            int count = nodes.Count();
            this.Write(count);
            foreach (Node node in nodes)
                this.Write(node);
        }

        private void Write(Attr attr)
        {
            this.Write(attr.NamespaceUri);
            this.Write(attr.Prefix);
            this.Write(attr.LocalName);
            this.Write(attr.Name);
            this.Write(attr.Value);
        }

        private void Write(IEnumerable<string> strings)
        {
            if (strings == null)
            {
                this.Write(-1);
                return;
            }

            int count = strings.Count();
            this.Write(count);
            foreach (string str in strings)
                this.Write(str);
        }
    }
}
