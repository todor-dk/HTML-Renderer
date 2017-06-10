using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheArtOfDev.HtmlRenderer.Dom.Exceptions
{
    /// <summary>
    /// See: http://www.w3.org/TR/2015/REC-dom-20151119/#hierarchyrequesterror
    /// </summary>
    [Serializable]
    public class HierarchyRequestException : DomException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyRequestException"/> class.
        /// </summary>
        public HierarchyRequestException()
            : this ("The operation would yield an incorrect node tree.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyRequestException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HierarchyRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyRequestException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public HierarchyRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchyRequestException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:global::System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:global::System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected HierarchyRequestException(
            global::System.Runtime.Serialization.SerializationInfo info,
            global::System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
