using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer
{
    internal static class Extensions
    {
        public static TItem[] With<TItem>(this TItem[] self, params TItem[] elements)
        {
            int len = (self?.Length ?? 0) + (elements?.Length ?? 0);
            TItem[] result = new TItem[len];

            int j = 0;

            if ((self != null) && (self.Length != 0))
            {
                for (int i = 0; i < self.Length; i++)
                {
                    result[j] = self[i];
                    j++;
                }
            }

            if ((elements != null) && (elements.Length != 0))
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    result[j] = elements[i];
                    j++;
                }
            }

            return result;
        }

        public static TItem[] FailIfNull<TItem>(this TItem[] self)
        {
            if (self == null)
                throw new NullReferenceException();

            return self;
        }
    }
}
