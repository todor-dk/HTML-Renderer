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

using TheArtOfDev.HtmlRenderer.Core;

namespace TheArtOfDev.HtmlRenderer.Adapters.Entities
{
    /// <summary>
    /// Even class for handling keyboard events in <see cref="HtmlContainerInt"/>.
    /// </summary>
    public sealed class RKeyEvent
    {
        /// <summary>
        /// is control is pressed
        /// </summary>
        private readonly bool _Control;

        /// <summary>
        /// is 'A' key is pressed
        /// </summary>
        private readonly bool _AKeyCode;

        /// <summary>
        /// is 'C' key is pressed
        /// </summary>
        private readonly bool _CKeyCode;

        /// <summary>
        /// Init.
        /// </summary>
        public RKeyEvent(bool control, bool aKeyCode, bool cKeyCode)
        {
            this._Control = control;
            this._AKeyCode = aKeyCode;
            this._CKeyCode = cKeyCode;
        }

        /// <summary>
        /// is control is pressed
        /// </summary>
        public bool Control
        {
            get { return this._Control; }
        }

        /// <summary>
        /// is 'A' key is pressed
        /// </summary>
        public bool AKeyCode
        {
            get { return this._AKeyCode; }
        }

        /// <summary>
        /// is 'C' key is pressed
        /// </summary>
        public bool CKeyCode
        {
            get { return this._CKeyCode; }
        }
    }
}