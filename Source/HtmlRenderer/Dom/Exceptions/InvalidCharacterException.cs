using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom.Exceptions
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#invalidcharactererror
    /// </summary>
    [Serializable]
    public class InvalidCharacterException : DomException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCharacterException"/> class.
        /// </summary>
        public InvalidCharacterException()
            : this("The string contains invalid characters.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCharacterException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidCharacterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCharacterException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public InvalidCharacterException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCharacterException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:global::System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:global::System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected InvalidCharacterException(
            global::System.Runtime.Serialization.SerializationInfo info,
            global::System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}