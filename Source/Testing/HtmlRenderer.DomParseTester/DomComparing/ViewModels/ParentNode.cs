using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public abstract class ParentNode<TReferenceNode> : Node<TReferenceNode>
        where TReferenceNode : ReferenceParentNode
    {
        public ParentNode(Context context, TReferenceNode model)
            : base(context, model)
        {
        }
    }
}
