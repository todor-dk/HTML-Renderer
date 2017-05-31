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
        public CharacterData(Document document)
            : base(document)
        {
        }

        public string Data { get; set; }

        public int Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Element NextElementSibling
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dom.Element PreviousElementSibling
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// The value of this node, depending on its type.
        /// For <see cref="Text"/> and <see cref="Comment"/>, this is the textual <see cref="CharacterData.Data"/> of the node.
        /// For other nodes, this is null.
        /// </summary>
        public override string NodeValue
        {
            get { return this.Data; }
            set { this.Data = value; }
        }

        /// <summary>
        /// The text content of a node and its descendants.
        /// </summary>
        public override string TextContent
        {
            get { return this.Data; }
            set { this.ReplaceData(0, value?.Length ?? 0, value); }
        }

        public void AppendData(string data)
        {
            throw new NotImplementedException();
        }

        public void DeleteData(int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void InsertData(int offset, string data)
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void ReplaceData(int offset, int count, string data)
        {
            throw new NotImplementedException();
        }

        public string SubstringData(int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
