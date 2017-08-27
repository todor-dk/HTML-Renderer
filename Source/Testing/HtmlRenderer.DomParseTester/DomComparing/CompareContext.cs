using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlRenderer.TestLib;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.DomParseTester.DomComparing
{
    internal class CompareContext : HtmlRenderer.TestLib.CompareContext
    {
        private readonly Dictionary<Node, ViewModels.Node> LeftMap = new Dictionary<Node, ViewModels.Node>();
        private readonly Dictionary<Node, ViewModels.Node> RightMap = new Dictionary<Node, ViewModels.Node>();

        private static void MapChildren(ViewModels.Node node, Dictionary<Node, ViewModels.Node> map)
        {
            if (node == null)
                return;

            node.HierarchyResult = HierarchyResult.Valid;
            node.CompareResult = CompareResult.Equal;
            Node model = node.GetModel();
            map.Add(model, node);

            foreach (ViewModels.Node child in node.ChildNodes)
                MapChildren(child, map);
        }

        public CompareContext(ViewModels.Node left, ViewModels.Node right)
        {
            MapChildren(left, this.LeftMap);
            MapChildren(right, this.RightMap);
        }

        public override CompareResult CompareRecursive(Node self, Node other)
        {
            CompareResult result = base.CompareRecursive(self, other);
            if (self != null)
                this.LeftMap[self].CompareResult = result;
            if (other != null)
                this.RightMap[other].CompareResult = result;
            return result;
        }

        public override HierarchyResult ValidateRecursive(Node self, Document ownerDocument, Node parentNode, Node previousSibling, Node nextSibling, Element previousElementSibling, Element nextElementSibling)
        {
            HierarchyResult result = base.ValidateRecursive(self, ownerDocument, parentNode, previousSibling, nextSibling, previousElementSibling, nextElementSibling);

            ViewModels.Node node = null;
            this.LeftMap.TryGetValue(self, out node);
            if (node != null)
                node.HierarchyResult = result;
            this.RightMap.TryGetValue(self, out node);
            if (node != null)
                node.HierarchyResult = result;
            return result;
        }
    }
}
