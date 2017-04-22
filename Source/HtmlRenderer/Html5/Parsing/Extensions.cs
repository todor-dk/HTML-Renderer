using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    internal static class Extensions
    {
        public static bool Remove<TItem>(this Stack<TItem> self, TItem item, EqualityComparer<TItem> comparer = null)
        {
            Contract.RequiresNotNull(self, nameof(self));

            if (self.Count == 0)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<TItem>.Default;
            bool found = false;
            Stack<TItem> saved = new Stack<TItem>(self.Count);
            while (self.Count != 0)
            {
                TItem candidate = self.Pop();
                if (comparer.Equals(candidate, item))
                {
                    found = true;
                    break;
                }

                saved.Push(candidate);
            }

            while (saved.Count != 0)
                self.Push(saved.Pop());

            return found;
        }
    }
}
