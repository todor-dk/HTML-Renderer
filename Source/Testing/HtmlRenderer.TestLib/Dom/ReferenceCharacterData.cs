using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scientia.HtmlRenderer.Dom;

namespace HtmlRenderer.TestLib.Dom
{
    public abstract class ReferenceCharacterData : ReferenceNode, CharacterData
    {
        public ReferenceCharacterData(Persisting.IReader reader, Scientia.HtmlRenderer.Dom.NodeType type)
            : base(reader, type)
        {
            reader.ReadElement("PreviousElementSibling", elem => this.PreviousElementSibling = elem);
            reader.ReadElement("NextElementSibling", elem => this.NextElementSibling = elem);
            this.Data = reader.ReadString("Data");
            this.Length = reader.ReadInt("Length");
        }

        public string Data { get; private set; }

        public int Length { get; private set; }
        
        public ReferenceElement PreviousElementSibling { get; private set; }
        
        public ReferenceElement NextElementSibling { get; private set; }


        #region CharacterData interface

        string CharacterData.Data { get => this.Data; set => throw new NotImplementedException(); }
        
        Element NonDocumentTypeChildNode.PreviousElementSibling => this.PreviousElementSibling;

        Element NonDocumentTypeChildNode.NextElementSibling => this.NextElementSibling;

        void CharacterData.AppendData(string data)
        {
            throw new NotImplementedException();
        }

        void CharacterData.DeleteData(int offset, int count)
        {
            throw new NotImplementedException();
        }

        void CharacterData.InsertData(int offset, string data)
        {
            throw new NotImplementedException();
        }

        void ChildNode.Remove()
        {
            throw new NotImplementedException();
        }

        void CharacterData.ReplaceData(int offset, int count, string data)
        {
            throw new NotImplementedException();
        }

        string CharacterData.SubstringData(int offset, int length)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
