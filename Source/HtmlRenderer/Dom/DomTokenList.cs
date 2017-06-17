using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#domtokenlist
    /// </summary>
    public interface DomTokenList : IReadOnlyList<string>
    {
        int Length { get; }

        string Item(int index);

        bool Contains(string token);

        void Add(params string[] tokens);

        void Remove(params string[] tokens);

        void Toggle(string token, bool force);

        string ToString();
    }
}
