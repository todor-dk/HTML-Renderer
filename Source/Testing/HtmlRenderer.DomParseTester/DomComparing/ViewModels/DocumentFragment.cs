using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class DocumentFragment : ParentNode<ReferenceDocumentFragment>
    {
        public DocumentFragment(Context context, ReferenceDocumentFragment model) : base(context, model)
        {
        }
    }
}
