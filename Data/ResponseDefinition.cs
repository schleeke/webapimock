using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMock.Data {
    public class ResponseDefinition {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int StatusCode { get; set; }

        public string Response { get; set; }

        [MaxLength(255, ErrorMessage = "Only 255 characters are allowed.")]
        public string MimeType { get; set; }

        public ICollection<RequestDefinition> Requests { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
