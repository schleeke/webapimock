using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiMock.Data;

namespace WebApiMock.Controllers {



    #pragma warning disable 1998
    /// <inheritdoc/>
    [Route("[controller]")]
    [ApiController]
    public class RequestController : ControllerBase {

        /// <summary>
        /// Gets a list of all existing request definitions.
        /// </summary>
        /// <returns>A list of mock-up request.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /request
        /// </remarks>
        /// <response code="200">All OK.</response>
        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<MockupRequest>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MockupRequest>>> Get() {
            MockupRequest[] result = DataService.GetRequests();
            return Ok(result);
        }

        /// <summary>
        /// Gets a specific request definition.
        /// </summary>
        /// <param name="id">The id of the request definition.</param>
        /// <returns>The mock-up request with the given id.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /request/5
        /// </remarks>
        /// <response code="200">All OK.</response>
        /// <response code="404">A request with the given id was not found.</response>
        [HttpGet]
        [Route("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MockupRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MockupRequest>> Get(int id) {
            MockupRequest result;
            try {
                result = DataService.GetRequestById(id);
            }
            catch (WebApiMockException ex) {
                if (ex.ErrorCode == 11) {
                    result = null;
                }
                else {
                    throw;
                }
            }
            if (result == null) { return NotFound($"No request found with id #{id}."); }
            return Ok(result);
        }

        /// <summary>
        /// Adds a new request definition.
        /// </summary>
        /// <param name="request">The mock-up request to add.</param>
        /// <returns>The added mock-up request with an updated Id value.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// PUT /request
        /// {
        ///   "Id": 0,
        ///   "Route": "myroute/subresource",
        ///   "HttpMethod": "POST",
        ///   "ResponseId": 5
        /// }
        /// </remarks>
        /// <response code="200">All OK.</response>
        /// <response code="400">A request with the given values already exists.</response>
        /// <response code="409">The id is not 0 (zero).</response>
        [HttpPut]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MockupRequest), StatusCodes.Status200OK)]
        public async Task<ActionResult<MockupRequest>> Put([FromBody]MockupRequest request) {
            var logId = Guid.NewGuid();
            Logger.Info($"[{logId}] Executing RequestController.Put({request})");
            if (request.Id > 0) {
                Logger.Error($"[{logId}] The new request definition does not have an empty id.");
                return StatusCode(409, "The new request definition does not have an empty id."); }
            if (DataService.RequestExists(request.HttpMethod, request.Route, request.Query, request.Body, logId)) {
                Logger.Error($"[{logId}] A request with the given values already exists.");
                return BadRequest("A request with the given values already exists."); }
            if(request.Body == null) { request.Body = ""; }
            if(request.Query == null) { request.Query = ""; }
            var retVal = DataService.AddRequest(request, logId);
            Logger.Info($"[{logId}] Successfully created request with id #{retVal.Id}.");
            return Ok(retVal);
        }

        /// <summary>
        /// Updates the values of an existing mock-up request.
        /// </summary>
        /// <param name="id">The id of the mock-up request that should be updated.</param>
        /// <param name="request">A mock-up request with the new values.</param>
        /// <returns>The updated mock-up request.</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// PATCH /request/18
        /// {
        ///   "Id": 18,
        ///   "Route": "mynewroute",
        ///   "HttpMethod": "POST",
        ///   "ResponseId": 5
        /// }
        /// </remarks>
        /// <response code="200">All OK.</response>
        /// <response code="404">No request with the given id found.</response>
        /// <response code="406">The ids (query, body) mismatch.</response>
        /// <response code="409">The id is 0 (null).</response>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [ProducesResponseType(typeof(MockupRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<ActionResult<MockupRequest>> Patch(int id, [FromBody] MockupRequest request) {
            if (request.Id < 1) {
                return StatusCode(409, "The request definition has an empty id."); }
            if (!request.Id.Equals(id)) {
                return StatusCode(406, "Ids mismatch."); }
            if (!DataService.RequestExistsForId(request.Id)) {
                return NotFound($"No request definition found with id #{id}."); }
            var existingRequest = DataService.GetRequestById(id);
            if (!existingRequest.HttpMethod.Equals(request.HttpMethod, StringComparison.InvariantCultureIgnoreCase)) {
                DataService.SetRequestMethod(id, request.HttpMethod);
            }
            if (!existingRequest.Query.Equals(request.Query, StringComparison.InvariantCultureIgnoreCase)) {
                DataService.SetRequestQuery(id, request.Query);
            }
            if (!existingRequest.Body.Equals(request.Body, StringComparison.InvariantCultureIgnoreCase)) {
                DataService.SetRequestBody(id, request.Body);
            }
            if (existingRequest.ResponseId != request.ResponseId) {
                DataService.SetRequestResponseId(id, request.ResponseId);
            }
            return Ok(request);
        }

        /// <summary>
        /// Removes a mock-up request from the database.
        /// </summary>
        /// <param name="id">The id of the mock-up request that should be removed.</param>
        /// <returns>Nothing</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// DELETE /request/18
        /// </remarks>
        /// <response code="200">All OK.</response>
        /// <response code="400">The id is 0 (zero).</response>
        /// <response code="404">No request with the given id found.</response>
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id) {
            if(id == 0) {
                return BadRequest("The id is 0."); }
            if(!DataService.RequestExistsForId(id)) {
                return NotFound($"No request with id #{id} found."); }
            DataService.RemoveRequest(id);
            return Ok();
        }

        /// <summary>
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));

    }
}
