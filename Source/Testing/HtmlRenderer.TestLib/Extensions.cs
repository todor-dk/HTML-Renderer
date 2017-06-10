using HtmlRenderer.TestLib.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static bool Compare<TNode>(this TNode a, TNode b, CompareContext context)
            where TNode : ReferenceNode
        {
            if ((a == null) && (b == null))
                return true;
            if (a == null)
                return false;
            if (b == null)
                return false;

            bool result;
            if (context.CompareResults.TryGetValue(a, out result))
                return result;
            if (context.CompareResults.TryGetValue(b, out result))
                return result;

            if (context.ComparedNodes.Contains(a))
                return true;
            if (context.ComparedNodes.Contains(b))
                return true;
            context.ComparedNodes.Add(a);
            context.ComparedNodes.Add(b);

            result = a.CompareWith(b, context);
            context.CompareResults[a] = result;
            context.CompareResults[b] = result;

            return result;
        }


        public static bool CompareCollection<TNode>(this IReadOnlyList<TNode> a, IReadOnlyList<TNode> b, CompareContext context)
            where TNode : ReferenceNode
        {
            if ((a == null) && (b == null))
                return true;
            if (a == null)
                return false;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Compare(b[i], context))
                    return false;
            }

            return true;
        }

        public static bool CompareCollection(this IReadOnlyList<ReferenceAttr> a, IReadOnlyList<ReferenceAttr> b, CompareContext context)
        {
            if ((a == null) && (b == null))
                return true;
            if (a == null)
                return false;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                ReferenceAttr aa = a[i];
                ReferenceAttr ab = b[i];
                if ((a == null) && (b == null))
                    continue;

                if (!a[i].CompareWith(b[i], context))
                    return false;
            }

            return true;
        }
    }
}
