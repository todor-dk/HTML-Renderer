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

using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Handlers;

namespace Scientia.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Represents a word inside an inline box
    /// </summary>
    /// <remarks>
    /// Because of performance, words of text are the most atomic
    /// element in the project. It should be characters, but come on,
    /// imagine the performance when drawing char by char on the device.<br/>
    /// It may change for future versions of the library.
    /// </remarks>
    internal abstract class CssRect
    {
        #region Fields and Consts

        /// <summary>
        /// the CSS box owner of the word
        /// </summary>
        private readonly CssBox _OwnerBox;

        /// <summary>
        /// Rectangle
        /// </summary>
        private RRect Rect;

        /// <summary>
        /// If the word is selected this points to the selection handler for more data
        /// </summary>
        private SelectionHandler _Selection;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="owner">the CSS box owner of the word</param>
        protected CssRect(CssBox owner)
        {
            this._OwnerBox = owner;
        }

        /// <summary>
        /// Gets the Box where this word belongs.
        /// </summary>
        public CssBox OwnerBox
        {
            get { return this._OwnerBox; }
        }

        /// <summary>
        /// Gets or sets the bounds of the rectangle
        /// </summary>
        public RRect Rectangle
        {
            get { return this.Rect; }
            set { this.Rect = value; }
        }

        /// <summary>
        /// Left of the rectangle
        /// </summary>
        public double Left
        {
            get { return this.Rect.X; }
            set { this.Rect.X = value; }
        }

        /// <summary>
        /// Top of the rectangle
        /// </summary>
        public double Top
        {
            get { return this.Rect.Y; }
            set { this.Rect.Y = value; }
        }

        /// <summary>
        /// Width of the rectangle
        /// </summary>
        public double Width
        {
            get { return this.Rect.Width; }
            set { this.Rect.Width = value; }
        }

        /// <summary>
        /// Get the full width of the word including the spacing.
        /// </summary>
        public double FullWidth
        {
            get { return this.Rect.Width + this.ActualWordSpacing; }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public double ActualWordSpacing
        {
            get { return this.OwnerBox != null ? (this.HasSpaceAfter ? this.OwnerBox.ActualWordSpacing : 0) + (this.IsImage ? this.OwnerBox.ActualWordSpacing : 0) : 0; }
        }

        /// <summary>
        /// Height of the rectangle
        /// </summary>
        public double Height
        {
            get { return this.Rect.Height; }
            set { this.Rect.Height = value; }
        }

        /// <summary>
        /// Gets or sets the right of the rectangle. When setting, it only affects the Width of the rectangle.
        /// </summary>
        public double Right
        {
            get { return this.Rectangle.Right; }
            set { this.Width = value - this.Left; }
        }

        /// <summary>
        /// Gets or sets the bottom of the rectangle. When setting, it only affects the Height of the rectangle.
        /// </summary>
        public double Bottom
        {
            get { return this.Rectangle.Bottom; }
            set { this.Height = value - this.Top; }
        }

        /// <summary>
        /// If the word is selected this points to the selection handler for more data
        /// </summary>
        public SelectionHandler Selection
        {
            get { return this._Selection; }
            set { this._Selection = value; }
        }

        /// <summary>
        /// was there a whitespace before the word chars (before trim)
        /// </summary>
        public virtual bool HasSpaceBefore
        {
            get { return false; }
        }

        /// <summary>
        /// was there a whitespace after the word chars (before trim)
        /// </summary>
        public virtual bool HasSpaceAfter
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the image this words represents (if one exists)
        /// </summary>
        public virtual RImage Image
        {
            get { return null; }

            // ReSharper disable ValueParameterNotUsed
            set { }

            // ReSharper restore ValueParameterNotUsed
        }

        /// <summary>
        /// Gets if the word represents an image.
        /// </summary>
        public virtual bool IsImage
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a bool indicating if this word is composed only by spaces.
        /// Spaces include tabs and line breaks
        /// </summary>
        public virtual bool IsSpaces
        {
            get { return true; }
        }

        /// <summary>
        /// Gets if the word is composed by only a line break
        /// </summary>
        public virtual bool IsLineBreak
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the text of the word
        /// </summary>
        public virtual string Text
        {
            get { return null; }
        }

        /// <summary>
        /// is the word is currently selected
        /// </summary>
        public bool Selected
        {
            get { return this._Selection != null; }
        }

        /// <summary>
        /// the selection start index if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public int SelectedStartIndex
        {
            get { return this._Selection != null ? this._Selection.GetSelectingStartIndex(this) : -1; }
        }

        /// <summary>
        /// the selection end index if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public int SelectedEndIndexOffset
        {
            get { return this._Selection != null ? this._Selection.GetSelectedEndIndexOffset(this) : -1; }
        }

        /// <summary>
        /// the selection start offset if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public double SelectedStartOffset
        {
            get { return this._Selection != null ? this._Selection.GetSelectedStartOffset(this) : -1; }
        }

        /// <summary>
        /// the selection end offset if the word is partially selected (-1 if not selected or fully selected)
        /// </summary>
        public double SelectedEndOffset
        {
            get { return this._Selection != null ? this._Selection.GetSelectedEndOffset(this) : -1; }
        }

        /// <summary>
        /// Gets or sets an offset to be considered in measurements
        /// </summary>
        internal double LeftGlyphPadding
        {
            get { return this.OwnerBox != null ? this.OwnerBox.ActualFont.LeftPadding : 0; }
        }

        /// <summary>
        /// Represents this word for debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1} char{2})", this.Text.Replace(' ', '-').Replace("\n", "\\n"), this.Text.Length, this.Text.Length != 1 ? "s" : string.Empty);
        }

        public bool BreakPage()
        {
            var container = this.OwnerBox.HtmlContainer;

            if (this.Height >= container.PageSize.Height)
                return false;

            var remTop = (this.Top - container.MarginTop) % container.PageSize.Height;
            var remBottom = (this.Bottom - container.MarginTop) % container.PageSize.Height;

            if (remTop > remBottom)
            {
                this.Top += container.PageSize.Height - remTop + 1;
                return true;
            }

            return false;
        }
    }
}