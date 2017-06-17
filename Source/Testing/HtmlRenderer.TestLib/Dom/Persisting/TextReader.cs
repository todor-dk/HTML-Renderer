using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.TestLib.Dom.Persisting
{
    public class TextReader : ReaderBase, IReader
    {
        public static ReferenceNode FromData(string data)
        {
            TextReader reader = new TextReader(data);
            reader.Read();
            reader.ResolveNodes();
            return reader.Root;
        }

        private readonly System.IO.StringReader Reader;

        private TextReader(string data)
        {
            this.Reader = new System.IO.StringReader(data);
        }

        private void Read()
        {
            while(true)
            {
                string line = this.Reader.ReadLine()?.Trim();
                if (line == null)
                    return;
                if (line.Length == 0)
                    continue;
                if (!line.StartsWith("NodeId : ", StringComparison.Ordinal))
                    throw new InvalidOperationException();
                line = line.Substring(9);
                int id = Int32.Parse(line);
                int type = this.ReadInt("NodeType");

                ReferenceNode node = this.ReadNode(type);
                this.NodeMap[id] = node;
                if (this.Root == null)
                    this.Root = node;
            }
        }

        public override int ReadInt(string name)
        {
            string line = this.Reader.ReadLine()?.Trim();
            if ((line == null) || !line.StartsWith(name + " : ", StringComparison.Ordinal))
                throw new InvalidOperationException();

            string value = line.Substring(name.Length + 3);
            return Int32.Parse(value);
        }

        public override int? ReadNullableInt(string name)
        {
            string line = this.Reader.ReadLine()?.Trim();
            if ((line == null) || !line.StartsWith(name + " : ", StringComparison.Ordinal))
                throw new InvalidOperationException();

            string value = line.Substring(name.Length + 3);
            if ((value == "undefined") || (value == "null"))
                return null;
            return Int32.Parse(value);
        }

        public override string ReadString(string name)
        {
            string line = this.Reader.ReadLine()?.Trim();
            if ((line == null) || !line.StartsWith(name + " : ", StringComparison.Ordinal))
                throw new InvalidOperationException();

            string value = line.Substring(name.Length + 3);
            return TextReader.DecodeString(value);
        }

        public override void ReadNode(string name, Action<ReferenceNode> setter)
        {
            int? nodeId = this.ReadNullableInt(name);
            this.ResolveNode(nodeId, setter);
        }

        public override ReferenceAttr ReadAttrib(ReferenceElement owner)
        {
            string line = this.Reader.ReadLine()?.Trim();
            if (line == null)
                throw new InvalidOperationException();
            if (line.Length != 0)
                throw new InvalidOperationException();

            string namespaceUri = this.ReadString("NamespaceURI");
            string prefix = this.ReadString("Prefix");
            string localName = this.ReadString("LocalName");
            string name = this.ReadString("Name");
            string value = this.ReadString("Value");

            return new ReferenceAttr(owner, namespaceUri, prefix, localName, name, value);
        }

        public override string[] ReadStringList(string name)
        {
            string data = this.ReadString(name)?.Trim();
            if ((data == null) || (data == "null") || (data == "undefined"))
                return null;

            string[] parts = data.Split(':');
            if (parts.Length != 2)
                throw new InvalidOperationException();

            int cnt = Int32.Parse(parts[0].Trim());
            if (cnt == 0)
                return Array.Empty<string>();

            parts = parts[1].Split(',').Select(str => TextReader.DecodeString(str.Trim())).ToArray();
            if (parts.Length != cnt)
                throw new InvalidOperationException();

            return parts;
        }

        public override void ReadNodeList(string name, Action<ReferenceNode[]> setter)
        {
            string data = this.ReadString(name)?.Trim();
            if ((data == null) || (data == "null") || (data == "undefined"))
            {
                setter(null);
                return;
            }
                
            string[] parts = data.Split(':');
            if (parts.Length != 2)
                throw new InvalidOperationException();

            int cnt = Int32.Parse(parts[0].Trim());
            if (cnt == 0)
            {
                setter(Array.Empty<ReferenceNode>());
                return;
            }

            int[] ids = parts[1].Split(',').Select(id => Int32.Parse(id.Trim())).ToArray();

            if (ids.Length != cnt)
                throw new InvalidOperationException();

            if (ids.All(id => this.NodeMap.ContainsKey(id)))
            {
                setter(ids.Select(id => this.NodeMap[id]).ToArray());
                return;
            }

            this.ResolveListActions.Add(new Tuple<int[], Action<ReferenceNode[]>>(ids, setter));
        }

        private static string DecodeString(string value)
        {
            if (value.Length == 0)
                throw new InvalidOperationException();

            if (value == "=== null")
                return null;
            if (value == "=== undefined")
                return null;
            if (value == "=== empty")
                return String.Empty;

            if (!value.StartsWith("== "))
                return value;

            // Must decode.
            byte[] utf8 = Convert.FromBase64String(value.Substring(3));
            return System.Text.Encoding.UTF8.GetString(utf8);
        }
    }
}
