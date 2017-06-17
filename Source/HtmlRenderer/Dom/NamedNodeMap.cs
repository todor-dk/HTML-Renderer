using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom
{
    public interface NamedNodeMap
    {
        /// <summary>
        /// Returns the indexth item in the map. If index is greater than or equal to the number of nodes in this map, this returns null.
        /// </summary>
        /// <param name="index">Index into this map.</param>
        /// <returns>The node at the indexth position in the map, or null if that is not a valid index.</returns>
        Attr Item(int index);

        int Length { get; }

        /// <summary>
        /// Retrieves a node specified by name.
        /// </summary>
        /// <param name="name">The nodeName of a node to retrieve.</param>
        /// <returns>A Node (of any type) with the specified nodeName, or null if it does not identify any node in this map.</returns>
        Attr GetNamedItem(string name);

        /// <summary>
        /// Retrieves a node specified by local name and namespace URI.
        /// </summary>
        /// <param name="namespaceUri">The namespace URI of the node to retrieve.</param>
        /// <param name="localName">The local name of the node to retrieve.</param>
        /// <returns>A Attr (of any type) with the specified local name and namespace URI, or null if they do not identify any node in this map.</returns>
        Attr GetNamedItemNS(string namespaceUri, string localName);

#if NOT_IMPLEMENTED


        Attr SetNamedItem(Attr arg);

        Attr RemoveNamedItem(string name);


        Attr SetNamedItemNS(Attr arg);

        Attr RemoveNamedItemNS(string namespaceUri, string localName);

#endif
    }
}
