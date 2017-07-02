using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scientia.HtmlRenderer.Internal.DomImplementation
{
    internal class ClassAttr : NormalAttr, Dom.DomTokenList
    {
        public ClassAttr(string localName, string value)
            : base(localName, value)
        {
        }

        string IReadOnlyList<string>.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Length
        {
            get
            {
                return 0;
            }
        }

        int IReadOnlyCollection<string>.Count
        {
            get
            {
                return this.Length;
            }
        }

        public void Add(params string[] tokens)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string token)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public string Item(int index)
        {
            throw new NotImplementedException();
        }

        public void Remove(params string[] tokens)
        {
            throw new NotImplementedException();
        }

        public void Toggle(string token, bool force)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
