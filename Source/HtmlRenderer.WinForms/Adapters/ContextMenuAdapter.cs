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
using System.Windows.Forms;
using Scientia.HtmlRenderer.Adapters.Entities;
using Scientia.HtmlRenderer.Core.Utils;
using Scientia.HtmlRenderer.Adapters;
using Scientia.HtmlRenderer.WinForms.Utilities;

namespace Scientia.HtmlRenderer.WinForms.Adapters
{
    /// <summary>
    /// Adapter for WinForms context menu for core.
    /// </summary>
    internal sealed class ContextMenuAdapter : RContextMenu
    {
        #region Fields and Consts

        /// <summary>
        /// the underline win forms context menu
        /// </summary>
        private readonly ContextMenuStrip ContextMenu;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        public ContextMenuAdapter()
        {
            this.ContextMenu = new ContextMenuStrip();
            this.ContextMenu.ShowImageMargin = false;
        }

        public override int ItemsCount
        {
            get { return this.ContextMenu.Items.Count; }
        }

        public override void AddDivider()
        {
            this.ContextMenu.Items.Add("-");
        }

        public override void AddItem(string text, bool enabled, EventHandler onClick)
        {
            ArgChecker.AssertArgNotNullOrEmpty(text, "text");
            ArgChecker.AssertArgNotNull(onClick, "onClick");

            var item = this.ContextMenu.Items.Add(text, null, onClick);
            item.Enabled = enabled;
        }

        public override void RemoveLastDivider()
        {
            if (this.ContextMenu.Items[this.ContextMenu.Items.Count - 1].Text == string.Empty)
            {
                this.ContextMenu.Items.RemoveAt(this.ContextMenu.Items.Count - 1);
            }
        }

        public override void Show(RControl parent, RPoint location)
        {
            this.ContextMenu.Show(((ControlAdapter)parent).Control, Utils.ConvertRound(location));
        }

        public override void Dispose()
        {
            this.ContextMenu.Dispose();
        }
    }
}