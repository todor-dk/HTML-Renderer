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

        public override CompareResult Compare(Node other)
        {
            Comment candidate = other as Comment;
            if (candidate == null)
                return CompareResult.NodeTypeMismatch;
            return this.CompareWithCharacterData(candidate.Model);
        }
    }
}
