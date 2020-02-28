using System;
using System.Runtime.Serialization;

namespace WebApiMock {
    /// <summary>
    /// An exception thrown by the web API mock-up service.
    /// </summary>
    public class WebApiMockException : ApplicationException {
        public WebApiMockException() { ErrorCode = 0; }

        public WebApiMockException(string message, int errorCode) : base(message) { ErrorCode = errorCode; }

        public WebApiMockException(string message, int errorCode, Exception innerException) : base(message, innerException) { ErrorCode = errorCode; }

        protected WebApiMockException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// The error code that is associated with this exception.
        /// </summary>
        public int ErrorCode { get; }
    }
}
