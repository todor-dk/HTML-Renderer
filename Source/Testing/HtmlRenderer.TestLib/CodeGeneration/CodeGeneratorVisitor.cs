using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlRenderer.TestLib.Dom;
using System.IO;

namespace HtmlRenderer.TestLib.CodeGeneration
{
    public class CodeGeneratorVisitor : Dom.IConcreteVisitor
    {
        private readonly StringBuilder Body = new StringBuilder();

        public override string ToString()
        {
            return this.Body.ToString();
        }

        public void VisitComment(ReferenceComment comment)
        {
            this.PrintDepth(comment);
            this.Body.Append("<!--");
            this.PrintString(comment.Data);
            this.Body.Append("-->");
        }

        public void VisitDocument(ReferenceDocument document)
        {
            this.PrintDepth(document);
        }

        public void VisitDocumentFragment(ReferenceDocumentFragment fragment)
        {
            this.PrintDepth(fragment);
            throw new NotImplementedException();
        }

        public void VisitDocumentType(ReferenceDocumentType doctype)
        {
            this.PrintDepth(doctype);
            this.Body.Append("<!DOCTYPE ");
            this.Body.Append(doctype.Name);
            this.Body.Append(" ");
            this.Body.Append(doctype.PublicId);
            this.Body.Append(" ");
            this.Body.Append(doctype.SystemId);
            this.Body.Append(">");
        }

        public void VisitElement(ReferenceElement element)
        {
            this.PrintDepth(element);
            this.Body.Append("<");
            this.Body.Append(element.TagName.ToLower());

            foreach(ReferenceAttr attr in element.Attributes)
            {
                this.Body.Append(" ");
                this.Body.Append(attr.Name);
                this.Body.Append("=");
                this.PrintString(attr.Value);
            }

            this.Body.Append(">");
        }

        public void VisitText(ReferenceText text)
        {
            this.PrintDepth(text);
            this.Body.Append("#text ");
            this.PrintString(text.Data);
        }

        private int PrintDepth(ReferenceNode node)
        {
            int d = -1;
            this.Body.AppendLine();
            while(node != null)
            {
                d++;
                this.Body.Append("    ");
                node = node.ParentNode;
            }
            return d;
        }

        private void PrintString(string str)
        {
            if (str == null)
            {
                this.Body.Append("null");
                return;
            }

            this.Body.Append("\"");
            str = str.Replace("\\", "\\\\");
            str = str.Replace("\"", "\\\"");
            str = str.Replace("\t", "\\t");
            str = str.Replace("\n", "\\n");
            str = str.Replace("\f", "\\f");
            str = str.Replace("\r", "\\r");
            this.Body.Append(str);
            this.Body.Append("\"");
        }
    }
}
