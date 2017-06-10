using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public interface IConcreteVisitor
    {
        void VisitComment(ReferenceComment comment);

        void VisitText(ReferenceText text);

        void VisitDocumentType(ReferenceDocumentType doctype);

        void VisitDocumentFragment(ReferenceDocumentFragment fragment);

        void VisitDocument(ReferenceDocument document);

        void VisitElement(ReferenceElement element);
    }
}
