using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMock.Data;

namespace WebApiMock.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase {
        private readonly DataService _data;
        public ResponseController(DataService data) => _data = data;

        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IEnumerable<MockupResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MockupResponse>>> Get() {
            MockupResponse[] result = _data.GetResponses();
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MockupResponse>> Get(int id) {
            MockupResponse result;
            try {
                result = _data.GetResponseById(id); }
            catch (WebApiMockException ex) {
                if (ex.ErrorCode == 11) {
                    result = null; }
                else {
                    throw; } }
            if (result == null) {
                return NotFound($"No request found with id #{id}."); }
            return Ok(result);
        }

        [HttpPut]
        [Route("")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<MockupResponse>> Put([FromBody]MockupResponse response) {
            if(response.Response == null) { response.Response = ""; }
            if(response.MimeType == null) { response.MimeType = ""; }
            if (response.Id > 0) {
                return StatusCode(409, "The new response definition does not have an empty id.");
            }
            if (_data.ResponseExists(response.StatusCode, response.Response, response.MimeType)) {
                return BadRequest("A response with the given values already exists."); }
            var retVal = _data.AddResponse(response);
            return Ok(retVal);
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id) {
            if (id == 0) {
                return BadRequest("The id is 0.");
            }
            if (!_data.ResponseExistsForId(id)) {
                return NotFound($"No request with id #{id} found.");
            }
            _data.RemoveResponse(id);
            return Ok();
        }


        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MockupResponse>> Patch(int id, [FromBody] MockupResponse response) {
            MockupResponse existingResponse;

            if (response.Id < 1) {
                return StatusCode(409, "The response definition has an empty id."); }
            if (!response.Id.Equals(id)) {
                return StatusCode(406, "Ids mismatch."); }
            if (!_data.ResponseExistsForId(response.Id)) {
                return NotFound($"No response definition found with id #{id}."); }
            existingResponse = _data.GetResponseById(id);
            if(existingResponse.StatusCode != response.StatusCode) {
                _data.SetResponseStatusCode(id, response.StatusCode); }
            if (!existingResponse.Response.Equals(response.Response)) {
                _data.SetResponseResponse(id, response.Response); }
            if (!existingResponse.MimeType.Equals(response.MimeType)) {
                _data.SetResponseMimeType(id, response.MimeType); }
            return Ok(response);
        }



    }
}