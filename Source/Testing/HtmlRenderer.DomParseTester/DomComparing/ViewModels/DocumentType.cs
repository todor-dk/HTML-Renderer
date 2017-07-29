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

        protected CompareResult CompareWithDocumentType(ReferenceDocumentType otherModel)
        {
            CompareResult result = this.CompareWithNode(otherModel);

            if (this.Model.Name != otherModel.Name)
                result = result | CompareResult.DocumentType_Name;

            if (this.Model.PublicId != otherModel.PublicId)
                result = result | CompareResult.DocumentType_PublicId;

            if (this.Model.SystemId != otherModel.SystemId)
                result = result | CompareResult.DocumentType_SystemId;

            return result;
        }

        public override CompareResult Compare(Node other)
        {
            DocumentType candidate = other as DocumentType;
            if (candidate == null)
                return CompareResult.NodeTypeMismatch;
            return this.CompareWithDocumentType(candidate.Model);
        }
    }
}
