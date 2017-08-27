using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public sealed class Text : CharacterData<ReferenceText>
    {
        public Text(Context context, ReferenceText model) : base(context, model)
        {
        }
    }
}
