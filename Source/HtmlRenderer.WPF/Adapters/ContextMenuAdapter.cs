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
using System.Windows;
using System.Windows.Controls;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;
using TheArtOfDev.HtmlRenderer.WPF.Utilities;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters
{
    /// <summary>
    /// Adapter for WPF context menu for core.
    /// </summary>
    internal sealed class ContextMenuAdapter : RContextMenu
    {
        #region Fields and Consts

        /// <summary>
        /// the underline WPF context menu
        /// </summary>
        private readonly ContextMenu ContextMenu;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public ContextMenuAdapter()
        {
            this.ContextMenu = new ContextMenu();
        }

        public override int ItemsCount
        {
            get { return this.ContextMenu.Items.Count; }
        }

        public override void AddDivider()
        {
            this.ContextMenu.Items.Add(new Separator());
        }

        public override void AddItem(string text, bool enabled, EventHandler onClick)
        {
            ArgChecker.AssertArgNotNullOrEmpty(text, "text");
            ArgChecker.AssertArgNotNull(onClick, "onClick");

            var item = new MenuItem();
            item.Header = text;
            item.IsEnabled = enabled;
            item.Click += new RoutedEventHandler(onClick);
            this.ContextMenu.Items.Add(item);
        }

        public override void RemoveLastDivider()
        {
            if (this.ContextMenu.Items[this.ContextMenu.Items.Count - 1].GetType() == typeof(Separator))
            {
                this.ContextMenu.Items.RemoveAt(this.ContextMenu.Items.Count - 1);
            }
        }

        public override void Show(RControl parent, RPoint location)
        {
            this.ContextMenu.PlacementTarget = ((ControlAdapter)parent).Control;
            this.ContextMenu.PlacementRectangle = new Rect(Utils.ConvertRound(location), Size.Empty);
            this.ContextMenu.IsOpen = true;
        }

        public override void Dispose()
        {
            this.ContextMenu.IsOpen = false;
            this.ContextMenu.PlacementTarget = null;
            this.ContextMenu.Items.Clear();
        }
    }
}