using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester
{
    internal static class Extensions
    {
        /// <summary>
        /// Adds the elements of the specified collection <paramref name="collection"/> to the target collection <paramref name="self"/>.
        /// </summary>
        /// <typeparam name="TItem">The type of elements in the collection.</typeparam>
        /// <param name="self">Target collection.</param>
        /// <param name="collection">The collection whose elements should be added to the target collection.</param>
        /// <exception cref="ArgumentNullException"> is thrown if either <paramref name="self"/> or <paramref name="collection"/> is null.</exception>
        public static void AddRange<TItem>(this ICollection<TItem> self, IEnumerable<TItem> collection)
        {
            foreach (TItem item in collection)
                self.Add(item);
        }

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
    }
}
