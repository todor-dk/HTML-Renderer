using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Html5.Parsing
{
    /// <summary>
    /// Represents an attribute of a parsed tag. This is used to comminucate
    /// information from the <see cref="Tokenizer"/> to the <see cref="DomParser"/>.
    /// Eventually, this will be converted to DOM element attributes.
    /// </summary>
    internal struct Attribute
    {
        public static readonly Attribute[] None = Array.Empty<Attribute>();

        public readonly string Name;

        public readonly string Value;

        public Attribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        #region Equality Members

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return (this.Name == null) ? 0 : this.Name.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified Attribute is equal to the current Attribute.
        /// </summary>
        /// <param name="other">Another Attribute to compare to this Attribute.</param>
        /// <returns><c>true</c> if this Attribute equals the given Attribute, <c>false</c> otherwise.</returns>
        public bool Equals(Attribute other)
        {
            return String.Equals(this.Name, other.Name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Attribute))
                return false;

            return this.Equals((Attribute)obj);
        }

        /// <summary>
        /// Compares whether the left Attribute operand is equal to the right Attribute operand.
        /// </summary>
        /// <param name="left">The left Attribute operand.</param>
        /// <param name="right">The right Attribute operand.</param>
        /// <returns>The result of the equality operator.</returns>
        public static bool operator ==(Attribute left, Attribute right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares whether the left Attribute operand is not equal to the right Attribute operand.
        /// </summary>
        /// <param name="left">The left Attribute operand.</param>
        /// <param name="right">The right Attribute operand.</param>
        /// <returns>The result of the inequality operator.</returns>
        public static bool operator !=(Attribute left, Attribute right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
