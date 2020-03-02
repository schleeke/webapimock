using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMock.Data {
    /// <summary>
    /// Data definition of a mock-up response.
    /// </summary>
    public class ResponseDefinition {
        /// <summary>
        /// The id of the record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The HTTP status code of the response.
        /// </summary>
        [Required]
        public int StatusCode { get; set; }

        /// <summary>
        /// The content of the response (can be null/empty).
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// The MIME type for the <see cref="Response"/>.
        /// </summary>
        [MaxLength(255, ErrorMessage = "Only 255 characters are allowed.")]
        public string MimeType { get; set; }

        /// <summary>
        /// A list of requests that are using this response.
        /// </summary>
        /// <remarks>
        /// Used by EF.
        /// </remarks>
        public ICollection<RequestDefinition> Requests { get; set; }

        /// <summary>
        /// The timestampt of the record's last change.
        /// </summary>
        [Timestamp]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Defined by EF.")]
        public byte[] Timestamp { get; set; }
    }
}
