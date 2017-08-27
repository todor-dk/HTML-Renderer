using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class Element : ParentNode<ReferenceElement>
    {
        public Element(Context context, ReferenceElement model) : base(context, model)
        {
        }
    }
}
