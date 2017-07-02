// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

namespace Scientia.HtmlRenderer.Demo.Common
{
    /// <summary>
    /// Used to hold a single html sample with its name.
    /// </summary>
    public sealed class HtmlSample
    {
        private readonly string _Name;
        private readonly string _FullName;
        private readonly string _Html;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public HtmlSample(string name, string fullName, string html)
        {
            this._Name = name;
            this._FullName = fullName;
            this._Html = html;
        }

        public string Name
        {
            get { return this._Name; }
        }

        public string FullName
        {
            get { return this._FullName; }
        }

        public string Html
        {
            get { return this._Html; }
        }
    }
}