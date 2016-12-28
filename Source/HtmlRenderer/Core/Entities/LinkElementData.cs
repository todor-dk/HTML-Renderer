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

namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
    /// <summary>
    /// Holds data on link element in HTML.<br/>
    /// Used to expose data outside of HTML Renderer internal structure.
    /// </summary>
    public sealed class LinkElementData<T>
    {
        /// <summary>
        /// the id of the link element if present
        /// </summary>
        private readonly string _Id;

        /// <summary>
        /// the href data of the link
        /// </summary>
        private readonly string _Href;

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        private readonly T _Rectangle;

        /// <summary>
        /// Init.
        /// </summary>
        public LinkElementData(string id, string href, T rectangle)
        {
            this._Id = id;
            this._Href = href;
            this._Rectangle = rectangle;
        }

        /// <summary>
        /// the id of the link element if present
        /// </summary>
        public string Id
        {
            get { return this._Id; }
        }

        /// <summary>
        /// the href data of the link
        /// </summary>
        public string Href
        {
            get { return this._Href; }
        }

        /// <summary>
        /// the rectangle of element as calculated by html layout
        /// </summary>
        public T Rectangle
        {
            get { return this._Rectangle; }
        }

        /// <summary>
        /// Is the link is directed to another element in the html
        /// </summary>
        public bool IsAnchor
        {
            get { return this._Href.Length > 0 && this._Href[0] == '#'; }
        }

        /// <summary>
        /// Return the id of the element this anchor link is referencing.
        /// </summary>
        public string AnchorId
        {
            get { return this.IsAnchor && this._Href.Length > 1 ? this._Href.Substring(1) : string.Empty; }
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, Href: {1}, Rectangle: {2}", this._Id, this._Href, this._Rectangle);
        }
    }
}