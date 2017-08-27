using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class DocumentType : Node<ReferenceDocumentType>
    {
        public DocumentType(Context context, ReferenceDocumentType model) : base(context, model)
        {
        }
    }
}
