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
using System.Drawing;
using System.Drawing.Drawing2D;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Utils;
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.WinForms.Utilities;

namespace Scientia.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms Graphics for core.
    /// </summary>
    internal sealed class GraphicsAdapter : RGraphics
    {
        #region Fields and Consts

        /// <summary>
        /// used for <see cref="MeasureString(string,RFont,double,out int,out double)"/> calculation.
        /// </summary>
        private static readonly int[] CharFit = new int[1];

        /// <summary>
        /// used for <see cref="MeasureString(string,RFont,double,out int,out double)"/> calculation.
        /// </summary>
        private static readonly int[] CharFitWidth = new int[1000];

        /// <summary>
        /// Used for GDI+ measure string.
        /// </summary>
        private static readonly CharacterRange[] CharacterRanges = new CharacterRange[1];

        /// <summary>
        /// The string format to use for measuring strings for GDI+ text rendering
        /// </summary>
        private static readonly StringFormat StringFormat;

        /// <summary>
        /// The string format to use for rendering strings for GDI+ text rendering
        /// </summary>
        private static readonly StringFormat StringFormat2;

        /// <summary>
        /// The wrapped WinForms graphics object
        /// </summary>
        private readonly Graphics Graphics;

        /// <summary>
        /// Use GDI+ text rendering to measure/draw text.
        /// </summary>
        private readonly bool UseGdiPlusTextRendering;

#if !MONO
        /// <summary>
        /// the initialized HDC used
        /// </summary>
        private IntPtr Hdc;
#endif

        /// <summary>
        /// if to release the graphics object on dispose
        /// </summary>
        private readonly bool ReleaseGraphics;

        /// <summary>
        /// If text alignment was set to RTL
        /// </summary>
        private bool SetRtl;

        #endregion

        /// <summary>
        /// Init static resources.
        /// </summary>
        static GraphicsAdapter()
        {
            StringFormat = new StringFormat(StringFormat.GenericTypographic);
            StringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.MeasureTrailingSpaces;

            StringFormat2 = new StringFormat(StringFormat.GenericTypographic);
        }

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="g">the win forms graphics object to use</param>
        /// <param name="useGdiPlusTextRendering">Use GDI+ text rendering to measure/draw text</param>
        /// <param name="releaseGraphics">optional: if to release the graphics object on dispose (default - false)</param>
        public GraphicsAdapter(Graphics g, bool useGdiPlusTextRendering, bool releaseGraphics = false)
            : base(WinFormsAdapter.Instance, Utils.Convert(g.ClipBounds))
        {
            ArgChecker.AssertArgNotNull(g, "g");

            this.Graphics = g;
            this.ReleaseGraphics = releaseGraphics;

#if MONO
            _useGdiPlusTextRendering = true;
#else
            this.UseGdiPlusTextRendering = useGdiPlusTextRendering;
#endif
        }

        public override void PopClip()
        {
            this.ReleaseHdc();
            this.ClipStack.Pop();
            this.Graphics.SetClip(Utils.Convert(this.ClipStack.Peek()), CombineMode.Replace);
        }

        public override void PushClip(RRect rect)
        {
            this.ReleaseHdc();
            this.ClipStack.Push(rect);
            this.Graphics.SetClip(Utils.Convert(rect), CombineMode.Replace);
        }

        public override void PushClipExclude(RRect rect)
        {
            this.ReleaseHdc();
            this.ClipStack.Push(this.ClipStack.Peek());
            this.Graphics.SetClip(Utils.Convert(rect), CombineMode.Exclude);
        }

        public override Object SetAntiAliasSmoothingMode()
        {
            this.ReleaseHdc();
            var prevMode = this.Graphics.SmoothingMode;
            this.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            return prevMode;
        }

        public override void ReturnPreviousSmoothingMode(Object prevMode)
        {
            if (prevMode != null)
            {
                this.ReleaseHdc();
                this.Graphics.SmoothingMode = (SmoothingMode)prevMode;
            }
        }

        public override RSize MeasureString(string str, RFont font)
        {
            if (this.UseGdiPlusTextRendering)
            {
                this.ReleaseHdc();
                var fontAdapter = (FontAdapter)font;
                var realFont = fontAdapter.Font;
                CharacterRanges[0] = new CharacterRange(0, str.Length);
                StringFormat.SetMeasurableCharacterRanges(CharacterRanges);
                var size = this.Graphics.MeasureCharacterRanges(str, realFont, RectangleF.Empty, StringFormat)[0].GetBounds(this.Graphics).Size;

                if (font.Height < 0)
                {
                    var height = realFont.Height;
                    var descent = realFont.Size * realFont.FontFamily.GetCellDescent(realFont.Style) / realFont.FontFamily.GetEmHeight(realFont.Style);
#if !MONO
                    fontAdapter.SetMetrics(height, (int)Math.Round(height - descent + .5f));
#else
                    fontAdapter.SetMetrics(height, (int)Math.Round((height - descent + 1f)));
#endif

                }

                return Utils.Convert(size);
            }
            else
            {
#if !MONO
                this.SetFont(font);
                var size = new Size();
                Win32Utils.GetTextExtentPoint32(this.Hdc, str, str.Length, ref size);

                if (font.Height < 0)
                {
                    TextMetric lptm;
                    Win32Utils.GetTextMetrics(this.Hdc, out lptm);
                    ((FontAdapter)font).SetMetrics(size.Height, lptm.Height - lptm.Descent + lptm.Underlined + 1);
                }

                return Utils.Convert(size);
#else
                throw new InvalidProgramException("Invalid Mono code");
#endif
            }
        }

        public override void MeasureString(string str, RFont font, double maxWidth, out int charFit, out double charFitWidth)
        {
            charFit = 0;
            charFitWidth = 0;
            if (this.UseGdiPlusTextRendering)
            {
                this.ReleaseHdc();

                var size = this.MeasureString(str, font);

                for (int i = 1; i <= str.Length; i++)
                {
                    charFit = i - 1;
                    RSize pSize = this.MeasureString(str.Substring(0, i), font);
                    if (pSize.Height <= size.Height && pSize.Width < maxWidth)
                    {
                        charFitWidth = pSize.Width;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
#if !MONO
                this.SetFont(font);

                var size = new Size();
                Win32Utils.GetTextExtentExPoint(this.Hdc, str, str.Length, (int)Math.Round(maxWidth), CharFit, CharFitWidth, ref size);
                charFit = CharFit[0];
                charFitWidth = charFit > 0 ? CharFitWidth[charFit - 1] : 0;
#endif
            }
        }

        public override void DrawString(string str, RFont font, RColor color, RPoint point, RSize size, bool rtl)
        {
            if (this.UseGdiPlusTextRendering)
            {
                this.ReleaseHdc();
                this.SetRtlAlignGdiPlus(rtl);
                var brush = ((BrushAdapter)this.Adapter.GetSolidBrush(color)).Brush;
                this.Graphics.DrawString(str, ((FontAdapter)font).Font, brush, (int)(Math.Round(point.X) + (rtl ? size.Width : 0)), (int)Math.Round(point.Y), StringFormat2);
            }
            else
            {
#if !MONO
                var pointConv = Utils.ConvertRound(point);
                var colorConv = Utils.Convert(color);

                if (color.A == 255)
                {
                    this.SetFont(font);
                    this.SetTextColor(colorConv);
                    this.SetRtlAlignGdi(rtl);

                    Win32Utils.TextOut(this.Hdc, pointConv.X, pointConv.Y, str, str.Length);
                }
                else
                {
                    this.InitHdc();
                    this.SetRtlAlignGdi(rtl);
                    DrawTransparentText(this.Hdc, str, font, pointConv, Utils.ConvertRound(size), colorConv);
                }
#endif
            }
        }

        public override RBrush GetTextureBrush(RImage image, RRect dstRect, RPoint translateTransformLocation)
        {
            var brush = new TextureBrush(((ImageAdapter)image).Image, Utils.Convert(dstRect));
            brush.TranslateTransform((float)translateTransformLocation.X, (float)translateTransformLocation.Y);
            return new BrushAdapter(brush, true);
        }

        public override RGraphicsPath GetGraphicsPath()
        {
            return new GraphicsPathAdapter();
        }

        public override void Dispose()
        {
            this.ReleaseHdc();
            if (this.ReleaseGraphics)
            {
                this.Graphics.Dispose();
            }

            if (this.UseGdiPlusTextRendering && this.SetRtl)
            {
                StringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
            }
        }

        #region Delegate graphics methods

        public override void DrawLine(RPen pen, double x1, double y1, double x2, double y2)
        {
            this.ReleaseHdc();
            this.Graphics.DrawLine(((PenAdapter)pen).Pen, (float)x1, (float)y1, (float)x2, (float)y2);
        }

        public override void DrawRectangle(RPen pen, double x, double y, double width, double height)
        {
            this.ReleaseHdc();
            this.Graphics.DrawRectangle(((PenAdapter)pen).Pen, (float)x, (float)y, (float)width, (float)height);
        }

        public override void DrawRectangle(RBrush brush, double x, double y, double width, double height)
        {
            this.ReleaseHdc();
            this.Graphics.FillRectangle(((BrushAdapter)brush).Brush, (float)x, (float)y, (float)width, (float)height);
        }

        public override void DrawImage(RImage image, RRect destRect, RRect srcRect)
        {
            this.ReleaseHdc();
            this.Graphics.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect), Utils.Convert(srcRect), GraphicsUnit.Pixel);
        }

        public override void DrawImage(RImage image, RRect destRect)
        {
            this.ReleaseHdc();
            this.Graphics.DrawImage(((ImageAdapter)image).Image, Utils.Convert(destRect));
        }

        public override void DrawPath(RPen pen, RGraphicsPath path)
        {
            this.Graphics.DrawPath(((PenAdapter)pen).Pen, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        public override void DrawPath(RBrush brush, RGraphicsPath path)
        {
            this.ReleaseHdc();
            this.Graphics.FillPath(((BrushAdapter)brush).Brush, ((GraphicsPathAdapter)path).GraphicsPath);
        }

        public override void DrawPolygon(RBrush brush, RPoint[] points)
        {
            if (points != null && points.Length > 0)
            {
                this.ReleaseHdc();
                this.Graphics.FillPolygon(((BrushAdapter)brush).Brush, Utils.Convert(points));
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Release current HDC to be able to use <see cref="System.Drawing.Graphics"/> methods.
        /// </summary>
        private void ReleaseHdc()
        {
#if !MONO
            if (this.Hdc != IntPtr.Zero)
            {
                Win32Utils.SelectClipRgn(this.Hdc, IntPtr.Zero);
                this.Graphics.ReleaseHdc(this.Hdc);
                this.Hdc = IntPtr.Zero;
            }
#endif
        }

#if !MONO
        /// <summary>
        /// Init HDC for the current graphics object to be used to call GDI directly.
        /// </summary>
        private void InitHdc()
        {
            if (this.Hdc == IntPtr.Zero)
            {
                var clip = this.Graphics.Clip.GetHrgn(this.Graphics);

                this.Hdc = this.Graphics.GetHdc();
                this.SetRtl = false;
                Win32Utils.SetBkMode(this.Hdc, 1);

                Win32Utils.SelectClipRgn(this.Hdc, clip);

                Win32Utils.DeleteObject(clip);
            }
        }

        /// <summary>
        /// Set a resource (e.g. a font) for the specified device context.
        /// WARNING: Calling Font.ToHfont() many times without releasing the font handle crashes the app.
        /// </summary>
        private void SetFont(RFont font)
        {
            this.InitHdc();
            Win32Utils.SelectObject(this.Hdc, ((FontAdapter)font).HFont);
        }

        /// <summary>
        /// Set the text color of the device context.
        /// </summary>
        private void SetTextColor(Color color)
        {
            this.InitHdc();
            int rgb = (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R;
            Win32Utils.SetTextColor(this.Hdc, rgb);
        }

        /// <summary>
        /// Change text align to Left-to-Right or Right-to-Left if required.
        /// </summary>
        private void SetRtlAlignGdi(bool rtl)
        {
            if (this.SetRtl)
            {
                if (!rtl)
                {
                    Win32Utils.SetTextAlign(this.Hdc, Win32Utils.TextAlignDefault);
                }
            }
            else if (rtl)
            {
                Win32Utils.SetTextAlign(this.Hdc, Win32Utils.TextAlignRtl);
            }

            this.SetRtl = rtl;
        }

        /// <summary>
        /// Special draw logic to draw transparent text using GDI.<br/>
        /// 1. Create in-memory DC<br/>
        /// 2. Copy background to in-memory DC<br/>
        /// 3. Draw the text to in-memory DC<br/>
        /// 4. Copy the in-memory DC to the proper location with alpha blend<br/>
        /// </summary>
        private static void DrawTransparentText(IntPtr hdc, string str, RFont font, Point point, Size size, Color color)
        {
            IntPtr dib;
            var memoryHdc = Win32Utils.CreateMemoryHdc(hdc, size.Width, size.Height, out dib);

            try
            {
                // copy target background to memory HDC so when copied back it will have the proper background
                Win32Utils.BitBlt(memoryHdc, 0, 0, size.Width, size.Height, hdc, point.X, point.Y, Win32Utils.BitBltCopy);

                // Create and select font
                Win32Utils.SelectObject(memoryHdc, ((FontAdapter)font).HFont);
                Win32Utils.SetTextColor(memoryHdc, (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R);

                // Draw text to memory HDC
                Win32Utils.TextOut(memoryHdc, 0, 0, str, str.Length);

                // copy from memory HDC to normal HDC with alpha blend so achieve the transparent text
                Win32Utils.AlphaBlend(hdc, point.X, point.Y, size.Width, size.Height, memoryHdc, 0, 0, size.Width, size.Height, new BlendFunction(color.A));
            }
            finally
            {
                Win32Utils.ReleaseMemoryHdc(memoryHdc, dib);
            }
        }
#endif

        /// <summary>
        /// Change text align to Left-to-Right or Right-to-Left if required.
        /// </summary>
        private void SetRtlAlignGdiPlus(bool rtl)
        {
            if (this.SetRtl)
            {
                if (!rtl)
                {
                    StringFormat2.FormatFlags ^= StringFormatFlags.DirectionRightToLeft;
                }
            }
            else if (rtl)
            {
                StringFormat2.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
            }

            this.SetRtl = rtl;
        }

        #endregion
    }
}