using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebApiMock.Data;

namespace WebApiMock.Web {
    /// <summary>
    /// Web request middleware for interception requests and mocking their response if neccessary/defined.
    /// </summary>
    public class MockupMiddleware {
        private readonly RequestDelegate _next;

        /// <inheritdoc />
        public MockupMiddleware(RequestDelegate next) => _next = next;

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context) {
            MockupRequest requestInfo;
            MockupResponse responseInfo;
            HttpMethodEnum method;
            string path;
            string query;
            string body;

            if (!context.Request.Path.HasValue
              || context.Request.Path.Value.IndexOf("/mockup", System.StringComparison.InvariantCultureIgnoreCase) != 0) {
                await _next(context);
                return; }
            var (Path, Query, Body, Method) = context.GetRequestInfo();
            path = Path;
            query = Query;
            body = Body;
            method = Method;
            if (!DataService.RequestExists(method.ToMethodString(), path, query, body)) {
                Logger.Debug($"No request '{path}' [{context.Request.Method}] exists.");
                context.Response.StatusCode = 501;
                await context.Response.WriteAsync("Not implemented");
                await context.Response.CompleteAsync();
                return; }
            try {
                requestInfo = DataService.GetRequest(method.ToMethodString(), path, query, body); }
            catch (WebApiMockException ex) {
                if(ex.ErrorCode == 13) {
                    Logger.Error($"$Unable to find request [{method.ToMethodString()}] for '{path}' in database."); }
                throw; }
            try {
                responseInfo = DataService.GetResponseById(requestInfo.ResponseId); }
            catch (WebApiMockException ex) {
                if(ex.ErrorCode == 11) {
                    Logger.Error($"Unable to find response for id #{requestInfo.ResponseId}."); }
                throw; }
            context.Response.StatusCode = responseInfo.StatusCode;
            if (string.IsNullOrEmpty(responseInfo.Response)) {
                await context.Response.CompleteAsync();
                return; }
            if(string.IsNullOrEmpty(responseInfo.MimeType)) {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("MIME type for response not set.");
                await context.Response.CompleteAsync();
                return; }
            context.Response.ContentType = responseInfo.MimeType;
            await context.Response.WriteAsync(responseInfo.Response);
            await context.Response.CompleteAsync();
        }

        /// <summary>
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));

    }
}
