using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public sealed class ReferenceText : ReferenceCharacterData, Text
    {
        public ReferenceText(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            this.WholeText = reader.ReadString("WholeText");
        }

        public string WholeText { get; private set; }

        protected override void AcceptOverride(IConcreteVisitor visitor)
        {
            visitor.VisitText(this);
        }
        
        #region Text interface

        Text Text.SplitText(int offset)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
