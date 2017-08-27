using Scientia.HtmlRenderer.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace HtmlRenderer.TestLib.Dom
{
    public class ReferenceDomTokenList : IReadOnlyList<string>, DomTokenList
    {
        public static ReferenceDomTokenList FromArray(string[] items)
        {
            if (items == null)
                return null;
            return new ReferenceDomTokenList(items);
        }

        private ReferenceDomTokenList(string[] items)
        {
            this.Items = items;
        }

        private readonly string[] Items;

        public string this[int index] => this.Items[index];

        public int Length => this.Items.Length;

        public int Count => this.Items.Length;

        int DomTokenList.Length => throw new NotImplementedException();

        int IReadOnlyCollection<string>.Count => throw new NotImplementedException();

        string IReadOnlyList<string>.this[int index] => throw new NotImplementedException();

        public void Add(params string[] tokens)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string token)
        {
            return this.Items.Contains(token);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return this.Items.OfType<string>().GetEnumerator();
        }

        string DomTokenList.Item(int index)
        {
            if ((index < 0) || (index >= this.Items.Length))
                return null;
            return this.Items[index];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void DomTokenList.Add(params string[] tokens)
        {
            throw new NotImplementedException();
        }

        void DomTokenList.Remove(params string[] tokens)
        {
            throw new NotImplementedException();
        }

        void DomTokenList.Toggle(string token, bool force)
        {
            throw new NotImplementedException();
        }
    }
}
