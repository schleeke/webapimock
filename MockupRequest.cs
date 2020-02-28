using System.ComponentModel.DataAnnotations;

namespace WebApiMock {
    /// <summary>
    /// A definition for a request whose response should be mocked. 
    /// </summary>
    public class MockupRequest {

        /// <summary>
        /// The id of the mockup request definition record.
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The route (relative url) for the request.
        /// </summary>
        /// <remarks>
        /// Do not use leading slashes (/).
        /// </remarks>
        /// <example>
        /// myroute/subcontent
        /// </example>
        [Required]
        public string Route { get; set; }

        /// <summary>
        /// The HTTP method type for the request definition.
        /// </summary>
        /// <remarks>
        /// Only GET, PUT, POST, DELETE and PATCH are available.
        /// </remarks>
        /// <example>GET</example>
        [Required]
        public string HttpMethod { get; set; }

        /// <summary>
        /// An optional query string.
        /// </summary>
        /// <example>
        /// var1=value1&amp;var2=value2
        /// </example>
        public string Query { get; set; }

        /// <summary>
        /// The body to match for this request definition.
        /// </summary>
        /// <remarks>
        /// Not noted with GET methods.
        /// </remarks>
        public string Body { get; set; }

        /// <summary>
        /// The id to the <see cref="MockupResponse"/> that is linked to the request definition.
        /// </summary>
        [Required]
        public int ResponseId { get; set; }
    }
}
