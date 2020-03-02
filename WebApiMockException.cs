using System;
using System.Runtime.Serialization;

namespace WebApiMock {
    /// <summary>
    /// An exception thrown by the web API mock-up service.
    /// </summary>
    public class WebApiMockException : ApplicationException {
        
        /// <inheritdoc/>
        public WebApiMockException() { ErrorCode = 0; }

        /// <inheritdoc/>
        public WebApiMockException(string message, int errorCode) : base(message) { ErrorCode = errorCode; }

        /// <inheritdoc/>
        public WebApiMockException(string message, int errorCode, Exception innerException) : base(message, innerException) { ErrorCode = errorCode; }

        /// <inheritdoc/>
        protected WebApiMockException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The error code that is associated with this exception.
        /// </summary>
        public int ErrorCode { get; }
    }
}
