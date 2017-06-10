using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom.Exceptions
{
    /// <summary>
    /// This is the base class for exception that the DOM implementation may throw according
    /// to the DOM specification. See: http://www.w3.org/TR/2015/REC-dom-20151119/#errors
    /// </summary>
    [Serializable]
    public class DomException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomException"/> class.
        /// </summary>
        public DomException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DomException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public DomException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:global::System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:global::System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected DomException(
            global::System.Runtime.Serialization.SerializationInfo info,
            global::System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
