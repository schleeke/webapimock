using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiMock.Data {
    public class RequestDefinition {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Route { get; set; }

        [Required]
        public HttpMethodEnum Method { get; set; }

        public string Query { get; set; }

        public string Body { get; set; }

        [Required]
        public int MockupResponseId { get; set; }

        [ForeignKey(nameof(MockupResponseId))]
        public ResponseDefinition Response { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
