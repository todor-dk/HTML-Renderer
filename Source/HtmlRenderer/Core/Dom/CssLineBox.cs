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

using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Represents a line of text.
    /// </summary>
    /// <remarks>
    /// To learn more about line-boxes see CSS spec:
    /// http://www.w3.org/TR/CSS21/visuren.html
    /// </remarks>
    internal sealed class CssLineBox
    {
        #region Fields and Consts

        private readonly List<CssRect> _Words;
        private readonly CssBox _OwnerBox;
        private readonly Dictionary<CssBox, RRect> Rects;
        private readonly List<CssBox> _RelatedBoxes;

        #endregion

        /// <summary>
        /// Creates a new LineBox
        /// </summary>
        public CssLineBox(CssBox ownerBox)
        {
            this.Rects = new Dictionary<CssBox, RRect>();
            this._RelatedBoxes = new List<CssBox>();
            this._Words = new List<CssRect>();
            this._OwnerBox = ownerBox;
            this._OwnerBox.LineBoxes.Add(this);
        }

        /// <summary>
        /// Gets a list of boxes related with the linebox.
        /// To know the words of the box inside this linebox, use the <see cref="WordsOf"/> method.
        /// </summary>
        public List<CssBox> RelatedBoxes
        {
            get { return this._RelatedBoxes; }
        }

        /// <summary>
        /// Gets the words inside the linebox
        /// </summary>
        public List<CssRect> Words
        {
            get { return this._Words; }
        }

        /// <summary>
        /// Gets the owner box
        /// </summary>
        public CssBox OwnerBox
        {
            get { return this._OwnerBox; }
        }

        /// <summary>
        /// Gets a List of rectangles that are to be painted on this linebox
        /// </summary>
        public Dictionary<CssBox, RRect> Rectangles
        {
            get { return this.Rects; }
        }

        /// <summary>
        /// Get the height of this box line (the max height of all the words)
        /// </summary>
        public double LineHeight
        {
            get
            {
                double height = 0;
                foreach (var rect in this.Rects)
                {
                    height = Math.Max(height, rect.Value.Height);
                }

                return height;
            }
        }

        /// <summary>
        /// Get the bottom of this box line (the max bottom of all the words)
        /// </summary>
        public double LineBottom
        {
            get
            {
                double bottom = 0;
                foreach (var rect in this.Rects)
                {
                    bottom = Math.Max(bottom, rect.Value.Bottom);
                }

                return bottom;
            }
        }

        /// <summary>
        /// Lets the linebox add the word an its box to their lists if necessary.
        /// </summary>
        /// <param name="word"></param>
        internal void ReportExistanceOf(CssRect word)
        {
            if (!this.Words.Contains(word))
            {
                this.Words.Add(word);
            }

            if (!this.RelatedBoxes.Contains(word.OwnerBox))
            {
                this.RelatedBoxes.Add(word.OwnerBox);
            }
        }

        /// <summary>
        /// Return the words of the specified box that live in this linebox
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        internal List<CssRect> WordsOf(CssBox box)
        {
            List<CssRect> r = new List<CssRect>();

            foreach (CssRect word in this.Words)
            {
                if (word.OwnerBox.Equals(box))
                    r.Add(word);
            }

            return r;
        }

        /// <summary>
        /// Updates the specified rectangle of the specified box.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        internal void UpdateRectangle(CssBox box, double x, double y, double r, double b)
        {
            double leftspacing = box.ActualBorderLeftWidth + box.ActualPaddingLeft;
            double rightspacing = box.ActualBorderRightWidth + box.ActualPaddingRight;
            double topspacing = box.ActualBorderTopWidth + box.ActualPaddingTop;
            double bottomspacing = box.ActualBorderBottomWidth + box.ActualPaddingTop;

            if ((box.FirstHostingLineBox != null && box.FirstHostingLineBox.Equals(this)) || box.IsImage)
                x -= leftspacing;
            if ((box.LastHostingLineBox != null && box.LastHostingLineBox.Equals(this)) || box.IsImage)
                r += rightspacing;

            if (!box.IsImage)
            {
                y -= topspacing;
                b += bottomspacing;
            }

            if (!this.Rectangles.ContainsKey(box))
            {
                this.Rectangles.Add(box, RRect.FromLTRB(x, y, r, b));
            }
            else
            {
                RRect f = this.Rectangles[box];
                this.Rectangles[box] = RRect.FromLTRB(
                    Math.Min(f.X, x),
                    Math.Min(f.Y, y),
                    Math.Max(f.Right, r),
                    Math.Max(f.Bottom, b));
            }

            if (box.ParentBox != null && box.ParentBox.IsInline)
            {
                this.UpdateRectangle(box.ParentBox, x, y, r, b);
            }
        }

        /// <summary>
        /// Copies the rectangles to their specified box
        /// </summary>
        internal void AssignRectanglesToBoxes()
        {
            foreach (CssBox b in this.Rectangles.Keys)
            {
                b.Rectangles.Add(this, this.Rectangles[b]);
            }
        }

        /// <summary>
        /// Sets the baseline of the words of the specified box to certain height
        /// </summary>
        /// <param name="g">Device info</param>
        /// <param name="b">box to check words</param>
        /// <param name="baseline">baseline</param>
        internal void SetBaseLine(RGraphics g, CssBox b, double baseline)
        {
            // TODO: Aqui me quede, checar poniendo "by the" con un font-size de 3em
            List<CssRect> ws = this.WordsOf(b);

            if (!this.Rectangles.ContainsKey(b))
                return;

            RRect r = this.Rectangles[b];

            // Save top of words related to the top of rectangle
            double gap = 0f;

            if (ws.Count > 0)
            {
                gap = ws[0].Top - r.Top;
            }
            else
            {
                CssRect firstw = b.FirstWordOccourence(b, this);

                if (firstw != null)
                {
                    gap = firstw.Top - r.Top;
                }
            }

            // New top that words will have
            // float newtop = baseline - (Height - OwnerBox.FontDescent - 3); //OLD
            double newtop = baseline; // -GetBaseLineHeight(b, g); //OLD

            if (b.ParentBox != null &&
                b.ParentBox.Rectangles.ContainsKey(this) &&
                r.Height < b.ParentBox.Rectangles[this].Height)
            {
                // Do this only if rectangle is shorter than parent's
                double recttop = newtop - gap;
                RRect newr = new RRect(r.X, recttop, r.Width, r.Height);
                this.Rectangles[b] = newr;
                b.OffsetRectangle(this, gap);
            }

            foreach (var word in ws)
            {
                if (!word.IsImage)
                    word.Top = newtop;
            }
        }

        /// <summary>
        /// Check if the given word is the last selected word in the line.<br/>
        /// It can either be the last word in the line or the next word has no selection.
        /// </summary>
        /// <param name="word">the word to check</param>
        /// <returns></returns>
        public bool IsLastSelectedWord(CssRect word)
        {
            for (int i = 0; i < this._Words.Count - 1; i++)
            {
                if (this._Words[i] == word)
                {
                    return !this._Words[i + 1].Selected;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the words of the linebox
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string[] ws = new string[this.Words.Count];
            for (int i = 0; i < ws.Length; i++)
            {
                ws[i] = this.Words[i].Text;
            }

            return string.Join(" ", ws);
        }
    }
}