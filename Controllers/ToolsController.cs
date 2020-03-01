using System;
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
    public class ToolsController : ControllerBase {
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
            var transactionId = Guid.NewGuid();
            System.IO.DirectoryInfo directoryInfo;
            System.IO.DirectoryInfo[] subDirectories;

            Logger.Info($"[{transactionId}] Executing ToolsController.Import(\"{path}\")");
            directoryInfo = new System.IO.DirectoryInfo(path);
            if (!directoryInfo.Exists) {
                Logger.Warn($"[{transactionId}] Canceling import because the path was not found.");
                return NotFound(); }
            subDirectories = directoryInfo.GetDirectories();
            Logger.Debug($"[{transactionId}] Processing {subDirectories.Length} directories...");
            foreach (var subDir in subDirectories) {
                Logger.Debug($"[{transactionId}] Processing {subDir.Name}...");
                ImportDirectory(subDir, false, transactionId);
            }
            Logger.Info($"[{transactionId}] Successfully imported legacy path.");
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
            Logger.Info($"[{transactionId}] Executing ToolsController.Delete()");
            var fileName = System.IO.Path.Combine(Program.ApplicationDirectory.FullName, "mock-data.db");
            if(!System.IO.File.Exists(fileName)) {
                Logger.Debug($"[{transactionId}] Database file does not exists. Skipping file deletion.");
                Logger.Info($"[{transactionId}] Successfully removed the existing database.");
                return Ok(); }
            try {
                System.IO.File.Delete(fileName); }
            catch (System.IO.IOException ex) {
                Logger.Error($"[{transactionId}] An error occured while deleting the database file: {ex.GetFullMessage()}");
                return StatusCode(500, $"Unable to delete the database: {ex.Message}"); }
            Logger.Info($"[{transactionId}] Successfully removed the existing database.");
            return Ok();
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
                Logger.Warn($"[{transactionId}] Skipping directory.");
                return; }

            Logger.Debug($"[{transactionId}] Processing {(isQueryDirectory ? "query" : "route")} directory.");
            if(isQueryDirectory) {
                route = dir.Parent.Name;
                query = HttpUtility.UrlDecode(dir.Name); }
            statusCode = dir.GetMockupStatusCode();
            response = dir.GetMockupResponse();
            responseExists = DataService.ResponseExists(statusCode, response, mimeType, transactionId);
            requestExists = DataService.RequestExists("GET", route, query, body, transactionId);
            if(requestExists && responseExists) {
                Logger.Info($"[{transactionId}] Response and request already exist for current directory.");
                return; }
            if(!responseExists) {
                responseId = DataService.AddResponse(new MockupResponse {
                    Id = 0,
                    MimeType = mimeType,
                    Response = response,
                    StatusCode = statusCode                    
                }, transactionId).Id; }
            else {
                responseId = _data.GetResponse(statusCode, response, mimeType, transactionId).Id; }
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
                requestId = _data.GetRequest("GET", route, query, body, transactionId).Id; }
            Logger.Info($"[{transactionId}] Successfully created request/response pair. IDs: {requestId}/{responseId}.");
            if(!isQueryDirectory) {
                var subDirs = dir.GetDirectories();
                Logger.Debug($"[{transactionId}] Processing query directories ({subDirs.Length})...");
                foreach (var subDir in subDirs) {
                    ImportDirectory(subDir, true, transactionId); }
            }
        }
   

        /// <summary>
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));

    }
}