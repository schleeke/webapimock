using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMock.Data {
    /// <summary>
    /// The data definition for a mock-up request.
    /// </summary>
    public class RequestDefinition {
        /// <summary>
        /// The id of the record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The route/relative path for the request.
        /// </summary>
        [Required]
        public string Route { get; set; }

        /// <summary>
        /// The HTTP method of the request.
        /// </summary>
        [Required]
        public HttpMethodEnum Method { get; set; }

        /// <summary>
        /// The query part of the request's URL.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The body content of the request.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The id of the corresponding <see cref="ResponseDefinition"/>.
        /// </summary>
        [Required]
        public int MockupResponseId { get; set; }

        /// <summary>
        /// The corresponding response.
        /// </summary>
        [ForeignKey(nameof(MockupResponseId))]
        public ResponseDefinition Response { get; set; }

        /// <summary>
        /// The timestamp of the last change. For change tracking purpose.
        /// </summary>
        [Timestamp]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Given by EF")]
        public byte[] Timestamp { get; set; }
    }
}
