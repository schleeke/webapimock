using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WebApiMock {
    /// <summary>
    /// A definition for a mock-up response related to a given <see cref="MockupRequest"/>.
    /// </summary>
    public class MockupResponse {
        
        /// <summary>
        /// The id of the record.
        /// </summary>
        [Required]
        public int Id { get; set; }
        
        /// <summary>
        /// The HTTP status code for the response.
        /// </summary>
        [Required]
        public int StatusCode { get; set; }
        
        /// <summary>
        /// The MIME type of the reponse.
        /// </summary>
        /// <remarks>
        /// Must be filled if <see cref="Response"/> is set.
        /// </remarks>
        /// <example>
        /// application/json
        /// </example>
        public string MimeType { get; set; }
        
        /// <summary>
        /// The content of the response.
        /// </summary>
        /// <remarks>
        /// Must match the <see cref="MimeType"/> that is refered.
        /// <see cref="MimeType"/> must also be set if set.
        /// </remarks>
        public string Response { get; set; }

        /// <inheritdoc/>
        public override string ToString() {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var retVal = JsonSerializer.Serialize(this, options);
            return retVal;
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="MockupResponse"/>.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>Returns a <see cref="MockupResponse"/> with the values from the JSON string.</returns>
        public static MockupResponse FromJson(string json) {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var retVal = JsonSerializer.Deserialize<MockupResponse>(json, options);
            return retVal;
        }

    }
}
