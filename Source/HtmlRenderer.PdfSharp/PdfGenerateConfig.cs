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

using PdfSharp;
using PdfSharp.Drawing;

namespace Scientia.HtmlRenderer.PdfSharp
{
    /// <summary>
    /// The settings for generating PDF using <see cref="PdfGenerator"/>
    /// </summary>
    public sealed class PdfGenerateConfig
    {
        #region Fields/Consts

        /// <summary>
        /// the page size to use for each page in the generated pdf
        /// </summary>
        private PageSize _PageSize;

        /// <summary>
        /// if the page size is undefined this allow you to set manually the page size
        /// </summary>
        private XSize Xsize;

        /// <summary>
        /// the orientation of each page of the generated pdf
        /// </summary>
        private PageOrientation _PageOrientation;

        /// <summary>
        /// the top margin between the page start and the text
        /// </summary>
        private int _MarginTop;

        /// <summary>
        /// the bottom margin between the page end and the text
        /// </summary>
        private int _MarginBottom;

        /// <summary>
        /// the left margin between the page start and the text
        /// </summary>
        private int _MarginLeft;

        /// <summary>
        /// the right margin between the page end and the text
        /// </summary>
        private int _MarginRight;

        #endregion

        /// <summary>
        /// the page size to use for each page in the generated pdf
        /// </summary>
        public PageSize PageSize
        {
            get { return this._PageSize; }
            set { this._PageSize = value; }
        }

        /// <summary>
        /// if the page size is undefined this allow you to set manually the page size
        /// </summary>
        public XSize ManualPageSize
        {
            get { return this.Xsize; }
            set { this.Xsize = value; }
        }

        /// <summary>
        /// the orientation of each page of the generated pdf
        /// </summary>
        public PageOrientation PageOrientation
        {
            get { return this._PageOrientation; }
            set { this._PageOrientation = value; }
        }

        /// <summary>
        /// the top margin between the page start and the text
        /// </summary>
        public int MarginTop
        {
            get
            {
                return this._MarginTop;
            }

            set
            {
                if (value > -1)
                {
                    this._MarginTop = value;
                }
            }
        }

        /// <summary>
        /// the bottom margin between the page end and the text
        /// </summary>
        public int MarginBottom
        {
            get
            {
                return this._MarginBottom;
            }

            set
            {
                if (value > -1)
                {
                    this._MarginBottom = value;
                }
            }
        }

        /// <summary>
        /// the left margin between the page start and the text
        /// </summary>
        public int MarginLeft
        {
            get
            {
                return this._MarginLeft;
            }

            set
            {
                if (value > -1)
                {
                    this._MarginLeft = value;
                }
            }
        }

        /// <summary>
        /// the right margin between the page end and the text
        /// </summary>
        public int MarginRight
        {
            get
            {
                return this._MarginRight;
            }

            set
            {
                if (value > -1)
                {
                    this._MarginRight = value;
                }
            }
        }

        /// <summary>
        /// Set all 4 margins to the given value.
        /// </summary>
        /// <param name="value"></param>
        public void SetMargins(int value)
        {
            if (value > -1)
            {
                this._MarginBottom = this._MarginLeft = this._MarginTop = this._MarginRight = value;
            }
        }

        // The international definitions are:
        //   1 inch == 25.4 mm
        //   1 inch == 72 point

        /// <summary>
        /// Convert the units passed in milimiters to the units used in PdfSharp
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static XSize MilimitersToUnits(double width, double height)
        {
            return new XSize(width / 25.4 * 72, height / 25.4 * 72);
        }

        /// <summary>
        /// Convert the units passed in inches to the units used in PdfSharp
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static XSize InchesToUnits(double width, double height)
        {
            return new XSize(width * 72, height * 72);
        }
    }
}