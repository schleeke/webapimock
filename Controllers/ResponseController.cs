using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMock.Data;

namespace WebApiMock.Controllers {
    /// <inheritdoc/>
    [Route("[controller]")]
    [ApiController]
    public class ResponseController : ControllerBase {
        private readonly DataService _data;

        /// <inheritdoc/>
        public ResponseController(DataService data) => _data = data;

        /// <summary>
        /// Gets a list of all existing response definitions.
        /// </summary>
        /// <returns>A list of mock-up responses.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /response
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<MockupResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MockupResponse>>> Get() {
            Logger.Info("Executing ToolsController.Get()");
            MockupResponse[] result = _data.GetResponses();
            Logger.Info($"Returning {result.Length} responses.");
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific response definition.
        /// </summary>
        /// <param name="id">The id of the response definition.</param>
        /// <returns>The mock-up response with the given id.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /response/5
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="404">A response with the given id was not found.</response>
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MockupResponse>> Get(int id) {
            MockupResponse result;
            Logger.Info($"Executing ToolsController.Get({id})");
            try {
                result = _data.GetResponseById(id); }
            catch (WebApiMockException ex) {
                if (ex.ErrorCode == 11) {
                    result = null; }
                else {
                    Logger.Error(ex.GetFullMessage());
                    throw; } }
            if (result == null) {
                Logger.Warn($"No response found for id #{id}.");
                return NotFound($"No response found with id #{id}."); }
            Logger.Info($"Successfully fetched info for id #{id}.");
            return Ok(result);
        }

        /// <summary>
        /// Adds a new response definition.
        /// </summary>
        /// <param name="response">The mock-up response to add.</param>
        /// <returns>The added mock-up response with an updated Id value.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// PUT /response
        /// {
        ///   "Id": 0,
        ///   "StatusCode": 205,
        ///   "MimeType": "text/plain" |null,
        ///   "Response": "my cool response" | null
        /// }
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">A response with the given values already exists.</response>
        /// <response code="409">The id is not 0 (zero).</response>
        [HttpPut]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<MockupResponse>> Put([FromBody]MockupResponse response) {
            Logger.Info($"Executing ToolsController.Put({response})");
            if(response.Response == null) { response.Response = ""; }
            if(response.MimeType == null) { response.MimeType = ""; }
            if (response.Id > 0) {
                Logger.Error("The new response definition does not have an empty id.");
                return StatusCode(409, "The new response definition does not have an empty id.");
            }
            if (_data.ResponseExists(response.StatusCode, response.Response, response.MimeType)) {
                Logger.Error("A response with the given values already exists.");
                return BadRequest("A response with the given values already exists."); }
            var retVal = _data.AddResponse(response);
            Logger.Info($"Successfully created new reponse with id #{retVal.Id}.");
            return Ok(retVal);
        }

        /// <summary>
        /// Removes a mock-up response from the database.
        /// </summary>
        /// <param name="id">The id of the mock-up response that should be removed.</param>
        /// <returns>Nothing</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// DELETE /response/18
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">The id is 0 (zero).</response>
        /// <response code="404">No response with the given id found.</response>
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id) {
            Logger.Info($"Executing ToolsController.Delete({id})");
            if (id == 0) {
                Logger.Error("The id is 0.");
                return BadRequest("The id is 0.");
            }
            if (!_data.ResponseExistsForId(id)) {
                Logger.Error($"No request with id #{id} found.");
                return NotFound($"No request with id #{id} found.");
            }
            _data.RemoveResponse(id);
            Logger.Info($"Successfully removed response with id #{id}.");
            return Ok();
        }

        /// <summary>
        /// Updates the values of an existing mock-up response.
        /// </summary>
        /// <param name="id">The id of the mock-up response that should be updated.</param>
        /// <param name="response">A mock-up response with the new values.</param>
        /// <remarks>
        /// Sample request:
        /// 
        /// PATCH /response/18
        /// {
        ///   "Id": 18,
        ///   "StatusCode": 205,
        ///   "MimeType": "text/plain",
        ///   "Response": "my new response value"
        /// }
        /// </remarks>
        /// <returns>The updated mock-up response.</returns>
        [HttpPatch]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(typeof(MockupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MockupResponse>> Patch(int id, [FromBody] MockupResponse response) {
            MockupResponse existingResponse;
            Logger.Info($"Executing ToolsController.Patch({id}, {response})");
            if (response.Id < 1) {
                Logger.Error("The response definition has an empty id.");
                return StatusCode(409, "The response definition has an empty id."); }
            if (!response.Id.Equals(id)) {
                Logger.Error("Ids mismatch.");
                return StatusCode(406, "Ids mismatch."); }
            if (!_data.ResponseExistsForId(response.Id)) {
                Logger.Error($"No response definition found with id #{id}.");
                return NotFound($"No response definition found with id #{id}."); }
            existingResponse = _data.GetResponseById(id);
            if(existingResponse.StatusCode != response.StatusCode) {
                Logger.Debug("Updating HTTP status code.");
                _data.SetResponseStatusCode(id, response.StatusCode); }
            if (!existingResponse.Response.Equals(response.Response)) {
                Logger.Debug("Updating response content.");
                _data.SetResponseResponse(id, response.Response); }
            if (!existingResponse.MimeType.Equals(response.MimeType)) {
                Logger.Debug("Updating MIME type.");
                _data.SetResponseMimeType(id, response.MimeType); }
            Logger.Info($"Successfully updated response #{id}.");
            return Ok(response);
        }

        /// <summary>
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));


    }
}