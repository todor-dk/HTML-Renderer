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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Scientia.HtmlRenderer.Core.Entities;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.WPF
{
    /// <summary>
    /// Provides HTML rendering using the text property.<br/>
    /// WPF control that will render html content in it's client rectangle.<br/>
    /// If the layout of the html resulted in its content beyond the client bounds of the panel it will show scrollbars (horizontal/vertical) allowing to scroll the content.<br/>
    /// The control will handle mouse and keyboard events on it to support html text selection, copy-paste and mouse clicks.<br/>
    /// </summary>
    /// <remarks>
    /// See <see cref="HtmlControl"/> for more info.
    /// </remarks>
    public class HtmlPanel : HtmlControl
    {
        #region Fields and Consts

        /// <summary>
        /// the vertical scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar VerticalScrollBar;

        /// <summary>
        /// the horizontal scroll bar for the control to scroll to html content out of view
        /// </summary>
        protected ScrollBar HorizontalScrollBar;

        #endregion

        static HtmlPanel()
        {
            BackgroundProperty.OverrideMetadata(typeof(HtmlPanel), new FrameworkPropertyMetadata(SystemColors.WindowBrush));

            TextProperty.OverrideMetadata(typeof(HtmlPanel), new PropertyMetadata(null, OnTextProperty_change));
        }

        /// <summary>
        /// Creates a new HtmlPanel and sets a basic css for it's styling.
        /// </summary>
        public HtmlPanel()
        {
            this.VerticalScrollBar = new ScrollBar();
            this.VerticalScrollBar.Orientation = Orientation.Vertical;
            this.VerticalScrollBar.Width = 18;
            this.VerticalScrollBar.Scroll += this.OnScrollBarScroll;
            this.AddVisualChild(this.VerticalScrollBar);
            this.AddLogicalChild(this.VerticalScrollBar);

            this.HorizontalScrollBar = new ScrollBar();
            this.HorizontalScrollBar.Orientation = Orientation.Horizontal;
            this.HorizontalScrollBar.Height = 18;
            this.HorizontalScrollBar.Scroll += this.OnScrollBarScroll;
            this.AddVisualChild(this.HorizontalScrollBar);
            this.AddLogicalChild(this.HorizontalScrollBar);

            this.HtmlContainer.ScrollChange += this.OnScrollChange;
        }

        /// <summary>
        /// Adjust the scrollbar of the panel on html element by the given id.<br/>
        /// The top of the html element rectangle will be at the top of the panel, if there
        /// is not enough height to scroll to the top the scroll will be at maximum.<br/>
        /// </summary>
        /// <param name="elementId">the id of the element to scroll to</param>
        public virtual void ScrollToElement(string elementId)
        {
            ArgChecker.AssertArgNotNullOrEmpty(elementId, "elementId");

            if (this.HtmlContainer != null)
            {
                var rect = this.HtmlContainer.GetElementRectangle(elementId);
                if (rect.HasValue)
                {
                    this.ScrollToPoint(rect.Value.Location.X, rect.Value.Location.Y);
                    this.HtmlContainer.HandleMouseMove(this, Mouse.GetPosition(this));
                }
            }
        }

        #region Private methods

        protected override int VisualChildrenCount
        {
            get { return 2; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return this.VerticalScrollBar;
            }
            else if (index == 1)
            {
                return this.HorizontalScrollBar;
            }

            return null;
        }

        /// <summary>
        /// Perform the layout of the html in the control.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = this.PerformHtmlLayout(constraint);

            // to handle if scrollbar is appearing or disappearing
            bool relayout = false;
            var htmlWidth = this.HtmlWidth(constraint);
            var htmlHeight = this.HtmlHeight(constraint);

            if ((this.VerticalScrollBar.Visibility == Visibility.Hidden && size.Height > htmlHeight) ||
                (this.VerticalScrollBar.Visibility == Visibility.Visible && size.Height <= htmlHeight))
            {
                this.VerticalScrollBar.Visibility = this.VerticalScrollBar.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                relayout = true;
            }

            if ((this.HorizontalScrollBar.Visibility == Visibility.Hidden && size.Width > htmlWidth) ||
                (this.HorizontalScrollBar.Visibility == Visibility.Visible && size.Width <= htmlWidth))
            {
                this.HorizontalScrollBar.Visibility = this.HorizontalScrollBar.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                relayout = true;
            }

            if (relayout)
            {
                this.PerformHtmlLayout(constraint);
            }

            if (double.IsPositiveInfinity(constraint.Width) || double.IsPositiveInfinity(constraint.Height))
            {
                constraint = size;
            }

            return constraint;
        }

        /// <summary>
        /// After measurement arrange the scrollbars of the panel.
        /// </summary>
        protected override Size ArrangeOverride(Size bounds)
        {
            var scrollHeight = this.HtmlHeight(bounds) + this.Padding.Top + this.Padding.Bottom;
            scrollHeight = scrollHeight > 1 ? scrollHeight : 1;
            var scrollWidth = this.HtmlWidth(bounds) + this.Padding.Left + this.Padding.Right;
            scrollWidth = scrollWidth > 1 ? scrollWidth : 1;
            this.VerticalScrollBar.Arrange(new Rect(System.Math.Max(bounds.Width - this.VerticalScrollBar.Width - this.BorderThickness.Right, 0), this.BorderThickness.Top, this.VerticalScrollBar.Width, scrollHeight));
            this.HorizontalScrollBar.Arrange(new Rect(this.BorderThickness.Left, System.Math.Max(bounds.Height - this.HorizontalScrollBar.Height - this.BorderThickness.Bottom, 0), scrollWidth, this.HorizontalScrollBar.Height));

            if (this.HtmlContainer != null)
            {
                if (this.VerticalScrollBar.Visibility == Visibility.Visible)
                {
                    this.VerticalScrollBar.ViewportSize = this.HtmlHeight(bounds);
                    this.VerticalScrollBar.SmallChange = 25;
                    this.VerticalScrollBar.LargeChange = this.VerticalScrollBar.ViewportSize * .9;
                    this.VerticalScrollBar.Maximum = this.HtmlContainer.ActualSize.Height - this.VerticalScrollBar.ViewportSize;
                }

                if (this.HorizontalScrollBar.Visibility == Visibility.Visible)
                {
                    this.HorizontalScrollBar.ViewportSize = this.HtmlWidth(bounds);
                    this.HorizontalScrollBar.SmallChange = 25;
                    this.HorizontalScrollBar.LargeChange = this.HorizontalScrollBar.ViewportSize * .9;
                    this.HorizontalScrollBar.Maximum = this.HtmlContainer.ActualSize.Width - this.HorizontalScrollBar.ViewportSize;
                }

                // update the scroll offset because the scroll values may have changed
                this.UpdateScrollOffsets();
            }

            return bounds;
        }

        /// <summary>
        /// Perform html container layout by the current panel client size.
        /// </summary>
        protected Size PerformHtmlLayout(Size constraint)
        {
            if (this.HtmlContainer != null)
            {
                this.HtmlContainer.MaxSize = new Size(this.HtmlWidth(constraint), 0);
                this.HtmlContainer.PerformLayout();
                return this.HtmlContainer.ActualSize;
            }

            return Size.Empty;
        }

        /// <summary>
        /// Handle minor case where both scroll are visible and create a rectangle at the bottom right corner between them.
        /// </summary>
        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            // render rectangle in right bottom corner where both scrolls meet
            if (this.HorizontalScrollBar.Visibility == Visibility.Visible && this.VerticalScrollBar.Visibility == Visibility.Visible)
            {
                context.DrawRectangle(SystemColors.ControlBrush, null, new Rect(this.BorderThickness.Left + this.HtmlWidth(this.RenderSize), this.BorderThickness.Top + this.HtmlHeight(this.RenderSize), this.VerticalScrollBar.Width, this.HorizontalScrollBar.Height));
            }
        }

        /// <summary>
        /// Handle mouse up to set focus on the control.
        /// </summary>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.Focus();
        }

        /// <summary>
        /// Handle mouse wheel for scrolling.
        /// </summary>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (this.VerticalScrollBar.Visibility == Visibility.Visible)
            {
                this.VerticalScrollBar.Value -= e.Delta;
                this.UpdateScrollOffsets();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handle key down event for selection, copy and scrollbars handling.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (this.VerticalScrollBar.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Up)
                {
                    this.VerticalScrollBar.Value -= this.VerticalScrollBar.SmallChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    this.VerticalScrollBar.Value += this.VerticalScrollBar.SmallChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.PageUp)
                {
                    this.VerticalScrollBar.Value -= this.VerticalScrollBar.LargeChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.PageDown)
                {
                    this.VerticalScrollBar.Value += this.VerticalScrollBar.LargeChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.Home)
                {
                    this.VerticalScrollBar.Value = 0;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.End)
                {
                    this.VerticalScrollBar.Value = this.VerticalScrollBar.Maximum;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
            }

            if (this.HorizontalScrollBar.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Left)
                {
                    this.HorizontalScrollBar.Value -= this.HorizontalScrollBar.SmallChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    this.HorizontalScrollBar.Value += this.HorizontalScrollBar.SmallChange;
                    this.UpdateScrollOffsets();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected override double HtmlWidth(Size size)
        {
            var width = base.HtmlWidth(size) - (this.VerticalScrollBar.Visibility == Visibility.Visible ? this.VerticalScrollBar.Width : 0);
            return width > 1 ? width : 1;
        }

        /// <summary>
        /// Get the width the HTML has to render in (not including vertical scroll iff it is visible)
        /// </summary>
        protected override double HtmlHeight(Size size)
        {
            var height = base.HtmlHeight(size) - (this.HorizontalScrollBar.Visibility == Visibility.Visible ? this.HorizontalScrollBar.Height : 0);
            return height > 1 ? height : 1;
        }

        /// <summary>
        /// On HTML container scroll change request scroll to the requested location.
        /// </summary>
        private void OnScrollChange(object sender, HtmlScrollEventArgs e)
        {
            this.ScrollToPoint(e.X, e.Y);
        }

        /// <summary>
        /// Set the control scroll offset to the given values.
        /// </summary>
        private void ScrollToPoint(double x, double y)
        {
            this.HorizontalScrollBar.Value = x;
            this.VerticalScrollBar.Value = y;
            this.UpdateScrollOffsets();
        }

        /// <summary>
        /// On scrollbar scroll update the scroll offsets and invalidate.
        /// </summary>
        private void OnScrollBarScroll(object sender, ScrollEventArgs e)
        {
            this.UpdateScrollOffsets();
        }

        /// <summary>
        /// Update the scroll offset of the HTML container and invalidate visual to re-render.
        /// </summary>
        private void UpdateScrollOffsets()
        {
            var newScrollOffset = new Point(-this.HorizontalScrollBar.Value, -this.VerticalScrollBar.Value);
            if (!newScrollOffset.Equals(this.HtmlContainer.ScrollOffset))
            {
                this.HtmlContainer.ScrollOffset = newScrollOffset;
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// On text property change reset the scrollbars to zero.
        /// </summary>
        private static void OnTextProperty_change(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as HtmlPanel;
            if (panel != null)
            {
                panel.HorizontalScrollBar.Value = panel.VerticalScrollBar.Value = 0;
            }
        }

        #endregion
    }
}