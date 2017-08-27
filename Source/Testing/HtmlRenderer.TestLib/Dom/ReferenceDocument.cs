using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;
using HtmlRenderer.TestLib.Dom.Persisting;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceDocument : ReferenceParentNode, Document
    {
        public static ReferenceDocument FromDocument(Document document)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            DomBinaryWriter.Save(document, stream);
            stream.Position = 0;

            return BinaryReader.FromStream(stream) as ReferenceDocument;
        }

        public ReferenceDocument(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.QuirksMode = QuirksMode.NoQuirks;
            this.Url = reader.ReadString("URL");
            this.DocumentUri = reader.ReadString("DocumentURI");
            this.Origin = reader.ReadString("Origin");
            this.CompatMode = reader.ReadString("CompatMode");
            this.CharacterSet = reader.ReadString("CharacterSet");
            this.ContentType = reader.ReadString("ContentType");

            reader.ReadNode("DocType", node => this.DocType = (ReferenceDocumentType)node);
            reader.ReadElement("DocumentElement", elem => this.DocumentElement = elem);
        }

        public QuirksMode QuirksMode { get; private set; }

        public string Url { get; private set; }

        public string DocumentUri { get; private set; }

        public string Origin { get; private set; }

        public string CompatMode { get; private set; }

        public string CharacterSet { get; private set; }

        public string ContentType { get; private set; }

        public ReferenceDocumentType DocType { get; private set; }

        public ReferenceElement DocumentElement { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitDocument(this);
        }
        
        #region Document interface

        DomImplementation Document.Implementation => throw new NotImplementedException();

        DocumentType Document.DocType => this.DocType;

        Element Document.DocumentElement => this.DocumentElement;

        HtmlCollection Document.GetElementsByTagName(string localName)
        {
            throw new NotImplementedException();
        }

        HtmlCollection Document.GetElementsByTagNameNS(string @namespace, string localName)
        {
            throw new NotImplementedException();
        }

        HtmlCollection Document.GetElementsByClassName(string className)
        {
            throw new NotImplementedException();
        }

        Element Document.CreateElement(string localName)
        {
            throw new NotImplementedException();
        }

        Element Document.CreateElementNS(string @namespace, string qualifiedName)
        {
            throw new NotImplementedException();
        }

        DocumentFragment Document.CreateDocumentFragment()
        {
            throw new NotImplementedException();
        }

        Text Document.CreateTextNode(string data)
        {
            throw new NotImplementedException();
        }

        Comment Document.CreateComment(string data)
        {
            throw new NotImplementedException();
        }

        ProcessingInstruction Document.CreateProcessingInstruction(string target, string data)
        {
            throw new NotImplementedException();
        }

        Node Document.ImportNode(Node node, bool deep)
        {
            throw new NotImplementedException();
        }

        Node Document.AdoptNode(Node node)
        {
            throw new NotImplementedException();
        }

        Element NonElementParentNode.GetElementById(string elementId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
