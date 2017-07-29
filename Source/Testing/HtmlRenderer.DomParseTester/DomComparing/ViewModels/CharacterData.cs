using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing.ViewModels
{
    public abstract class CharacterData<TReferenceNode> : Node<TReferenceNode>
        where TReferenceNode : ReferenceCharacterData
    {
        public CharacterData(Context context, TReferenceNode model) : base(context, model)
        {
        }

        protected CompareResult CompareWithCharacterData(TReferenceNode otherModel)
        {
            CompareResult result = this.CompareWithNode(otherModel);

            if (this.Model.Data != otherModel.Data)
                result = result | CompareResult.CharacterData_Data;

            if (this.Model.Length != otherModel.Length)
                result = result | CompareResult.CharacterData_Length;

            return result;
        }
    }
}
