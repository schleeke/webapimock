using Microsoft.AspNetCore.Http;
using System;
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
            var transactionId = Guid.NewGuid();

            Program.Logger.Debug($"[{transactionId}] Incoming request: {context.Request.Path}.");
            if(!context.Request.IsMockupRequest()) {
                await _next(context);
                Program.Logger.Debug($"[{transactionId}] No mock-up because path does not start with '{Program.MockupRelativePath}'.");
                return; }

            var (Path, Query, Body, Method) = context.GetRequestInfo();
            path = Path;
            query = Query;
            body = Body;
            method = Method;
            if (!DataService.RequestExists(method.ToMethodString(), path, query, body, transactionId)) {
                Program.Logger.Debug($"[{transactionId}] No request '{path}' [{context.Request.Method}] exists.");
                if(!Program.AutoGenerate) {
                    Program.Logger.Debug($"[{transactionId}] Auto generation of responses for unknown requests ist turned off.");
                    context.Response.StatusCode = 501;
                    await context.Response.WriteAsync("Not implemented");
                    await context.Response.CompleteAsync();
                    return; }
                Program.Logger.Debug($"[{transactionId}] Generating automatic response for unknown request.");
                CreateAutoResponse(method.ToMethodString(), path, query, body, transactionId);
                Program.Logger.Info($"[{transactionId}] Automatically created response for unknown request."); }
            try {
                requestInfo = DataService.GetRequest(method.ToMethodString(), path, query, body, transactionId); }
            catch (WebApiMockException ex) {
                if(ex.ErrorCode == 13) {
                    Program.Logger.Error($"[{transactionId}] Unable to find request [{method.ToMethodString()}] for '{path}' in database."); }
                Program.Logger.Error($"[{transactionId}] Unknown error while getting response by id: {ex.GetFullMessage()}");
                throw; }
            try {
                responseInfo = DataService.GetResponseById(requestInfo.ResponseId, transactionId); }
            catch (WebApiMockException ex) {
                if(ex.ErrorCode == 11) {
                    Program.Logger.Error($"[{transactionId}] Unable to find response for id #{requestInfo.ResponseId}."); }
                Program.Logger.Error($"[{transactionId}] Unknown error while getting response by id: {ex.GetFullMessage()}");
                throw; }
            context.Response.StatusCode = responseInfo.StatusCode;
            if (string.IsNullOrEmpty(responseInfo.Response)) {
                await context.Response.CompleteAsync();
                Program.Logger.Info($"[{transactionId}] Successfully mocked request.");
                return; }
            if(string.IsNullOrEmpty(responseInfo.MimeType)) {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("MIME type for response not set.");
                await context.Response.CompleteAsync();
                Program.Logger.Info($"[{transactionId}] Successfully mocked request.");
                return; }
            context.Response.ContentType = responseInfo.MimeType;
            await context.Response.WriteAsync(responseInfo.Response);
            await context.Response.CompleteAsync();
            Program.Logger.Info($"[{transactionId}] Successfully mocked request.");
        }
    
    
    
        private void CreateAutoResponse(string httpMethod, string route, string query, string body, Guid transactionId) {
            var responseExists = DataService.ResponseExists(200,transactionId: transactionId);
            if(!responseExists) {
                DataService.AddResponse(new MockupResponse {
                    Id = 0,
                    StatusCode = 200,
                    MimeType = "",
                    Response = "" }, transactionId); }
            var response = DataService.GetResponse(200, transactionId: transactionId);
            DataService.AddRequest(new MockupRequest {
                Id = 0,
                Body = body,
                HttpMethod = httpMethod,
                Query = query,
                ResponseId = response.Id,
                Route = route }, transactionId);
        }
    }
}
