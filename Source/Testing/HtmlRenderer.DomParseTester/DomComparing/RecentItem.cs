using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlRenderer.DomParseTester.DomComparing
{
    public class RecentItem : IEquatable<RecentItem>
    {
        public string Path { get; set; }

        public FileType Type { get; set; }

        public RecentItem(string path, FileType type)
        {
            this.Path = path;
            this.Type = type;
        }

        public bool Equals(RecentItem other)
        {
            return (this.Type == other.Type) && String.Equals(this.Path, other.Path, StringComparison.InvariantCultureIgnoreCase);
        }

        public enum FileType
        {
            Dom,
            Html
        }

        public override string ToString()
        {
            if (this.Type == FileType.Dom)
                return "DOM: " + this.Path;
            else
                return "HTML: " + this.Path;
        }
    }
}
