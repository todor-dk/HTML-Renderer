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

        protected CompareResult CompareWithElement(ReferenceElement otherModel)
        {
            CompareResult result = this.CompareWithParentNode(otherModel);

            if (this.Model.Attributes.Count != otherModel.Attributes.Count)
            {
                result = result | CompareResult.Element_AttributeCount;
            }
            else
            {
                List<ReferenceAttr> attrs = new List<ReferenceAttr>(otherModel.Attributes);

                foreach(ReferenceAttr attr in this.Model.Attributes)
                {
                    int idx = attrs.FindIndex(at => at.CompareName(attr));
                    if (idx != -1)
                    {
                        ReferenceAttr at = attrs[idx];
                        attrs.RemoveAt(idx);
                        if (attr.Value == at.Value)
                            continue;
                    }

                    result = result | CompareResult.Element_AttributeValue;
                    break;
                }
            }

            if (!this.Model.ClassList.ArraysEquals(otherModel.ClassList))
                result = result | CompareResult.Element_ClassList;

            if (this.Model.Id != otherModel.Id)
                result = result | CompareResult.Element_Id;

            if (this.Model.LocalName != otherModel.LocalName)
                result = result | CompareResult.Element_LocalName;

            if (this.Model.NamespaceUri != otherModel.NamespaceUri)
                result = result | CompareResult.Element_NamespaceUri;

            if (this.Model.Prefix != otherModel.Prefix)
                result = result | CompareResult.Element_Prefix;

            if (this.Model.TagName != otherModel.TagName)
                result = result | CompareResult.Element_TagName;

            return result;
        }

        public override CompareResult Compare(Node other)
        {
            Element candidate = other as Element;
            if (candidate == null)
                return CompareResult.NodeTypeMismatch;
            return this.CompareWithElement(candidate.Model);
        }
    }
}
