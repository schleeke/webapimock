using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiMock.Data;

namespace WebApiMock.Controllers {

#pragma warning disable 1998
    /// <inheritdoc/>
    [Route("[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase {

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
            var transactionId = Guid.NewGuid();
            System.IO.DirectoryInfo directoryInfo;
            System.IO.DirectoryInfo[] subDirectories;

            Program.Logger.Info($"[{transactionId}] Executing ToolsController.Import(\"{path}\")");
            directoryInfo = new System.IO.DirectoryInfo(path);
            if (!directoryInfo.Exists) {
                Program.Logger.Warn($"[{transactionId}] Canceling import because the path was not found.");
                return NotFound(); }
            subDirectories = directoryInfo.GetDirectories();
            Program.Logger.Debug($"[{transactionId}] Processing {subDirectories.Length} directories...");
            foreach (var subDir in subDirectories) {
                Program.Logger.Debug($"[{transactionId}] Processing {subDir.Name}...");
                ImportDirectory(subDir, false, transactionId);
            }
            Program.Logger.Info($"[{transactionId}] Successfully imported legacy path.");
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
            var transactionId = Guid.NewGuid();
            Program.Logger.Info($"[{transactionId}] Executing ToolsController.Delete()");
            var fileName = System.IO.Path.Combine(Program.ApplicationDirectory.FullName, "mock-data.db");
            if(!System.IO.File.Exists(fileName)) {
                Program.Logger.Debug($"[{transactionId}] Database file does not exists. Skipping file deletion.");
                Program.Logger.Info($"[{transactionId}] Successfully removed the existing database.");
                return Ok(); }
            try {
                System.IO.File.Delete(fileName); }
            catch (System.IO.IOException ex) {
                Program.Logger.Error($"[{transactionId}] An error occured while deleting the database file: {ex.GetFullMessage()}");
                return StatusCode(500, $"Unable to delete the database: {ex.Message}"); }
            Program.Logger.Info($"[{transactionId}] Successfully removed the existing database.");
            return Ok();
        }

        /// <summary>
        /// Sets all request definitions for a given route to a certain HTTP method.
        /// </summary>
        /// <param name="route">The route to change the methods for.</param>
        /// <param name="httpMethod">The new HTTP method.</param>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /tools/setmethod?route=myroute&amp;httpmethod=GET
        /// </remarks>
        /// <returns>Nothing</returns>
        /// <response code="200">OK</response>
        /// <response code="500">An error occured while execution.</response>
        [HttpGet]
        [Route("setmethod")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SetMethod(string route, string httpMethod) {
            try {
                DataService.SetMethodForRoute(route, httpMethod); }
            catch (WebApiMockException ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.GetFullMessage()); }
            return Ok();
        }

        /// <summary>
        /// Gets the prefix (relative URL) for the mock-up responses.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /tools/mockupprefix
        /// </remarks>
        /// <returns>The prefix for the mock-up requests.</returns>
        /// <response code="200">OK</response>
        [HttpGet]
        [Route("mockupprefix")]
        [Produces("text/plain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetMockupPrefix() {
            var prefix = Program.MockupRelativePath;
            return Ok(prefix);
        }

    
        private void ImportDirectory(System.IO.DirectoryInfo dir, bool isQueryDirectory, Guid transactionId) {
            var comp = StringComparison.InvariantCultureIgnoreCase;
            var mimeType = "application/json";
            var query = string.Empty;
            var body = string.Empty;
            var route = dir.Name;
            int statusCode;
            string response;
            bool responseExists;
            bool requestExists;
            int responseId;
            int requestId;

            if(dir.Name.Equals("src", comp)) {
                Program.Logger.Warn($"[{transactionId}] Skipping directory.");
                return; }

            Program.Logger.Debug($"[{transactionId}] Processing {(isQueryDirectory ? "query" : "route")} directory.");
            if(isQueryDirectory) {
                route = dir.Parent.Name;
                query = HttpUtility.UrlDecode(dir.Name); }
            statusCode = dir.GetMockupStatusCode();
            response = dir.GetMockupResponse();
            responseExists = DataService.ResponseExists(statusCode, response, mimeType, transactionId);
            requestExists = DataService.RequestExists("GET", route, query, body, transactionId);
            if(requestExists && responseExists) {
                Program.Logger.Info($"[{transactionId}] Response and request already exist for current directory.");
                return; }
            if(!responseExists) {
                responseId = DataService.AddResponse(new MockupResponse {
                    Id = 0,
                    MimeType = mimeType,
                    Response = response,
                    StatusCode = statusCode                    
                }, transactionId).Id; }
            else {
                responseId = DataService.GetResponse(statusCode, response, mimeType, transactionId).Id; }
            if(!requestExists) {
                requestId = DataService.AddRequest(new MockupRequest {
                    Id = 0,
                    Body = body,
                    HttpMethod = "GET",
                    Query = query,
                    Route = route,
                    ResponseId = responseId
                }, transactionId).Id; }
            else {
                requestId = DataService.GetRequest("GET", route, query, body, transactionId).Id; }
            Program.Logger.Info($"[{transactionId}] Successfully created request/response pair. IDs: {requestId}/{responseId}.");
            if(!isQueryDirectory) {
                var subDirs = dir.GetDirectories();
                Program.Logger.Debug($"[{transactionId}] Processing query directories ({subDirs.Length})...");
                foreach (var subDir in subDirs) {
                    ImportDirectory(subDir, true, transactionId); }
            }
        }
    }
}