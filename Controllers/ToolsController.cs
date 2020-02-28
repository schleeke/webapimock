using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMock.Data;

namespace WebApiMock.Controllers {

    /// <inheritdoc/>
    [Route("[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {

        private readonly DataService _data;

        /// <inheritdoc/>
        public ToolsController(DataService data) => _data = data;


        /// <summary>
        /// Imports the definitions from the old mock-up service.
        /// </summary>
        /// <param name="path">The full path to the application directory of the old service.</param>
        /// <returns>Nothing/OK</returns>
        /// <response code="200">All OK.</response>
        /// <response code="404">The directory was not found.</response>
        [HttpGet]
        [Route("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Import(string path) {
            var directoryInfo = new System.IO.DirectoryInfo(path);
            if (!directoryInfo.Exists) { return NotFound(); }
            Logger.Info($"Importing legacy requests from directory '{path}'...");
            foreach (var dir in directoryInfo.GetDirectories()) {
                if(dir.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase)) { continue; }
                ProcessMockupDirectory(dir);
            }
            return Ok(path);
        }

        /// <summary>
        /// Deletes the database.
        /// </summary>
        /// <returns>Nothing</returns>
        /// <response code="200">OK. Data successfully deleted (or no data present).</response>
        /// <response code="500">An error occured while deleting the database file.</response>
        [HttpDelete]
        [Route("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete() {
            var fileName = System.IO.Path.Combine(Program.ApplicationDirectory.FullName, "mock-data.db");
            if(!System.IO.File.Exists(fileName)) { return Ok(); }
            try {
                System.IO.File.Delete(fileName); }
            catch (System.IO.IOException ex) {
                Logger.Error($"An error occured while deleting the database file: {ex.GetFullMessage()}");
                return StatusCode(500, $"Unable to delete the database: {ex.Message}"); }
            Logger.Info("Successfully removed the existing database.");
            return Ok();
        }
    
        private void ProcessMockupDirectory(System.IO.DirectoryInfo dir, bool isQueryDirectory = false) {
            Logger.Debug($"Processing {(isQueryDirectory ? "(sub-)" : "")}directory '{dir.Name}'...");
            var route = isQueryDirectory ? dir.Parent.Name : dir.Name;
            var query = isQueryDirectory ? dir.Name : "";
            int statusCode;
            string response;
            var hasResponse = dir.GetFiles("response.json").Any();
            var hasStatusCode = dir.GetFiles("*.statuscode").Any();

            response = dir.GetMockupResponse();
            statusCode = dir.GetMockupStatusCode();
            if(!string.IsNullOrEmpty(query) && isQueryDirectory) {
                query = HttpUtility.UrlDecode(query); }
            if (hasResponse || hasStatusCode) {
                CreateMockupResponse(route, "GET", statusCode, response, query); }
            if (isQueryDirectory) {
                return; }
            foreach (var subDir in dir.GetDirectories()) {
                ProcessMockupDirectory(subDir, true); }

        }

        private void CreateMockupResponse(string route, string method, int statusCode, string response, string query="") {
            Logger.Debug($"Trying to create response for route '{route}' [{method}] and HTTP status code #{statusCode}{(string.IsNullOrEmpty(query) ? "." : " and query '" + query + "}'.")}");
            var requestExists = _data.RequestExists(method, route);
            var responseExists = _data.ResponseExists(statusCode, response, "application/json");
            int responseId;

            if(requestExists) {
                Logger.Warn($"A request for route '{route}' [{method}] already exists. Skipping import of current directory.");
                return; }
            if(responseExists) {
                Logger.Debug("A response already exists. Fetiching its information.");
                responseId = _data.GetResponse(statusCode, response, "application/json").Id; }
            else {
                Logger.Debug("Creating new response.");
                responseId = _data.AddResponse(new MockupResponse {
                    Id = 0,
                    StatusCode = statusCode,
                    Response = response,
                    MimeType = string.IsNullOrEmpty(response) ? "" : "application/json" }).Id; }
            Logger.Debug("Creating new request.");
            _data.AddRequest(new MockupRequest {
                HttpMethod = method,
                Route = route,
                Query = query,
                ResponseId = responseId });
        }

        /// <summary>
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));

    }
}