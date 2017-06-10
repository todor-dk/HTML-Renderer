using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Dom;

namespace TheArtOfDev.HtmlRenderer.Internal.DomImplementation
{
    internal abstract class CharacterData : Node, Dom.CharacterData
    {
        private string Text;

        public CharacterData(Document document, string data = null)
            : base(document)
        {
            this.Text = data ?? String.Empty;
        }

        #region Node interface overrides

        /// The value of this node, depending on its type.
        /// For <see cref="Text"/> and <see cref="Comment"/>, this is the textual <see cref="CharacterData.Data"/> of the node.
        /// For other nodes, this is null.
        /// </summary>
        public override string NodeValue
        {
            get { return this.Text; }
            set { this.Data = value; }
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.Text; }
            set { this.Data = value; }
        }

        #endregion

        #region CharacterData interface

        /// <summary>
        /// The textual data contained in this object
        /// </summary>
        public string Data
        {
            get { return this.Text; }
            set { this.ReplaceData(0, this.Length, value); }
        }

        /// <summary>
        /// The size of the string contained in <see cref="Data"/>.
        /// </summary>
        public int Length
        {
            get { return this.Text?.Length ?? 0; }
        }

        /// <summary>
        /// Appends the given string to the <see cref="Data"/> string;
        /// when this method returns, <see cref="Data"/> contains the concatenated string.
        /// </summary>
        /// <param name="data">The string to add to the comment node.</param>
        public void AppendData(string data)
        {
            this.ReplaceData(this.Length, 0, data);
        }

        /// <summary>
        /// Removes the specified <paramref name="count"/> of characters, starting at the
        /// specified <paramref name="offset"/>, from the <see cref="Data"/> string;
        /// when this method returns, <see cref="Data"/> contains the shortened string.
        /// </summary>
        /// <param name="offset">Specifies where to begin removing characters. Start value starts at zero.</param>
        /// <param name="count">Specifies how many characters to delete.</param>
        public void DeleteData(int offset, int count)
        {
            this.ReplaceData(offset, count, String.Empty);
        }

        /// <summary>
        /// Inserts the specified characters, at the specified <paramref name="offset"/>, in the
        /// <see cref="Data"/> string; when this method returns, data contains the modified string.
        /// </summary>
        /// <param name="offset">Specifies where to begin inserting characters. Start value starts at zero.</param>
        /// <param name="data">Specifies the string to insert.</param>
        public void InsertData(int offset, string data)
        {
            this.ReplaceData(offset, 0, data);
        }

        /// <summary>
        /// Replaces the specified <paramref name="count"/> of characters, starting at the specified
        /// <paramref name="offset"/>, with the specified string; when this method returns,
        /// <see cref="Data"/> contains the modified string.
        /// </summary>
        /// <param name="offset">Specifies where to begin replacing characters. Start value starts at zero.</param>
        /// <param name="count">Specifies how many characters to replace.</param>
        /// <param name="data">Specifies the string to insert.</param>
        public void ReplaceData(int offset, int count, string data)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-cd-replace
            // To replace data of node node with offset offset, count count, and data data, run these steps:
            data = data ?? String.Empty;

            // 1. Let length be node's length attribute value.
            int length = this.Length;

            // 2. If offset is greater than length, throw an "IndexSizeError" exception.
            if (offset > length)
                throw new Dom.Exceptions.IndexSizeException();

            // 3. If offset plus count is greater than length let count be length minus offset.
            if ((offset + count) > length)
                count = length - offset;

            // 4. Queue a mutation record of "characterData" for node with oldValue node's data.
            // TODO: Implement observers

            // 5. Insert data into node's data after offset code units.
            this.Text = this.Text.Insert(offset, data).Remove(offset + data.Length, count);

            // 6. Let delete offset be offset plus the number of code units in data.
            // 7. Starting from delete offset code units, remove count code units from node's data.
            // 8. For each range whose start node is node and start offset is greater than offset but less than or equal to offset plus count, set its start offset to offset.
            // 9. For each range whose end node is node and end offset is greater than offset but less than or equal to offset plus count, set its end offset to offset.
            // 10. For each range whose start node is node and start offset is greater than offset plus count, increase its start offset by the number of code units in data, then decrease it by count.
            // 11. For each range whose end node is node and end offset is greater than offset plus count, increase its end offset by the number of code units in data, then decrease it by count.
            // TODO: The purpose of the above is to patch existing Range objects that may be alive and in use.
        }

        /// <summary>
        /// Returns a string containing the part of <see cref="Data"/> of the specified
        /// <paramref name="length"/> and starting at the specified <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">Specifies where to begin extracting characters. Start value starts at zero.</param>
        /// <param name="length">Specifies how many characters to extract.</param>
        /// <returns></returns>
        public string SubstringData(int offset, int count)
        {
            // See: http://www.w3.org/TR/2015/REC-dom-20151119/#concept-cd-substring

            // 1. Let length be node's length attribute value.
            int length = this.Length;

            // 2. If offset is greater than length, throw an "IndexSizeError" exception.
            if (offset > length)
                throw new Dom.Exceptions.IndexSizeException();

            // 3. If offset plus count is greater than length, return a string whose value is the code units
            //    from the offsetth code unit to the end of node's data, and then terminate these steps.
            if ((offset + count) > length)
                return this.Text.Substring(offset);

            // 4. Return a string whose value is the code units from the offsetth code unit to
            //    the offset+countth code unit in node's data.
            return this.Text.Substring(offset, count);
        }

        #endregion

        #region ChildNode interface

        /// <summary>
        /// Removes this node from its parent children list.
        /// </summary>
        public void Remove()
        {
            this._ParentNode?.RemoveNode(this);
        }

        #endregion

        #region NonDocumentTypeChildNode interface

        /// <summary>
        /// Returns the first following sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element NextElementSibling
        {
            get { return this._ParentNode?.GetNextElementSibling(this); }
        }

        /// <summary>
        /// Returns the first preceding sibling that is an element, and null otherwise.
        /// </summary>
        public Dom.Element PreviousElementSibling
        {
            get { return this._ParentNode.GetPreviousElementSibling(this); }
        }

        #endregion
    }
}
