using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom
{
    public class CompareContext
    {
        internal readonly HashSet<ReferenceNode> ComparedNodes = new HashSet<ReferenceNode>();

        internal readonly Dictionary<ReferenceNode, bool> CompareResults = new Dictionary<ReferenceNode, bool>();

        public bool IgnoreBaseUriExceptForElementAndDocument;

        public bool IgnoreChildrenPropertiesExceptForElement;

        public bool IgnoreDocumentUri;

        public bool IgnoreDocumentOrigin;
        
    }
}
