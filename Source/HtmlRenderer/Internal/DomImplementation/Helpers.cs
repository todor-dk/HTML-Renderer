using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal static class Helpers
    {
        public static string GetTextContent(this Node self)
        {
            // The concatenation of data of all the Text node descendants of the context object, in tree order.
            throw new NotImplementedException();
        }

        public static string SetTextContent(this Node self, string data)
        {
            if (data == null)
                data = String.Empty;

            // 1. Let node be null.
            // 2. If new value is not the empty string, set node to a new Text node whose data is new value.
            // 3. Replace all with node within the context object.
            throw new NotImplementedException();
        }
    }
}
