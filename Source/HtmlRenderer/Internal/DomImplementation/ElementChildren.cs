using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    /*
        Our goal is a reduce memory footprint combined with decent performance.
        Reducing the memory footprint itself brings performance.
        Compacting the data in a CPU cache friendly way increases performance.
        Therefore, some algorythms that may look stupid are actually faster,
        for example, it is cheaper to brute force search a small array than
        to have hashed table, linked list collection or similar data structure.


        There are many elements that have only ONE child node. If we use a normal
        collection of child nodes, we will need an overhead of object_header +
        length + reference_pointer. On 32-bit that is minimum 16 bytes. On top of
        that, we still need the reference to the array.

        To optimize for this, if we have ONLY ONE CHILD, instead of storing a
        reference to an array with one item, we store the child directly. When we
        read, if we find that the slot contains a child and NOT an array, we know
        it is the single item.


        Many elements contion FEW child nodes. To preserve memory and reduce the
        number of lookups the CPU needs to make, we will use the most basic way of
        storing the children, i.e. an ARRAY. The overhead is minimal and the data
        is CPU friendly. The disadvantage is that when adding to the array, we will
        grow it by EXACTLY ONE item, which results in a copy of the entire array
        for each item that is added to it. This however is not a huge issue,
        because we employ this trategy only for smaller arrays. The copy operations
        are very fast, because the array can be copied with a few CPU register
        operations.


        Few elements contain MANY child nodes. When the collection of children
        becomes large, the above strategy is no longer feasable. We then swtich
        to a strategy that employs a complex collection object like a List<>
        collection (or similar). This object has a larger overhead, but that is
        justifiable, because only few elements will have many children and need
        to use this strategy.


        When it comed to determining the number of children, we use the contents
        to determine the count. The logis is as follows:
        - IF children is null
            - return 0
        - If children is a node
            - return 1
        - IF children is an array
            - return array.Length  (the array will be utilized 100% and never contain unused slots)
        - IF children is complex_collection
            - return complex_collection.Count  (the collection has a field that contains the count)
        - Implementation error - should not happen.

        ------------------------------------------------------------------------

        We are required to keep a collection of both elements and nodes (elements,
        texts, comments etc). Keeping two collections will waste memory and
        introduce overhead. So, following the above strategy and where we have
        zero, one or few elements, we will optimize the additional collection
        away and use brute force apprach. This is still cheaper.

        - IF children is null
            - no elements
        - If children is a node
            - one element, of the child is element, otherwise, no elements
        - IF children is an array
            - brute-force examine each slot if it is element.
        - IF children is complex_collection
            - complex collections keep separate track of elements.


        Some functions require us to find a child given its name. Again, we apply
        similar strategy. For zero, one or few children, we use brute-force.
        For large number of children, we will still use brute force, and if this is
        too slow, the complex children collection will need to keep map of names.
    */
    internal struct ElementChildren
    {
        private object Content;

        internal bool HasNodes()
        {
            return this.Content != null;
        }

        internal int GetNodeCount()
        {
            if (this.Content == null)
                return 0;
            if (this.Content is Node)
                return 1;
            Node[] nodes = this.Content as Node[];
            if (nodes != null)
                return nodes.Length;
            return ((ComplexContentCollection)this.Content).Count;
        }

        internal Node GetNode(int index)
        {
            if (index < 0)
                return null;

            if (this.Content == null)
                return null;

            Node node = this.Content as Node;
            if (node != null)
            {
                if (index == 0)
                    return node;
                else
                    return null;
            }

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                if (index < nodes.Length)
                    return nodes[index];
                else
                    return null;
            }

            return ((ComplexContentCollection)this.Content).GetNode(index);
        }

        internal bool HasElements()
        {
            if (this.Content == null)
                return false;

            Node node = this.Content as Node;
            if (node != null)
                return node is Element;

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] is Element)
                        return true;
                }

                return false;
            }

            return ((ComplexContentCollection)this.Content).ElementCount != 0;
        }

        internal int GetElementCount()
        {
            if (this.Content == null)
                return 0;

            Node node = this.Content as Node;
            if (node != null)
                return (node is Element) ? 1 : 0;

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                int cnt = 0;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] is Element)
                        cnt++;
                }

                return cnt;
            }

            return ((ComplexContentCollection)this.Content).ElementCount;
        }

        internal Element GetElement(int index)
        {
            if (index < 0)
                return null;

            if (this.Content == null)
                return null;

            Node node = this.Content as Node;
            if (node != null)
            {
                Element elem = node as Element;
                if ((index == 0) && (elem != null))
                    return elem;
                else
                    return null;
            }

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                int elementIndex = 0;
                for (int i = 0; i < nodes.Length; i++)
                {
                    Element elem = nodes[i] as Element;
                    if (elem != null)
                    {
                        if (elementIndex == index)
                            return elem;

                        elementIndex++;
                    }
                }

                return null;
            }

            return ((ComplexContentCollection)this.Content).GetElement(index);
        }

        internal Element GetNamedElement(string name)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#dom-htmlcollection-nameditem
            // 1. If key is the empty string, return null.
            if (String.IsNullOrEmpty(name))
                return null;

            // Brute force ... we may optimize in the future
            IEnumerator<Element> enumerator = this.GetElementEnumerator();
            while (enumerator.MoveNext())
            {
                // 2. Return the first element in the collection for which at least one of the following is true:
                //      * it has an ID which is key. 
                //      * it has a name attribute whose value is key;
                if (enumerator.Current.IsNamed(name))
                    return enumerator.Current;
            }

            // ... or null if there is no such element.
            return null;
        }

        internal IEnumerator<Node> GetNodeEnumerator()
        {
            if (this.Content == null)
                yield break;

            Node node = this.Content as Node;
            if (node != null)
            {
                yield return node;
                yield break;
            }

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                    yield return nodes[i];

                yield break;
            }

            IEnumerator<Node> enumerator = ((ComplexContentCollection)this.Content).GetNodeEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        internal IEnumerator<Element> GetElementEnumerator()
        {
            if (this.Content == null)
                yield break;

            Node node = this.Content as Node;
            if (node != null)
            {
                Element elem = node as Element;
                if (elem != null)
                    yield return elem;

                yield break;
            }

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    Element elem = nodes[i] as Element;
                    if (elem != null)
                        yield return elem;
                }

                yield break;
            }

            IEnumerator<Element> enumerator = ((ComplexContentCollection)this.Content).GetElementEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        internal Element GetLastElement()
        {
            if (this.Content == null)
                return null;

            Node node = this.Content as Node;
            if (node != null)
                return node as Element;

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    Element elem = nodes[i] as Element;
                    if (elem != null)
                        return elem;
                }

                return null;
            }

            return ((ComplexContentCollection)this.Content).GetLastElement();
        }

        internal Element GetNextElementSibling(Node node)
        {
            if (this.Content == null)
                return null;
            if (this.Content is Node)
                return null;

            int idx = node.NodeCollectionIndex;
            if (idx < 0)
                return null;

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = idx + 1; i < nodes.Length; i++)
                {
                    Element elem = nodes[i] as Element;
                    if (elem != null)
                        return elem;
                }

                return null;
            }

            return ((ComplexContentCollection)this.Content).GetNextElementSibling(node);
        }

        internal Element GetPreviousElementSibling(Node node)
        {
            if (this.Content == null)
                return null;
            if (this.Content is Node)
                return null;

            int idx = node.NodeCollectionIndex;
            if (idx < 0)
                return null;

            Node[] nodes = this.Content as Node[];
            if (nodes != null)
            {
                for (int i = idx - 1; i >= 0; i--)
                {
                    Element elem = nodes[i] as Element;
                    if (elem != null)
                        return elem;
                }

                return null;
            }

            return ((ComplexContentCollection)this.Content).GetPreviousElementSibling(node);
        }

        internal void InsertNode(ParentNode parent, Node node, Node child, bool suppressObservers)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-insert
            // To insert a node into a parent before a child with an optional suppress observers flag, run these steps:
            // NB: At this point, we expect that arguments are validated and legal.

            // 1. Let count be the number of children of node if it is a DocumentFragment node, and one otherwise.
            DocumentFragment df = node as DocumentFragment;
            int count = (df != null) ? df.ChildNodes.Length : 1;

            // 2. If child is non - null, run these substeps:
            if (child != null)
            {
                // 1. For each range whose start node is parent and start offset is greater than child's index, increase its start offset by count.
                // 2. For each range whose end node is parent and end offset is greater than child's index, increase its end offset by count.

                // TODO: The purpose of this is to patch existing Range objects that may be alive and in use.
            }

            // 3. Let nodes be node's children if node is a DocumentFragment node, and a list containing solely node otherwise.
            Node[] nodes = (df != null) ? df.ChildNodes.Cast<Node>().ToArray() : new Node[] { node };

            // 4. If node is a DocumentFragment node, queue a mutation record of "childList" for node with removedNodes nodes.
            // Note: This step intentionally does not pay attention to the suppress observers flag.
            if (df != null)
                throw new NotImplementedException();

            // 5. If node is a DocumentFragment node, remove its children with the suppress observers flag set.
            df?.RemoveAllChildNodes();

            // 6. If suppress observers flag is unset, queue a mutation record of "childList" for parent with addedNodes nodes,
            //    nextSibling child, and previousSibling child's previous sibling or parent's last child if child is null.
            if (!suppressObservers)
            {
                // TODO: Implement observers
            }

            // 7. For each newNode in nodes, in tree order, run these substeps:
            foreach (Node newNode in nodes)
            {
                // 1. Insert newNode into parent before child or at the end of parent if child is null.
                this.InsertNode(parent, newNode, child);

                // 2. Run the insertion steps with newNode.
                newNode.RunInsertionSteps();
            }
        }

        private void InsertNode(ParentNode parent, Node newChild, Node referenceChild)
        {
            // Insert newNode into parent before referenceChild or at the end of parent if referenceChild is null.
            // NB: At this point, we expect that arguments are validated and legal.
            if (this.Content == null)
            {
                Contract.RequiresNull(referenceChild, nameof(referenceChild));
                this.Content = newChild;
                newChild.SetParent(parent, 0);
                return;
            }

            Node existing = this.Content as Node;
            if (existing != null)
            {
                if (existing == referenceChild)
                {
                    this.Content = new Node[] { newChild, existing };
                    newChild.SetParent(parent, 0);
                    existing.SetCollectionIndex(1);
                }
                else
                {
                    Contract.RequiresNull(referenceChild, nameof(referenceChild));
                    this.Content = new Node[] { existing, newChild };
                    newChild.SetParent(parent, 1);
                }

                return;
            }

            Node[] existingNodes = this.Content as Node[];
            if (existingNodes != null)
            {
                if (existingNodes.Length == ElementChildren.ComplexThreshold)
                {
                    // Switch to complex
                    ComplexContentCollection collection = new ComplexContentCollection(existingNodes);
                    collection.InsertNode(parent, newChild, referenceChild);
                    this.Content = collection;
                    return;
                }

                Node[] newContents = new Node[existingNodes.Length + 1];
                if (referenceChild == null)
                {
                    // If no reference child, don't waste time.
                    Array.Copy(existingNodes, newContents, existingNodes.Length);
                    newContents[existingNodes.Length] = newChild;
                    newChild.SetParent(parent, existingNodes.Length);
                }
                else
                {
                    int j = 0;
                    for (int i = 0; i < existingNodes.Length; i++)
                    {
                        Node candidate = existingNodes[i];
                        if (candidate == referenceChild)
                        {
                            newContents[j] = newChild;
                            newChild.SetParent(parent, j);
                            j++;
                        }

                        newContents[j] = candidate;
                        if (j != i)
                            candidate.SetCollectionIndex(j);

                        j++;
                    }
                }

                this.Content = newContents;
                return;
            }

            ((ComplexContentCollection)this.Content).InsertNode(parent, newChild, referenceChild);
        }

        internal void RemoveNode(ParentNode parent, Node node, bool suppressObservers)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-node-remove
            // NB: At this point, we expect that arguments are validated and legal.

            // To remove a node from a parent with an optional suppress observers flag set, run these steps:

            // 1. Let index be node's index.
            // 2. For each range whose start node is an inclusive descendant of node, set its start to(parent, index).
            // 3. For each range whose end node is an inclusive descendant of node, set its end to(parent, index).
            // 4. For each range whose start node is parent and start offset is greater than index, decrease its start offset by one.
            // 5. For each range whose end node is parent and end offset is greater than index, decrease its end offset by one.
            // TODO: The purpose of the above is to patch existing Range objects that may be alive and in use.

            // 6. Let oldPreviousSibling be node's previous sibling.
            Dom.Node oldPreviousSibling = node.PreviousSibling;

            // 7. If suppress observers flag is unset, queue a mutation record of "childList" for parent with removedNodes
            //    a list solely containing node, nextSibling node's next sibling, and previousSibling oldPreviousSibling.
            if (!suppressObservers)
            {
                // TODO: Implement observers
            }

            // 8. For each ancestor ancestor of node, if ancestor has any registered observers whose options's subtree is true,
            //    then for each such registered observer registered, append a transient registered observer whose observer and
            //    options are identical to those of registered and source which is registered to node's list of registered observers.
            // TODO: Implement observers

            // 9. Remove node from its parent.
            this.RemoveNode(node);

            // 10. Run the removing steps with node, parent, and oldPreviousSibling.
            node.RunRemovingSteps(parent, oldPreviousSibling);
        }

        private void RemoveNode(Node node)
        {
            // NB: At this point, we expect that arguments are validated and legal.
            int idx = node.NodeCollectionIndex;
            node.SetParent(null, 0); // Second param is not used.

            Node existing = this.Content as Node;
            if (existing != null)
            {
                System.Diagnostics.Debug.Assert(existing == node, "Expected the contents to be the node being removed.");
                System.Diagnostics.Debug.Assert(idx == 0, "Expected the node index to be zero.");
                this.Content = null;
                return;
            }

            Node[] existingNodes = this.Content as Node[];
            if (existingNodes != null)
            {
                System.Diagnostics.Debug.Assert((idx >= 0) && (idx < existingNodes.Length), "Expected the node index to be within the child collection range.");
                System.Diagnostics.Debug.Assert(existingNodes[idx] == node, "Expected the collection to contain the node being removed.");

                if (existingNodes.Length == 2)
                {
                    if (idx == 0)
                        this.Content = existingNodes[1].SetCollectionIndex(0);
                    else
                        this.Content = existingNodes[0];
                }
                else
                {
                    Node[] newContents = new Node[existingNodes.Length - 1];
                    if (idx != 0)
                        Array.Copy(existingNodes, newContents, idx);
                    for (int i = idx; i < newContents.Length; i++)
                        newContents[i] = existingNodes[i + 1].SetCollectionIndex(i);
                }

                return;
            }

            this.Content = ((ComplexContentCollection)this.Content).RemoveNode(node);
        }

        private const int ComplexThreshold = 10;

        private class ComplexContentCollection
        {
            public ComplexContentCollection(Node[] existingNodes)
            {

            }

            public int Count
            {
                get { return 0; }
            }

            public Node GetNode(int index)
            {
                return null;
            }

            public IEnumerator<Node> GetNodeEnumerator()
            {
                return null;
            }

            public int ElementCount
            {
                get { return 0; }
            }

            public Element GetElement(int index)
            {
                return null;
            }

            public IEnumerator<Element> GetElementEnumerator()
            {
                return null;
            }

            public void InsertNode(ParentNode parent, Node newChild, Node referenceChild)
            {

            }

            public object RemoveNode(Node node)
            {
                return this;
            }

            public Element GetLastElement()
            {
                return null;
            }

            public Element GetNextElementSibling(Node node)
            {
                return null;
            }

            public Element GetPreviousElementSibling(Node node)
            {
                return null;
            }
        }
    }
}
