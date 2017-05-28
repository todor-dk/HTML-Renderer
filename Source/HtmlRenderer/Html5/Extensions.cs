using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Html5
{
    internal static class Extensions
    {
        public static bool IsInSameSubtree(this Element self, Element node)
        {
            // A node’s home subtree is the subtree rooted at that node’s root element.
            // When a node is in a Document, its home subtree is that Document's tree.
            return self.GetRoot() == node.GetRoot();
        }

        public static Node GetRoot(this Element self)
        {
            Contract.RequiresNotNull(self, nameof(self));

            Node node = self;
            while (true)
            {
                Node parent = node.ParentNode;
                if (parent == null)
                    return node;
                node = parent;
            }
        }
    }
}
