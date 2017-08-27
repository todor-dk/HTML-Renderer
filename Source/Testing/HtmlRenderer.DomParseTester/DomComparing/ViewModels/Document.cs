using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class Document : ParentNode<ReferenceDocument>
    {
        public Document(Context context, ReferenceDocument model)
            : base(context, model)
        {
        }
    }
}
