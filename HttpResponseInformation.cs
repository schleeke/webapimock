namespace WebApiMock
{
    /// <summary>
    /// Represents a HTTP response with all information needed to generate it.
    /// </summary>
    public class HttpResponseInformation {

        /// <summary>
        /// Standard constructor sets the Status Code to 200 and the Content Type to 'application/json'.
        /// </summary>
        public HttpResponseInformation() {
            StatusCode = 200;
            ContentType = "application/json";
        }

        /// <summary>
        /// The HTTP status code that shall be returned.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The content that should be returned.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The content type.
        /// </summary>
        public string ContentType { get; set; }
    }
}
