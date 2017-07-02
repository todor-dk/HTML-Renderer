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

using System.Collections.Generic;
using Scientia.HtmlRenderer.Core.Utils;

namespace Scientia.HtmlRenderer.Core.Dom
{
    internal sealed class HtmlTag
    {
        #region Fields and Consts

        /// <summary>
        /// the name of the html tag
        /// </summary>
        private readonly string _Name;

        /// <summary>
        /// if the tag is single placed; in other words it doesn't have a separate closing tag;
        /// </summary>
        private readonly bool _IsSingle;

        /// <summary>
        /// collection of attributes and their value the html tag has
        /// </summary>
        private readonly Dictionary<string, string> _Attributes;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="name">the name of the html tag</param>
        /// <param name="isSingle">if the tag is single placed; in other words it doesn't have a separate closing tag;</param>
        /// <param name="attributes">collection of attributes and their value the html tag has</param>
        public HtmlTag(string name, bool isSingle, Dictionary<string, string> attributes = null)
        {
            ArgChecker.AssertArgNotNullOrEmpty(name, "name");

            this._Name = name;
            this._IsSingle = isSingle;
            this._Attributes = attributes;
        }

        /// <summary>
        /// Gets the name of this tag
        /// </summary>
        public string Name
        {
            get { return this._Name; }
        }

        /// <summary>
        /// Gets collection of attributes and their value the html tag has
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get { return this._Attributes; }
        }

        /// <summary>
        /// Gets if the tag is single placed; in other words it doesn't have a separate closing tag; <br/>
        /// e.g. &lt;br&gt;
        /// </summary>
        public bool IsSingle
        {
            get { return this._IsSingle; }
        }

        /// <summary>
        /// is the html tag has attributes.
        /// </summary>
        /// <returns>true - has attributes, false - otherwise</returns>
        public bool HasAttributes()
        {
            return this._Attributes != null && this._Attributes.Count > 0;
        }

        /// <summary>
        /// Gets a boolean indicating if the attribute list has the specified attribute
        /// </summary>
        /// <param name="attribute">attribute name to check if exists</param>
        /// <returns>true - attribute exists, false - otherwise</returns>
        public bool HasAttribute(string attribute)
        {
            return this._Attributes != null && this._Attributes.ContainsKey(attribute);
        }

        /// <summary>
        /// Get attribute value for given attribute name or null if not exists.
        /// </summary>
        /// <param name="attribute">attribute name to get by</param>
        /// <param name="defaultValue">optional: value to return if attribute is not specified</param>
        /// <returns>attribute value or null if not found</returns>
        public string TryGetAttribute(string attribute, string defaultValue = null)
        {
            return this._Attributes != null && this._Attributes.ContainsKey(attribute) ? this._Attributes[attribute] : defaultValue;
        }

        public override string ToString()
        {
            return string.Format("<{0}>", this._Name);
        }
    }
}