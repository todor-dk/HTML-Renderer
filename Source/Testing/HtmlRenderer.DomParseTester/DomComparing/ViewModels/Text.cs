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

        protected CompareResult CompareWithText(ReferenceText otherModel)
        {
            CompareResult result = this.CompareWithCharacterData(otherModel);

            if (this.Model.WholeText != otherModel.WholeText)
                result = result | CompareResult.Text_WholeText;
            
            return result;
        }

        public override CompareResult Compare(Node other)
        {
            Text candidate = other as Text;
            if (candidate == null)
                return CompareResult.NodeTypeMismatch;
            return this.CompareWithText(candidate.Model);
        }
    }
}
