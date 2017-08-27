using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib
{
    internal static class Extensions
    {
        public static bool ArraysEquals<TItem>(this TItem[] a, TItem[] b)
            where TItem : IEquatable<TItem>
        {
            if (a == b)
                return true;
            if ((a == null) || (b == null))
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i]))
                    return false;
            }

            return true;
        }

        public static bool ArraysEquals<TItem>(this TItem[] a, TItem[] b, Func<TItem, TItem, bool> comparer)
        {
            if (a == b)
                return true;
            if ((a == null) || (b == null))
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!comparer(a[i], b[i]))
                    return false;
            }

            return true;
        }

        public static bool ListsEquals<TItem>(this IReadOnlyList<TItem> a, IReadOnlyList<TItem> b)
            where TItem : IEquatable<TItem>
        {
            if (a == b)
                return true;
            if ((a == null) || (b == null))
                return false;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                    return false;
            }

            return true;
        }




        //public static bool IsHierarchyValidRecursive(this ReferenceNode self, ReferenceDocument ownerDocument, ReferenceNode parentNode, ReferenceNode previousSibling, ReferenceNode nextSibling)
        //{
        //    if (!self.IsHierarchyValid(ownerDocument, parentNode, previousSibling, nextSibling))
        //        return false;

        //    IReadOnlyList<ReferenceNode> children = self.ChildNodes;
        //    for (int i = 0; i < children.Count; i++)
        //    {
        //        ReferenceNode child = children[i];
        //        ReferenceNode previousChild = (i == 0) ? null : children[i - 1];
        //        ReferenceNode nextChild = (i == (children.Count - 1)) ? null : children[i + 1];
        //        if (!child.IsHierarchyValidRecursive(self, previousChild, nextChild))
        //            return false;
        //    }

        //    return true;
        //}

        //public static bool IsHierarchyValidRecursive(this ReferenceDocument self)
        //{
        //    return self.IsHierarchyValidRecursive(null, null, null);
        //}
    }
}
