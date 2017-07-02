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
using System.Globalization;
using System.Text.RegularExpressions;
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Parse;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Core.Dom
{
    /// <summary>
    /// Base class for css box to handle the css properties.<br/>
    /// Has field and property for every css property that can be set, the properties add additional parsing like
    /// setting the correct border depending what border value was set (single, two , all four).<br/>
    /// Has additional fields to control the location and size of the box and 'actual' css values for some properties
    /// that require additional calculations and parsing.<br/>
    /// </summary>
    internal abstract class CssBoxProperties
    {
        #region CSS Fields

        private string _BackgroundColor = "transparent";
        private string _BackgroundGradient = "none";
        private string _BackgroundGradientAngle = "90";
        private string _BackgroundImage = "none";
        private string _BackgroundPosition = "0% 0%";
        private string _BackgroundRepeat = "repeat";
        private string _BorderTopWidth = "medium";
        private string _BorderRightWidth = "medium";
        private string _BorderBottomWidth = "medium";
        private string _BorderLeftWidth = "medium";
        private string _BorderTopColor = "black";
        private string _BorderRightColor = "black";
        private string _BorderBottomColor = "black";
        private string _BorderLeftColor = "black";
        private string _BorderTopStyle = "none";
        private string _BorderRightStyle = "none";
        private string _BorderBottomStyle = "none";
        private string _BorderLeftStyle = "none";
        private string _BorderSpacing = "0";
        private string _BorderCollapse = "separate";
        private string Bottom;
        private string _Color = "black";
        private string _Content = "normal";
        private string _CornerNwRadius = "0";
        private string _CornerNeRadius = "0";
        private string _CornerSeRadius = "0";
        private string _CornerSwRadius = "0";
        private string _CornerRadius = "0";
        private string _EmptyCells = "show";
        private string _Direction = "ltr";
        private string _Display = "inline";
        private string _FontFamily;
        private string _FontSize = "medium";
        private string _FontStyle = "normal";
        private string _FontVariant = "normal";
        private string _FontWeight = "normal";
        private string _Float = "none";
        private string _Height = "auto";
        private string _MarginBottom = "0";
        private string _MarginLeft = "0";
        private string _MarginRight = "0";
        private string _MarginTop = "0";
        private string _Left = "auto";
        private string _LineHeight = "normal";
        private string _ListStyleType = "disc";
        private string _ListStyleImage = string.Empty;
        private string _ListStylePosition = "outside";
        private string _ListStyle = string.Empty;
        private string _Overflow = "visible";
        private string _PaddingLeft = "0";
        private string _PaddingBottom = "0";
        private string _PaddingRight = "0";
        private string _PaddingTop = "0";
        private string _PageBreakInside = CssConstants.Auto;
        private string Right;
        private string _TextAlign = string.Empty;
        private string _TextDecoration = string.Empty;
        private string _TextIndent = "0";
        private string _Top = "auto";
        private string _Position = "static";
        private string _VerticalAlign = "baseline";
        private string _Width = "auto";
        private string _MaxWidth = "none";
        private string _WordSpacing = "normal";
        private string _WordBreak = "normal";
        private string _WhiteSpace = "normal";
        private string _Visibility = "visible";

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        private RPoint _Location;

        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        private RSize _Size;

        private double _ActualCornerNw = double.NaN;
        private double _ActualCornerNe = double.NaN;
        private double _ActualCornerSw = double.NaN;
        private double _ActualCornerSe = double.NaN;
        private RColor _ActualColor = RColor.Empty;
        private double _ActualBackgroundGradientAngle = double.NaN;
        private double _ActualHeight = double.NaN;
        private double _ActualWidth = double.NaN;
        private double _ActualPaddingTop = double.NaN;
        private double _ActualPaddingBottom = double.NaN;
        private double _ActualPaddingRight = double.NaN;
        private double _ActualPaddingLeft = double.NaN;
        private double _ActualMarginTop = double.NaN;
        private double _CollapsedMarginTop = double.NaN;
        private double _ActualMarginBottom = double.NaN;
        private double _ActualMarginRight = double.NaN;
        private double _ActualMarginLeft = double.NaN;
        private double _ActualBorderTopWidth = double.NaN;
        private double _ActualBorderLeftWidth = double.NaN;
        private double _ActualBorderBottomWidth = double.NaN;
        private double _ActualBorderRightWidth = double.NaN;

        /// <summary>
        /// the width of whitespace between words
        /// </summary>
        private double _ActualLineHeight = double.NaN;

        private double _ActualWordSpacing = double.NaN;
        private double _ActualTextIndent = double.NaN;
        private double _ActualBorderSpacingHorizontal = double.NaN;
        private double _ActualBorderSpacingVertical = double.NaN;
        private RColor _ActualBackgroundGradient = RColor.Empty;
        private RColor _ActualBorderTopColor = RColor.Empty;
        private RColor _ActualBorderLeftColor = RColor.Empty;
        private RColor _ActualBorderBottomColor = RColor.Empty;
        private RColor _ActualBorderRightColor = RColor.Empty;
        private RColor _ActualBackgroundColor = RColor.Empty;
        private RFont _ActualFont;

        #endregion

        #region CSS Properties

        public string BorderBottomWidth
        {
            get
            {
                return this._BorderBottomWidth;
            }

            set
            {
                this._BorderBottomWidth = value;
                this._ActualBorderBottomWidth = Single.NaN;
            }
        }

        public string BorderLeftWidth
        {
            get
            {
                return this._BorderLeftWidth;
            }

            set
            {
                this._BorderLeftWidth = value;
                this._ActualBorderLeftWidth = Single.NaN;
            }
        }

        public string BorderRightWidth
        {
            get
            {
                return this._BorderRightWidth;
            }

            set
            {
                this._BorderRightWidth = value;
                this._ActualBorderRightWidth = Single.NaN;
            }
        }

        public string BorderTopWidth
        {
            get
            {
                return this._BorderTopWidth;
            }

            set
            {
                this._BorderTopWidth = value;
                this._ActualBorderTopWidth = Single.NaN;
            }
        }

        public string BorderBottomStyle
        {
            get { return this._BorderBottomStyle; }
            set { this._BorderBottomStyle = value; }
        }

        public string BorderLeftStyle
        {
            get { return this._BorderLeftStyle; }
            set { this._BorderLeftStyle = value; }
        }

        public string BorderRightStyle
        {
            get { return this._BorderRightStyle; }
            set { this._BorderRightStyle = value; }
        }

        public string BorderTopStyle
        {
            get { return this._BorderTopStyle; }
            set { this._BorderTopStyle = value; }
        }

        public string BorderBottomColor
        {
            get
            {
                return this._BorderBottomColor;
            }

            set
            {
                this._BorderBottomColor = value;
                this._ActualBorderBottomColor = RColor.Empty;
            }
        }

        public string BorderLeftColor
        {
            get
            {
                return this._BorderLeftColor;
            }

            set
            {
                this._BorderLeftColor = value;
                this._ActualBorderLeftColor = RColor.Empty;
            }
        }

        public string BorderRightColor
        {
            get
            {
                return this._BorderRightColor;
            }

            set
            {
                this._BorderRightColor = value;
                this._ActualBorderRightColor = RColor.Empty;
            }
        }

        public string BorderTopColor
        {
            get
            {
                return this._BorderTopColor;
            }

            set
            {
                this._BorderTopColor = value;
                this._ActualBorderTopColor = RColor.Empty;
            }
        }

        public string BorderSpacing
        {
            get { return this._BorderSpacing; }
            set { this._BorderSpacing = value; }
        }

        public string BorderCollapse
        {
            get { return this._BorderCollapse; }
            set { this._BorderCollapse = value; }
        }

        public string CornerRadius
        {
            get
            {
                return this._CornerRadius;
            }

            set
            {
                MatchCollection r = RegexParserUtils.Match(RegexParserUtils.CssLength, value);
                switch (r.Count)
                {
                    case 1:
                        this.CornerNeRadius = r[0].Value;
                        this.CornerNwRadius = r[0].Value;
                        this.CornerSeRadius = r[0].Value;
                        this.CornerSwRadius = r[0].Value;
                        break;
                    case 2:
                        this.CornerNeRadius = r[0].Value;
                        this.CornerNwRadius = r[0].Value;
                        this.CornerSeRadius = r[1].Value;
                        this.CornerSwRadius = r[1].Value;
                        break;
                    case 3:
                        this.CornerNeRadius = r[0].Value;
                        this.CornerNwRadius = r[1].Value;
                        this.CornerSeRadius = r[2].Value;
                        break;
                    case 4:
                        this.CornerNeRadius = r[0].Value;
                        this.CornerNwRadius = r[1].Value;
                        this.CornerSeRadius = r[2].Value;
                        this.CornerSwRadius = r[3].Value;
                        break;
                }

                this._CornerRadius = value;
            }
        }

        public string CornerNwRadius
        {
            get { return this._CornerNwRadius; }
            set { this._CornerNwRadius = value; }
        }

        public string CornerNeRadius
        {
            get { return this._CornerNeRadius; }
            set { this._CornerNeRadius = value; }
        }

        public string CornerSeRadius
        {
            get { return this._CornerSeRadius; }
            set { this._CornerSeRadius = value; }
        }

        public string CornerSwRadius
        {
            get { return this._CornerSwRadius; }
            set { this._CornerSwRadius = value; }
        }

        public string MarginBottom
        {
            get { return this._MarginBottom; }
            set { this._MarginBottom = value; }
        }

        public string MarginLeft
        {
            get { return this._MarginLeft; }
            set { this._MarginLeft = value; }
        }

        public string MarginRight
        {
            get { return this._MarginRight; }
            set { this._MarginRight = value; }
        }

        public string MarginTop
        {
            get { return this._MarginTop; }
            set { this._MarginTop = value; }
        }

        public string PaddingBottom
        {
            get
            {
                return this._PaddingBottom;
            }

            set
            {
                this._PaddingBottom = value;
                this._ActualPaddingBottom = double.NaN;
            }
        }

        public string PaddingLeft
        {
            get
            {
                return this._PaddingLeft;
            }

            set
            {
                this._PaddingLeft = value;
                this._ActualPaddingLeft = double.NaN;
            }
        }

        public string PaddingRight
        {
            get
            {
                return this._PaddingRight;
            }

            set
            {
                this._PaddingRight = value;
                this._ActualPaddingRight = double.NaN;
            }
        }

        public string PaddingTop
        {
            get
            {
                return this._PaddingTop;
            }

            set
            {
                this._PaddingTop = value;
                this._ActualPaddingTop = double.NaN;
            }
        }

        public string PageBreakInside
        {
            get
            {
                return this._PageBreakInside;
            }

            set
            {
                this._PageBreakInside = value;
            }
        }

        public string Left
        {
            get
            {
                return this._Left;
            }

            set
            {
                this._Left = value;
                if (this.Position == CssConstants.Fixed)
                {
                    this._Location = this.GetActualLocation(this.Left, this.Top);
                }
            }
        }

        public string Top
        {
            get
            {
                return this._Top;
            }

            set
            {
                this._Top = value;
                if (this.Position == CssConstants.Fixed)
                {
                    this._Location = this.GetActualLocation(this.Left, this.Top);
                }
            }
        }

        public string Width
        {
            get { return this._Width; }
            set { this._Width = value; }
        }

        public string MaxWidth
        {
            get { return this._MaxWidth; }
            set { this._MaxWidth = value; }
        }

        public string Height
        {
            get { return this._Height; }
            set { this._Height = value; }
        }

        public string BackgroundColor
        {
            get { return this._BackgroundColor; }
            set { this._BackgroundColor = value; }
        }

        public string BackgroundImage
        {
            get { return this._BackgroundImage; }
            set { this._BackgroundImage = value; }
        }

        public string BackgroundPosition
        {
            get { return this._BackgroundPosition; }
            set { this._BackgroundPosition = value; }
        }

        public string BackgroundRepeat
        {
            get { return this._BackgroundRepeat; }
            set { this._BackgroundRepeat = value; }
        }

        public string BackgroundGradient
        {
            get { return this._BackgroundGradient; }
            set { this._BackgroundGradient = value; }
        }

        public string BackgroundGradientAngle
        {
            get { return this._BackgroundGradientAngle; }
            set { this._BackgroundGradientAngle = value; }
        }

        public string Color
        {
            get
            {
                return this._Color;
            }

            set
            {
                this._Color = value;
                this._ActualColor = RColor.Empty;
            }
        }

        public string Content
        {
            get { return this._Content; }
            set { this._Content = value; }
        }

        public string Display
        {
            get { return this._Display; }
            set { this._Display = value; }
        }

        public string Direction
        {
            get { return this._Direction; }
            set { this._Direction = value; }
        }

        public string EmptyCells
        {
            get { return this._EmptyCells; }
            set { this._EmptyCells = value; }
        }

        public string Float
        {
            get { return this._Float; }
            set { this._Float = value; }
        }

        public string Position
        {
            get { return this._Position; }
            set { this._Position = value; }
        }

        public string LineHeight
        {
            get { return this._LineHeight; }
            set { this._LineHeight = string.Format(NumberFormatInfo.InvariantInfo, "{0}px", CssValueParser.ParseLength(value, this.Size.Height, this, CssConstants.Em)); }
        }

        public string VerticalAlign
        {
            get { return this._VerticalAlign; }
            set { this._VerticalAlign = value; }
        }

        public string TextIndent
        {
            get { return this._TextIndent; }
            set { this._TextIndent = this.NoEms(value); }
        }

        public string TextAlign
        {
            get { return this._TextAlign; }
            set { this._TextAlign = value; }
        }

        public string TextDecoration
        {
            get { return this._TextDecoration; }
            set { this._TextDecoration = value; }
        }

        public string WhiteSpace
        {
            get { return this._WhiteSpace; }
            set { this._WhiteSpace = value; }
        }

        public string Visibility
        {
            get { return this._Visibility; }
            set { this._Visibility = value; }
        }

        public string WordSpacing
        {
            get { return this._WordSpacing; }
            set { this._WordSpacing = this.NoEms(value); }
        }

        public string WordBreak
        {
            get { return this._WordBreak; }
            set { this._WordBreak = value; }
        }

        public string FontFamily
        {
            get { return this._FontFamily; }
            set { this._FontFamily = value; }
        }

        public string FontSize
        {
            get
            {
                return this._FontSize;
            }

            set
            {
                string length = RegexParserUtils.Search(RegexParserUtils.CssLength, value);
                if (length != null)
                {
                    string computedValue;
                    CssLength len = new CssLength(length);

                    if (len.HasError)
                    {
                        computedValue = "medium";
                    }
                    else if (len.Unit == CssUnit.Ems && this.GetParent() != null)
                    {
                        computedValue = len.ConvertEmToPoints(this.GetParent().ActualFont.Size).ToString();
                    }
                    else
                    {
                        computedValue = len.ToString();
                    }

                    this._FontSize = computedValue;
                }
                else
                {
                    this._FontSize = value;
                }
            }
        }

        public string FontStyle
        {
            get { return this._FontStyle; }
            set { this._FontStyle = value; }
        }

        public string FontVariant
        {
            get { return this._FontVariant; }
            set { this._FontVariant = value; }
        }

        public string FontWeight
        {
            get { return this._FontWeight; }
            set { this._FontWeight = value; }
        }

        public string ListStyle
        {
            get { return this._ListStyle; }
            set { this._ListStyle = value; }
        }

        public string Overflow
        {
            get { return this._Overflow; }
            set { this._Overflow = value; }
        }

        public string ListStylePosition
        {
            get { return this._ListStylePosition; }
            set { this._ListStylePosition = value; }
        }

        public string ListStyleImage
        {
            get { return this._ListStyleImage; }
            set { this._ListStyleImage = value; }
        }

        public string ListStyleType
        {
            get { return this._ListStyleType; }
            set { this._ListStyleType = value; }
        }

        #endregion CSS Propertier

        /// <summary>
        /// Gets or sets the location of the box
        /// </summary>
        public RPoint Location
        {
            get
            {
                if (this._Location.IsEmpty && this.Position == CssConstants.Fixed)
                {
                    var left = this.Left;
                    var top = this.Top;

                    this._Location = this.GetActualLocation(this.Left, this.Top);
                }

                return this._Location;
            }

            set
            {
                this._Location = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the box
        /// </summary>
        public RSize Size
        {
            get { return this._Size; }
            set { this._Size = value; }
        }

        /// <summary>
        /// Gets the bounds of the box
        /// </summary>
        public RRect Bounds
        {
            get { return new RRect(this.Location, this.Size); }
        }

        /// <summary>
        /// Gets the width available on the box, counting padding and margin.
        /// </summary>
        public double AvailableWidth
        {
            get { return this.Size.Width - this.ActualBorderLeftWidth - this.ActualPaddingLeft - this.ActualPaddingRight - this.ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the right of the box. When setting, it will affect only the width of the box.
        /// </summary>
        public double ActualRight
        {
            get { return this.Location.X + this.Size.Width; }
            set { this.Size = new RSize(value - this.Location.X, this.Size.Height); }
        }

        /// <summary>
        /// Gets or sets the bottom of the box.
        /// (When setting, alters only the Size.Height of the box)
        /// </summary>
        public double ActualBottom
        {
            get { return this.Location.Y + this.Size.Height; }
            set { this.Size = new RSize(this.Size.Width, value - this.Location.Y); }
        }

        /// <summary>
        /// Gets the left of the client rectangle (Where content starts rendering)
        /// </summary>
        public double ClientLeft
        {
            get { return this.Location.X + this.ActualBorderLeftWidth + this.ActualPaddingLeft; }
        }

        /// <summary>
        /// Gets the top of the client rectangle (Where content starts rendering)
        /// </summary>
        public double ClientTop
        {
            get { return this.Location.Y + this.ActualBorderTopWidth + this.ActualPaddingTop; }
        }

        /// <summary>
        /// Gets the right of the client rectangle
        /// </summary>
        public double ClientRight
        {
            get { return this.ActualRight - this.ActualPaddingRight - this.ActualBorderRightWidth; }
        }

        /// <summary>
        /// Gets the bottom of the client rectangle
        /// </summary>
        public double ClientBottom
        {
            get { return this.ActualBottom - this.ActualPaddingBottom - this.ActualBorderBottomWidth; }
        }

        /// <summary>
        /// Gets the client rectangle
        /// </summary>
        public RRect ClientRectangle
        {
            get { return RRect.FromLTRB(this.ClientLeft, this.ClientTop, this.ClientRight, this.ClientBottom); }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public double ActualHeight
        {
            get
            {
                if (double.IsNaN(this._ActualHeight))
                {
                    this._ActualHeight = CssValueParser.ParseLength(this.Height, this.Size.Height, this);
                }

                return this._ActualHeight;
            }
        }

        /// <summary>
        /// Gets the actual height
        /// </summary>
        public double ActualWidth
        {
            get
            {
                if (double.IsNaN(this._ActualWidth))
                {
                    this._ActualWidth = CssValueParser.ParseLength(this.Width, this.Size.Width, this);
                }

                return this._ActualWidth;
            }
        }

        /// <summary>
        /// Gets the actual top's padding
        /// </summary>
        public double ActualPaddingTop
        {
            get
            {
                if (double.IsNaN(this._ActualPaddingTop))
                {
                    this._ActualPaddingTop = CssValueParser.ParseLength(this.PaddingTop, this.Size.Width, this);
                }

                return this._ActualPaddingTop;
            }
        }

        /// <summary>
        /// Gets the actual padding on the left
        /// </summary>
        public double ActualPaddingLeft
        {
            get
            {
                if (double.IsNaN(this._ActualPaddingLeft))
                {
                    this._ActualPaddingLeft = CssValueParser.ParseLength(this.PaddingLeft, this.Size.Width, this);
                }

                return this._ActualPaddingLeft;
            }
        }

        /// <summary>
        /// Gets the actual Padding of the bottom
        /// </summary>
        public double ActualPaddingBottom
        {
            get
            {
                if (double.IsNaN(this._ActualPaddingBottom))
                {
                    this._ActualPaddingBottom = CssValueParser.ParseLength(this.PaddingBottom, this.Size.Width, this);
                }

                return this._ActualPaddingBottom;
            }
        }

        /// <summary>
        /// Gets the actual padding on the right
        /// </summary>
        public double ActualPaddingRight
        {
            get
            {
                if (double.IsNaN(this._ActualPaddingRight))
                {
                    this._ActualPaddingRight = CssValueParser.ParseLength(this.PaddingRight, this.Size.Width, this);
                }

                return this._ActualPaddingRight;
            }
        }

        /// <summary>
        /// Gets the actual top's Margin
        /// </summary>
        public double ActualMarginTop
        {
            get
            {
                if (double.IsNaN(this._ActualMarginTop))
                {
                    if (this.MarginTop == CssConstants.Auto)
                        this.MarginTop = "0";
                    var actualMarginTop = CssValueParser.ParseLength(this.MarginTop, this.Size.Width, this);
                    if (this.MarginLeft.EndsWith("%"))
                        return actualMarginTop;
                    this._ActualMarginTop = actualMarginTop;
                }

                return this._ActualMarginTop;
            }
        }

        /// <summary>
        /// The margin top value if was effected by margin collapse.
        /// </summary>
        public double CollapsedMarginTop
        {
            get { return double.IsNaN(this._CollapsedMarginTop) ? 0 : this._CollapsedMarginTop; }
            set { this._CollapsedMarginTop = value; }
        }

        /// <summary>
        /// Gets the actual Margin on the left
        /// </summary>
        public double ActualMarginLeft
        {
            get
            {
                if (double.IsNaN(this._ActualMarginLeft))
                {
                    if (this.MarginLeft == CssConstants.Auto)
                        this.MarginLeft = "0";
                    var actualMarginLeft = CssValueParser.ParseLength(this.MarginLeft, this.Size.Width, this);
                    if (this.MarginLeft.EndsWith("%"))
                        return actualMarginLeft;
                    this._ActualMarginLeft = actualMarginLeft;
                }

                return this._ActualMarginLeft;
            }
        }

        /// <summary>
        /// Gets the actual Margin of the bottom
        /// </summary>
        public double ActualMarginBottom
        {
            get
            {
                if (double.IsNaN(this._ActualMarginBottom))
                {
                    if (this.MarginBottom == CssConstants.Auto)
                        this.MarginBottom = "0";
                    var actualMarginBottom = CssValueParser.ParseLength(this.MarginBottom, this.Size.Width, this);
                    if (this.MarginLeft.EndsWith("%"))
                        return actualMarginBottom;
                    this._ActualMarginBottom = actualMarginBottom;
                }

                return this._ActualMarginBottom;
            }
        }

        /// <summary>
        /// Gets the actual Margin on the right
        /// </summary>
        public double ActualMarginRight
        {
            get
            {
                if (double.IsNaN(this._ActualMarginRight))
                {
                    if (this.MarginRight == CssConstants.Auto)
                        this.MarginRight = "0";
                    var actualMarginRight = CssValueParser.ParseLength(this.MarginRight, this.Size.Width, this);
                    if (this.MarginLeft.EndsWith("%"))
                        return actualMarginRight;
                    this._ActualMarginRight = actualMarginRight;
                }

                return this._ActualMarginRight;
            }
        }

        /// <summary>
        /// Gets the actual top border width
        /// </summary>
        public double ActualBorderTopWidth
        {
            get
            {
                if (double.IsNaN(this._ActualBorderTopWidth))
                {
                    this._ActualBorderTopWidth = CssValueParser.GetActualBorderWidth(this.BorderTopWidth, this);
                    if (string.IsNullOrEmpty(this.BorderTopStyle) || this.BorderTopStyle == CssConstants.None)
                    {
                        this._ActualBorderTopWidth = 0f;
                    }
                }

                return this._ActualBorderTopWidth;
            }
        }

        /// <summary>
        /// Gets the actual Left border width
        /// </summary>
        public double ActualBorderLeftWidth
        {
            get
            {
                if (double.IsNaN(this._ActualBorderLeftWidth))
                {
                    this._ActualBorderLeftWidth = CssValueParser.GetActualBorderWidth(this.BorderLeftWidth, this);
                    if (string.IsNullOrEmpty(this.BorderLeftStyle) || this.BorderLeftStyle == CssConstants.None)
                    {
                        this._ActualBorderLeftWidth = 0f;
                    }
                }

                return this._ActualBorderLeftWidth;
            }
        }

        /// <summary>
        /// Gets the actual Bottom border width
        /// </summary>
        public double ActualBorderBottomWidth
        {
            get
            {
                if (double.IsNaN(this._ActualBorderBottomWidth))
                {
                    this._ActualBorderBottomWidth = CssValueParser.GetActualBorderWidth(this.BorderBottomWidth, this);
                    if (string.IsNullOrEmpty(this.BorderBottomStyle) || this.BorderBottomStyle == CssConstants.None)
                    {
                        this._ActualBorderBottomWidth = 0f;
                    }
                }

                return this._ActualBorderBottomWidth;
            }
        }

        /// <summary>
        /// Gets the actual Right border width
        /// </summary>
        public double ActualBorderRightWidth
        {
            get
            {
                if (double.IsNaN(this._ActualBorderRightWidth))
                {
                    this._ActualBorderRightWidth = CssValueParser.GetActualBorderWidth(this.BorderRightWidth, this);
                    if (string.IsNullOrEmpty(this.BorderRightStyle) || this.BorderRightStyle == CssConstants.None)
                    {
                        this._ActualBorderRightWidth = 0f;
                    }
                }

                return this._ActualBorderRightWidth;
            }
        }

        /// <summary>
        /// Gets the actual top border Color
        /// </summary>
        public RColor ActualBorderTopColor
        {
            get
            {
                if (this._ActualBorderTopColor.IsEmpty)
                {
                    this._ActualBorderTopColor = this.GetActualColor(this.BorderTopColor);
                }

                return this._ActualBorderTopColor;
            }
        }

        protected abstract RPoint GetActualLocation(string x, string y);

        protected abstract RColor GetActualColor(string colorStr);

        /// <summary>
        /// Gets the actual Left border Color
        /// </summary>
        public RColor ActualBorderLeftColor
        {
            get
            {
                if (this._ActualBorderLeftColor.IsEmpty)
                {
                    this._ActualBorderLeftColor = this.GetActualColor(this.BorderLeftColor);
                }

                return this._ActualBorderLeftColor;
            }
        }

        /// <summary>
        /// Gets the actual Bottom border Color
        /// </summary>
        public RColor ActualBorderBottomColor
        {
            get
            {
                if (this._ActualBorderBottomColor.IsEmpty)
                {
                    this._ActualBorderBottomColor = this.GetActualColor(this.BorderBottomColor);
                }

                return this._ActualBorderBottomColor;
            }
        }

        /// <summary>
        /// Gets the actual Right border Color
        /// </summary>
        public RColor ActualBorderRightColor
        {
            get
            {
                if (this._ActualBorderRightColor.IsEmpty)
                {
                    this._ActualBorderRightColor = this.GetActualColor(this.BorderRightColor);
                }

                return this._ActualBorderRightColor;
            }
        }

        /// <summary>
        /// Gets the actual length of the north west corner
        /// </summary>
        public double ActualCornerNw
        {
            get
            {
                if (double.IsNaN(this._ActualCornerNw))
                {
                    this._ActualCornerNw = CssValueParser.ParseLength(this.CornerNwRadius, 0, this);
                }

                return this._ActualCornerNw;
            }
        }

        /// <summary>
        /// Gets the actual length of the north east corner
        /// </summary>
        public double ActualCornerNe
        {
            get
            {
                if (double.IsNaN(this._ActualCornerNe))
                {
                    this._ActualCornerNe = CssValueParser.ParseLength(this.CornerNeRadius, 0, this);
                }

                return this._ActualCornerNe;
            }
        }

        /// <summary>
        /// Gets the actual length of the south east corner
        /// </summary>
        public double ActualCornerSe
        {
            get
            {
                if (double.IsNaN(this._ActualCornerSe))
                {
                    this._ActualCornerSe = CssValueParser.ParseLength(this.CornerSeRadius, 0, this);
                }

                return this._ActualCornerSe;
            }
        }

        /// <summary>
        /// Gets the actual length of the south west corner
        /// </summary>
        public double ActualCornerSw
        {
            get
            {
                if (double.IsNaN(this._ActualCornerSw))
                {
                    this._ActualCornerSw = CssValueParser.ParseLength(this.CornerSwRadius, 0, this);
                }

                return this._ActualCornerSw;
            }
        }

        /// <summary>
        /// Gets a value indicating if at least one of the corners of the box is rounded
        /// </summary>
        public bool IsRounded
        {
            get { return this.ActualCornerNe > 0f || this.ActualCornerNw > 0f || this.ActualCornerSe > 0f || this.ActualCornerSw > 0f; }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public double ActualWordSpacing
        {
            get { return this._ActualWordSpacing; }
        }

        /// <summary>
        ///
        /// Gets the actual color for the text.
        /// </summary>
        public RColor ActualColor
        {
            get
            {
                if (this._ActualColor.IsEmpty)
                {
                    this._ActualColor = this.GetActualColor(this.Color);
                }

                return this._ActualColor;
            }
        }

        /// <summary>
        /// Gets the actual background color of the box
        /// </summary>
        public RColor ActualBackgroundColor
        {
            get
            {
                if (this._ActualBackgroundColor.IsEmpty)
                {
                    this._ActualBackgroundColor = this.GetActualColor(this.BackgroundColor);
                }

                return this._ActualBackgroundColor;
            }
        }

        /// <summary>
        /// Gets the second color that creates a gradient for the background
        /// </summary>
        public RColor ActualBackgroundGradient
        {
            get
            {
                if (this._ActualBackgroundGradient.IsEmpty)
                {
                    this._ActualBackgroundGradient = this.GetActualColor(this.BackgroundGradient);
                }

                return this._ActualBackgroundGradient;
            }
        }

        /// <summary>
        /// Gets the actual angle specified for the background gradient
        /// </summary>
        public double ActualBackgroundGradientAngle
        {
            get
            {
                if (double.IsNaN(this._ActualBackgroundGradientAngle))
                {
                    this._ActualBackgroundGradientAngle = CssValueParser.ParseNumber(this.BackgroundGradientAngle, 360f);
                }

                return this._ActualBackgroundGradientAngle;
            }
        }

        /// <summary>
        /// Gets the actual font of the parent
        /// </summary>
        public RFont ActualParentFont
        {
            get { return this.GetParent() == null ? this.ActualFont : this.GetParent().ActualFont; }
        }

        /// <summary>
        /// Gets the font that should be actually used to paint the text of the box
        /// </summary>
        public RFont ActualFont
        {
            get
            {
                if (this._ActualFont == null)
                {
                    if (string.IsNullOrEmpty(this.FontFamily))
                    {
                        this.FontFamily = CssConstants.DefaultFont;
                    }

                    if (string.IsNullOrEmpty(this.FontSize))
                    {
                        this.FontSize = CssConstants.FontSize.ToString(CultureInfo.InvariantCulture) + "pt";
                    }

                    RFontStyle st = RFontStyle.Regular;

                    if (this.FontStyle == CssConstants.Italic || this.FontStyle == CssConstants.Oblique)
                    {
                        st |= RFontStyle.Italic;
                    }

                    if (this.FontWeight != CssConstants.Normal && this.FontWeight != CssConstants.Lighter && !string.IsNullOrEmpty(this.FontWeight) && this.FontWeight != CssConstants.Inherit)
                    {
                        st |= RFontStyle.Bold;
                    }

                    double fsize;
                    double parentSize = CssConstants.FontSize;

                    if (this.GetParent() != null)
                        parentSize = this.GetParent().ActualFont.Size;

                    switch (this.FontSize)
                    {
                        case CssConstants.Medium:
                            fsize = CssConstants.FontSize;
                            break;
                        case CssConstants.XXSmall:
                            fsize = CssConstants.FontSize - 4;
                            break;
                        case CssConstants.XSmall:
                            fsize = CssConstants.FontSize - 3;
                            break;
                        case CssConstants.Small:
                            fsize = CssConstants.FontSize - 2;
                            break;
                        case CssConstants.Large:
                            fsize = CssConstants.FontSize + 2;
                            break;
                        case CssConstants.XLarge:
                            fsize = CssConstants.FontSize + 3;
                            break;
                        case CssConstants.XXLarge:
                            fsize = CssConstants.FontSize + 4;
                            break;
                        case CssConstants.Smaller:
                            fsize = parentSize - 2;
                            break;
                        case CssConstants.Larger:
                            fsize = parentSize + 2;
                            break;
                        default:
                            fsize = CssValueParser.ParseLength(this.FontSize, parentSize, parentSize, null, true, true);
                            break;
                    }

                    if (fsize <= 1f)
                    {
                        fsize = CssConstants.FontSize;
                    }

                    this._ActualFont = this.GetCachedFont(this.FontFamily, fsize, st);
                }

                return this._ActualFont;
            }
        }

        protected abstract RFont GetCachedFont(string fontFamily, double fsize, RFontStyle st);

        /// <summary>
        /// Gets the line height
        /// </summary>
        public double ActualLineHeight
        {
            get
            {
                if (double.IsNaN(this._ActualLineHeight))
                {
                    this._ActualLineHeight = .9f * CssValueParser.ParseLength(this.LineHeight, this.Size.Height, this);
                }

                return this._ActualLineHeight;
            }
        }

        /// <summary>
        /// Gets the text indentation (on first line only)
        /// </summary>
        public double ActualTextIndent
        {
            get
            {
                if (double.IsNaN(this._ActualTextIndent))
                {
                    this._ActualTextIndent = CssValueParser.ParseLength(this.TextIndent, this.Size.Width, this);
                }

                return this._ActualTextIndent;
            }
        }

        /// <summary>
        /// Gets the actual horizontal border spacing for tables
        /// </summary>
        public double ActualBorderSpacingHorizontal
        {
            get
            {
                if (double.IsNaN(this._ActualBorderSpacingHorizontal))
                {
                    MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, this.BorderSpacing);

                    if (matches.Count == 0)
                    {
                        this._ActualBorderSpacingHorizontal = 0;
                    }
                    else if (matches.Count > 0)
                    {
                        this._ActualBorderSpacingHorizontal = CssValueParser.ParseLength(matches[0].Value, 1, this);
                    }
                }

                return this._ActualBorderSpacingHorizontal;
            }
        }

        /// <summary>
        /// Gets the actual vertical border spacing for tables
        /// </summary>
        public double ActualBorderSpacingVertical
        {
            get
            {
                if (double.IsNaN(this._ActualBorderSpacingVertical))
                {
                    MatchCollection matches = RegexParserUtils.Match(RegexParserUtils.CssLength, this.BorderSpacing);

                    if (matches.Count == 0)
                    {
                        this._ActualBorderSpacingVertical = 0;
                    }
                    else if (matches.Count == 1)
                    {
                        this._ActualBorderSpacingVertical = CssValueParser.ParseLength(matches[0].Value, 1, this);
                    }
                    else
                    {
                        this._ActualBorderSpacingVertical = CssValueParser.ParseLength(matches[1].Value, 1, this);
                    }
                }

                return this._ActualBorderSpacingVertical;
            }
        }

        /// <summary>
        /// Get the parent of this css properties instance.
        /// </summary>
        /// <returns></returns>
        protected abstract CssBoxProperties GetParent();

        /// <summary>
        /// Gets the height of the font in the specified units
        /// </summary>
        /// <returns></returns>
        public double GetEmHeight()
        {
            return this.ActualFont.Height;
        }

        /// <summary>
        /// Ensures that the specified length is converted to pixels if necessary
        /// </summary>
        /// <param name="length"></param>
        protected string NoEms(string length)
        {
            var len = new CssLength(length);
            if (len.Unit == CssUnit.Ems)
            {
                length = len.ConvertEmToPixels(this.GetEmHeight()).ToString();
            }

            return length;
        }

        /// <summary>
        /// Set the style/width/color for all 4 borders on the box.<br/>
        /// if null is given for a value it will not be set.
        /// </summary>
        /// <param name="style">optional: the style to set</param>
        /// <param name="width">optional: the width to set</param>
        /// <param name="color">optional: the color to set</param>
        protected void SetAllBorders(string style = null, string width = null, string color = null)
        {
            if (style != null)
                this.BorderLeftStyle = this.BorderTopStyle = this.BorderRightStyle = this.BorderBottomStyle = style;
            if (width != null)
                this.BorderLeftWidth = this.BorderTopWidth = this.BorderRightWidth = this.BorderBottomWidth = width;
            if (color != null)
                this.BorderLeftColor = this.BorderTopColor = this.BorderRightColor = this.BorderBottomColor = color;
        }

        /// <summary>
        /// Measures the width of whitespace between words (set <see cref="ActualWordSpacing"/>).
        /// </summary>
        protected void MeasureWordSpacing(RGraphics g)
        {
            if (double.IsNaN(this.ActualWordSpacing))
            {
                this._ActualWordSpacing = CssUtils.WhiteSpace(g, this);
                if (this.WordSpacing != CssConstants.Normal)
                {
                    string len = RegexParserUtils.Search(RegexParserUtils.CssLength, this.WordSpacing);
                    this._ActualWordSpacing += CssValueParser.ParseLength(len, 1, this);
                }
            }
        }

        /// <summary>
        /// Inherits inheritable values from specified box.
        /// </summary>
        /// <param name="everything">Set to true to inherit all CSS properties instead of only the ineritables</param>
        /// <param name="p">Box to inherit the properties</param>
        protected void InheritStyle(CssBox p, bool everything)
        {
            if (p != null)
            {
                this._BorderSpacing = p._BorderSpacing;
                this._BorderCollapse = p._BorderCollapse;
                this._Color = p._Color;
                this._EmptyCells = p._EmptyCells;
                this._WhiteSpace = p._WhiteSpace;
                this._Visibility = p._Visibility;
                this._TextIndent = p._TextIndent;
                this._TextAlign = p._TextAlign;
                this._VerticalAlign = p._VerticalAlign;
                this._FontFamily = p._FontFamily;
                this._FontSize = p._FontSize;
                this._FontStyle = p._FontStyle;
                this._FontVariant = p._FontVariant;
                this._FontWeight = p._FontWeight;
                this._ListStyleImage = p._ListStyleImage;
                this._ListStylePosition = p._ListStylePosition;
                this._ListStyleType = p._ListStyleType;
                this._ListStyle = p._ListStyle;
                this._LineHeight = p._LineHeight;
                this._WordBreak = p.WordBreak;
                this._Direction = p._Direction;

                if (everything)
                {
                    this._BackgroundColor = p._BackgroundColor;
                    this._BackgroundGradient = p._BackgroundGradient;
                    this._BackgroundGradientAngle = p._BackgroundGradientAngle;
                    this._BackgroundImage = p._BackgroundImage;
                    this._BackgroundPosition = p._BackgroundPosition;
                    this._BackgroundRepeat = p._BackgroundRepeat;
                    this._BorderTopWidth = p._BorderTopWidth;
                    this._BorderRightWidth = p._BorderRightWidth;
                    this._BorderBottomWidth = p._BorderBottomWidth;
                    this._BorderLeftWidth = p._BorderLeftWidth;
                    this._BorderTopColor = p._BorderTopColor;
                    this._BorderRightColor = p._BorderRightColor;
                    this._BorderBottomColor = p._BorderBottomColor;
                    this._BorderLeftColor = p._BorderLeftColor;
                    this._BorderTopStyle = p._BorderTopStyle;
                    this._BorderRightStyle = p._BorderRightStyle;
                    this._BorderBottomStyle = p._BorderBottomStyle;
                    this._BorderLeftStyle = p._BorderLeftStyle;
                    this.Bottom = p.Bottom;
                    this._CornerNwRadius = p._CornerNwRadius;
                    this._CornerNeRadius = p._CornerNeRadius;
                    this._CornerSeRadius = p._CornerSeRadius;
                    this._CornerSwRadius = p._CornerSwRadius;
                    this._CornerRadius = p._CornerRadius;
                    this._Display = p._Display;
                    this._Float = p._Float;
                    this._Height = p._Height;
                    this._MarginBottom = p._MarginBottom;
                    this._MarginLeft = p._MarginLeft;
                    this._MarginRight = p._MarginRight;
                    this._MarginTop = p._MarginTop;
                    this._Left = p._Left;
                    this._LineHeight = p._LineHeight;
                    this._Overflow = p._Overflow;
                    this._PaddingLeft = p._PaddingLeft;
                    this._PaddingBottom = p._PaddingBottom;
                    this._PaddingRight = p._PaddingRight;
                    this._PaddingTop = p._PaddingTop;
                    this.Right = p.Right;
                    this._TextDecoration = p._TextDecoration;
                    this._Top = p._Top;
                    this._Position = p._Position;
                    this._Width = p._Width;
                    this._MaxWidth = p._MaxWidth;
                    this._WordSpacing = p._WordSpacing;
                }
            }
        }
    }
}