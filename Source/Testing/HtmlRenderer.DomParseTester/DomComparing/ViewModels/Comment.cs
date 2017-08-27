using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class Comment : CharacterData<ReferenceComment>
    {
        public Comment(Context context, ReferenceComment model) : base(context, model)
        {
        }
    }
}
