using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal class EmptyCollection : Dom.NodeList, Dom.HtmlCollection
    {
        public static readonly EmptyCollection Current = new EmptyCollection();

        private EmptyCollection()
        {
        }

        Dom.Node IReadOnlyList<Dom.Node>.this[int index]
        {
            get { throw new ArgumentOutOfRangeException(nameof(index)); }
        }

        Dom.Element IReadOnlyList<Dom.Element>.this[int index]
        {
            get { throw new ArgumentOutOfRangeException(nameof(index)); }
        }

        int IReadOnlyCollection<Dom.Node>.Count
        {
            get { return 0; }
        }

        int IReadOnlyCollection<Dom.Element>.Count
        {
            get { return 0; }
        }

        int NodeList.Length
        {
            get { return 0; }
        }

        int HtmlCollection.Length
        {
            get { return 0; }
        }

        IEnumerator<Dom.Node> IEnumerable<Dom.Node>.GetEnumerator()
        {
            return new Enumerator<Dom.Node>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator<Dom.Node>();
        }

        IEnumerator<Dom.Element> IEnumerable<Dom.Element>.GetEnumerator()
        {
            return new Enumerator<Dom.Element>();
        }

        Dom.Node NodeList.Item(int index)
        {
            return null;
        }

        Dom.Element HtmlCollection.Item(int index)
        {
            return null;
        }

        Dom.Element HtmlCollection.NamedItem(string name)
        {
            return null;
        }

        private class Enumerator<TItem> : IEnumerator<TItem>
        {
            public TItem Current
            {
                get { return default(TItem); }
            }

            object IEnumerator.Current
            {
                get { return default(TItem); }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
            }
        }
    }
}
