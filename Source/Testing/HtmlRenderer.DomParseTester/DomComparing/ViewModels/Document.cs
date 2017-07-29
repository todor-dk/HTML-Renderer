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

        protected CompareResult CompareWithDocument(ReferenceDocument otherModel)
        {
            CompareResult result = this.CompareWithParentNode(otherModel);

            if (this.Model.CharacterSet != otherModel.CharacterSet)
                result = result | CompareResult.Document_CharacterSet;

            if (this.Model.CompatMode != otherModel.CompatMode)
                result = result | CompareResult.Document_CompatMode;

            if (this.Model.ContentType != otherModel.ContentType)
                result = result | CompareResult.Document_ContentType;

            if (this.Model.DocumentElement != otherModel.DocumentElement)
                result = result | CompareResult.Document_DocumentElement;

            if (this.Model.DocType != otherModel.DocType)
                result = result | CompareResult.Document_DocumentType;

            if (this.Model.DocumentUri != otherModel.DocumentUri)
                result = result | CompareResult.Document_DocumentUri;

            if (this.Model.Origin != otherModel.Origin)
                result = result | CompareResult.Document_Origin;

            if (this.Model.QuirksMode != otherModel.QuirksMode)
                result = result | CompareResult.Document_QuirksMode;

            if (this.Model.Url != otherModel.Url)
                result = result | CompareResult.Document_Url;

            return result;
        }

        public override CompareResult Compare(Node other)
        {
            Document candidate = other as Document;
            if (candidate == null)
                return CompareResult.NodeTypeMismatch;
            return this.CompareWithDocument(candidate.Model);
        }
    }
}
