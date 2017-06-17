using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom.Persisting
{
    public class BinaryReader : ReaderBase, IReader
    {
        public static ReferenceNode FromStream(System.IO.Stream stream)
        {

            BinaryReader reader = new BinaryReader(stream);
            reader.Read();
            reader.ResolveNodes();
            return reader.Root;
        }

        private readonly System.IO.BinaryReader Reader;  

        private BinaryReader(System.IO.Stream stream)
        {
            this.Reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8, true);
        }

        private void Read()
        {
            byte[] buffer = this.Reader.ReadBytes(BinaryWriter.Prefix.Length);
            if (!buffer.ArraysEquals(BinaryWriter.Prefix))
                throw new InvalidOperationException();

            while (true)
            {
                int id = this.Reader.ReadInt32();
                if (id == -1)
                    break;
                int type = this.ReadInt("NodeType");

                ReferenceNode node = this.ReadNode(type);
                this.NodeMap[id] = node;
                if (this.Root == null)
                    this.Root = node;
            }

            buffer = this.Reader.ReadBytes(BinaryWriter.Postfix.Length);
            if (!buffer.ArraysEquals(BinaryWriter.Postfix))
                throw new InvalidOperationException();
        }

        public override int? ReadNullableInt(string name)
        {
            if (this.Reader.ReadBoolean())
                return null;
            else
                return this.Reader.ReadInt32();
        }

        public override int ReadInt(string name)
        {
            return this.Reader.ReadInt32();
        }

        public override string ReadString(string name)
        {
            if (this.Reader.ReadBoolean())
                return null;
            else
                return this.Reader.ReadString();
        }

        public override void ReadNode(string name, Action<ReferenceNode> setter)
        {
            int id = this.Reader.ReadInt32();
            if (id == -1)
                this.ResolveNode(null, setter);
            else
                this.ResolveNode(id, setter);
        }

        public override ReferenceAttr ReadAttrib(ReferenceElement owner)
        {
            string namespaceUri = this.ReadString("NamespaceURI");
            string prefix = this.ReadString("Prefix");
            string localName = this.ReadString("LocalName");
            string name = this.ReadString("Name");
            string value = this.ReadString("Value");

            return new ReferenceAttr(owner, namespaceUri, prefix, localName, name, value);
        }

        public override string[] ReadStringList(string name)
        {
            int cnt = this.Reader.ReadInt32();
            if (cnt == -1)
                return null;

            string[] result = new string[cnt];
            for (int i = 0; i < cnt; i++)
                result[i] = this.ReadString(null);

            return null;
        }

        public override void ReadNodeList(string name, Action<ReferenceNode[]> setter)
        {
            int cnt = this.Reader.ReadInt32();
            if (cnt == -1)
            {
                setter(null);
                return;
            }

            if (cnt == 0)
            {
                setter(Array.Empty<ReferenceNode>());
                return;
            }

            int[] ids = new int[cnt];
            for (int i = 0; i < cnt; i++)
                ids[i] = this.Reader.ReadInt32();

            if (ids.Length != cnt)
                throw new InvalidOperationException();

            if (ids.All(id => this.NodeMap.ContainsKey(id)))
            {
                setter(ids.Select(id => this.NodeMap[id]).ToArray());
                return;
            }

            this.ResolveListActions.Add(new Tuple<int[], Action<ReferenceNode[]>>(ids, setter));
        }
    }
}
