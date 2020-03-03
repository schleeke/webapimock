using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApiMock.Data {

    /// <summary>
    /// Enumeration of all mockable HTTP methods.
    /// </summary>
    public enum HttpMethodEnum {
        /// <summary>
        /// Unknown HTTP method.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// GET HTTP method.
        /// </summary>
        GET,

        /// <summary>
        /// POST HTTP method.
        /// </summary>
        POST,

        /// <summary>
        /// PUT HTTP method.
        /// </summary>
        PUT,

        /// <summary>
        /// DELETE HTTP method.
        /// </summary>
        DELETE,

        /// <summary>
        /// PATH HTTP method.
        /// </summary>
        PATCH
    }

    /// <summary>
    /// Service / repository for the mockup data access.
    /// </summary>
    public static class DataService {

        #region "Response"

        /// <summary>
        /// Checks if a response with the give record id exists.
        /// </summary>
        /// <param name="id">The id of the response record.</param>
        /// <returns>True if a response record with the given id exists.</returns>
        public static bool ResponseExistsForId(int id) {
            using var ctx = new DataContext();
            return ctx.Responses.Any(r => r.Id.Equals(id));
        }

        /// <summary>
        /// Checks if a given respond exists.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="response">An (optional) return value for the response.</param>
        /// <param name="mimeType">The MIME type of the (optional) return value for the response.</param>
        /// <param name="transactionId">The identifier for the current transaction. For logging purpose.</param>
        /// <returns>True, if a response with the given values was found.</returns>
        /// <remarks>
        /// If an empty response is passed, the records in the database will be checked for NULL values or empty strings.
        /// The method will also skip MIME type checking if no response value was passed.
        /// </remarks>
        public static bool ResponseExists(int statusCode, string response="", string mimeType="", Guid? transactionId = null) {
            List<ResponseDefinition> tmpResponses;
            var logId = Guid.Empty;
            var executionId = Guid.NewGuid();
            var comp = StringComparison.InvariantCultureIgnoreCase;

            if (transactionId.HasValue) { logId = transactionId.Value; }
            if (logId.Equals(Guid.Empty)) { logId = Guid.NewGuid(); }
            if (string.IsNullOrEmpty(response)) {
                Program.Logger.Debug($"[{logId}] Checking if response for HTTP {statusCode} exists (NO RESPONSE)."); }
            else {
                Program.Logger.Debug($"[{logId}] Checking if response for HTTP {statusCode} and response for MIME type '{mimeType}' exists."); }
            using var ctx = new DataContext();
            tmpResponses = ctx.Responses.Where(r => r.StatusCode == statusCode).ToList();
            if(tmpResponses.Count < 1) {
                Program.Logger.Debug($"[{logId}] No response found.");
                return false; }
            if (string.IsNullOrEmpty(response)) {
                tmpResponses = tmpResponses.Where(r => r.Response == null || string.IsNullOrEmpty(r.Response)).ToList(); }
            else {
                tmpResponses = tmpResponses.Where(r => r.Response.Equals(response, comp)).ToList(); }
            if (tmpResponses.Count < 1) {
                Program.Logger.Debug($"[{logId}] No response found.");
                return false; }
            if(string.IsNullOrEmpty(mimeType)) {
                tmpResponses = tmpResponses.Where(r => r.MimeType == null || string.IsNullOrEmpty(r.MimeType)).ToList(); }
            else {
                tmpResponses = tmpResponses.Where(r => r.MimeType.Equals(mimeType, comp)).ToList(); }
            if (tmpResponses.Count < 1) {
                Program.Logger.Debug($"[{logId}] No response found.");
                return false; }
            Program.Logger.Debug($"[{logId}] Found existing response.");
            return true;
        }

        /// <summary>
        /// Gets all defined mockup responses.
        /// </summary>
        /// <returns>An array with all defined mockup responses.</returns>
        public static MockupResponse[] GetResponses() {
            using var ctx = new DataContext();
            return ctx.Responses.Select(r => r.ToMockupResponse()).ToArray();
        }

        /// <summary>
        /// Gets/returns a mockup response with a given record id.
        /// </summary>
        /// <param name="id">The id of the mockup response record.</param>
        /// <returns>The response record with the mockup values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #11 if no response record with the given id was found.</exception>
        public static MockupResponse GetResponseById(int id, Guid? transactionId = null) {
            var logId = Guid.NewGuid();
            
            if(transactionId.HasValue) { logId = transactionId.Value; }
            if(!ResponseExistsForId(id)) {
                Program.Logger.Error($"[{logId}] Unable to find a response definition for the id #{id}.");
                throw new WebApiMockException($"Unable to find a response definition for the id #{id}.", 11); }
            using var ctx = new DataContext();
            return ctx.Responses
                .Single(r => r.Id.Equals(id))
                .ToMockupResponse();
        }

        /// <summary>
        /// Returns all responses for a given HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>An array with all response records for the given HTTP status code.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #1 if no responsefor the given HTTP status code was found.</exception>
        public static MockupResponse[] GetAllResponsesForStatusCode(int statusCode) {
            //if(!ResponseExists(statusCode)) { throw new WebApiMockException($"Unable to find a response for the status code #{statusCode}.", 1); }
            using var ctx = new DataContext();
            return ctx.Responses
                .Where(r => r.StatusCode.Equals(statusCode))
                .Select(r => r.ToMockupResponse())
                .ToArray();
        }

        /// <summary>
        /// Returns the mockup response record with the given values.
        /// </summary>
        /// <param name="statusCode">The HTTP status core of the response.</param>
        /// <param name="response">The (optional) return value of the response.</param>
        /// <param name="mimeType">The MIME type for the (optional) return value.</param>
        /// <param name="transactionId">The identifier for the current transaction. For logging purpose.</param>
        /// <returns>The mockup response record for the given values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #2 if no matching response was found.</exception>
        public static MockupResponse GetResponse(int statusCode, string response="", string mimeType="", Guid? transactionId = null) {
            var logId = Guid.Empty;

            if (transactionId.HasValue) { logId = transactionId.Value; }
            if (logId.Equals(Guid.Empty)) { logId = Guid.NewGuid(); }
            if (string.IsNullOrEmpty(response)) {
                Program.Logger.Debug($"[{logId}] Reading response for HTTP status {statusCode}."); }
            else {
                Program.Logger.Debug($"[{logId}] Reading response for HTTP status {statusCode} and response for MIME type '{mimeType}'."); }
            if (!ResponseExists(statusCode, response, mimeType, logId)) {
                Program.Logger.Error($"[{logId}] Unable to find a response for the status code #{statusCode}.");
                throw new WebApiMockException($"Unable to find a response for the status code #{statusCode}.", 2); }
            var comp = StringComparison.InvariantCultureIgnoreCase;
            List<ResponseDefinition> tmpList;
            using var ctx = new DataContext();
            tmpList = ctx.Responses.Where(r => r.StatusCode == statusCode).ToList();
            if (string.IsNullOrEmpty(response)) {
                tmpList = tmpList.Where(r => r.Response == null || string.IsNullOrEmpty(r.Response)).ToList();
                Program.Logger.Debug($"[{logId}] Returning response with no response and HTTP {statusCode} only.");
                return tmpList.Single().ToMockupResponse(); }
            tmpList = tmpList.Where(r => r.Response.Equals(response, comp)).ToList();
            tmpList = tmpList.Where(r => r.MimeType.Equals(mimeType, comp)).ToList();
            Program.Logger.Debug($"[{logId}] Returning response for HTTP status {statusCode} and response with MIME type '{mimeType}'.");
            return tmpList.Single().ToMockupResponse();
        }

        /// <summary>
        /// Adds a new mockup response to the database.
        /// </summary>
        /// <param name="response">The new response.</param>
        /// <param name="transactionId">The identifier for the current transaction. For logging purpose.</param>
        /// <returns>The new mockup response with the matching id.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #3 if the id of the given response is not 0 (zero).</exception>
        /// <exception cref="WebApiMockException">Thrown with error code #4 if a response with the same values already exists.</exception>
        public static MockupResponse AddResponse(MockupResponse response, Guid? transactionId = null) {
            var logId = Guid.Empty;

            if (transactionId.HasValue) { logId = transactionId.Value; }
            if (logId.Equals(Guid.Empty)) { logId = Guid.NewGuid(); }
            if (response.Id != 0) {
                Program.Logger.Error($"[{logId}] The id for a new response must be 0.");
                throw new WebApiMockException("The id for a new response must be 0.", 3); }
            if(ResponseExists(response.StatusCode, response.Response, response.MimeType, logId)) {
                Program.Logger.Error($"[{logId}] The response already exists.");
                throw new WebApiMockException("The response already exists.", 4); }
            var newData = new ResponseDefinition {
                StatusCode = response.StatusCode,
                Response = response.Response,
                MimeType = response.MimeType };
            using var ctx = new DataContext();
            ctx.Responses.Add(newData);
            ctx.SaveChanges();
            Program.Logger.Info($"[{logId}] Successfully created response (HTTP {response.StatusCode}) with id #{newData.Id}.");
            return newData.ToMockupResponse();
        }

        /// <summary>
        /// Sets the value of an existing mockup response record HTTP status code to a new one.
        /// </summary>
        /// <param name="id">The id of the mockup response record.</param>
        /// <param name="statusCode">The value of the new HTTP status code.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #5 if no mockup response record with the given id exists.</exception>
        public static void SetResponseStatusCode(int id, int statusCode) {
            if (!ResponseExistsForId(id)) { throw new WebApiMockException($"No response exists with the id #{id}.", 5); }
            using var ctx = new DataContext();
            var existingItem = ctx.Responses.Single(r => r.Id.Equals(id));
            if (existingItem.StatusCode == statusCode) { return; }
            existingItem.StatusCode = statusCode;
            ctx.Responses.Update(existingItem);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Sets the return value of an existing mockup response record.
        /// </summary>
        /// <param name="id">The id of the existing mockup response record.</param>
        /// <param name="response">The new value for the response's return value.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #5 if no mockup response record with the given id exists.</exception>
        public static void SetResponseResponse(int id, string response) {
            if (!ResponseExistsForId(id)) { throw new WebApiMockException($"No response exists with the id #{id}.", 5); }
            using var ctx = new DataContext();
            var existingItem = ctx.Responses.Single(r => r.Id.Equals(id));
            if (existingItem.Response.Equals(response, StringComparison.InvariantCultureIgnoreCase)) { return; }
            existingItem.Response = response;
            if(string.IsNullOrEmpty(response)) {
                existingItem.Response = null;
                existingItem.MimeType = null; }
            ctx.Responses.Update(existingItem);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Sets the value for the MIME type of an existing mockup response record.
        /// </summary>
        /// <param name="id">The id of the existing mockup response record.</param>
        /// <param name="mimeType">The new MIME type value corresponding the the record's response return value.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #5 if no mockup response record with the given id exists.</exception>
        public static void SetResponseMimeType(int id, string mimeType) {
            if (!ResponseExistsForId(id)) { throw new WebApiMockException($"No response exists with the id #{id}.", 5); }
            using var ctx = new DataContext();
            var existingItem = ctx.Responses.Single(r => r.Id.Equals(id));
            if (existingItem.MimeType.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase)) { return; }
            existingItem.MimeType = mimeType;
            ctx.Responses.Update(existingItem);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Removes an existing mockup response record from the database.
        /// </summary>
        /// <param name="id">The id of the existing mockup response record.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #5 if no mockup response record with the given id exists.</exception>
        public static void RemoveResponse(int id) {
            if (!ResponseExistsForId(id)) { throw new WebApiMockException($"No response exists with the id #{id}.", 5); }
            using var ctx = new DataContext();
            var item = ctx.Responses.Single(r => r.Id.Equals(id));
            ctx.Responses.Remove(item);
            ctx.SaveChanges();
        }

        #endregion

        #region "Request"

        /// <summary>
        /// Checks if a request with the given id exists.
        /// </summary>
        /// <param name="id">The id to check.</param>
        /// <returns>True, if a record with the id was found.</returns>
        public static bool RequestExistsForId(int id) {
            using var ctx = new DataContext();
            return ctx.Requests.Any(r => r.Id.Equals(id));
        }

        /// <summary>
        /// Checks if a request with the given values exists.
        /// </summary>
        /// <param name="httpMethod">The HTTP method for the request.</param>
        /// <param name="route">The route/relative URI for the request.</param>
        /// <param name="query">The query part of the URI</param>
        /// <param name="body">The body for the request.</param>
        /// <param name="transactionId">An optional id for logging purpose.</param>
        /// <returns>True if a request with the given values exists.</returns>
        public static bool RequestExists(string httpMethod, string route, string query="", string body="", Guid? transactionId = null) {
            var logId = Guid.NewGuid();
            var method = httpMethod.ToMethodEnum();
            var comp = StringComparison.InvariantCultureIgnoreCase;
            List<RequestDefinition> tmpResult;

            if (route.StartsWith("/")) { route = route.Substring(1); }
            if (transactionId.HasValue) { logId = transactionId.Value; }
            Program.Logger.Debug($"[TRACE] {nameof(DataService)}.{nameof(RequestExists)}(httpMethod: {httpMethod}, route: {route}, query: {query}, body: {body}).");
            if(method == HttpMethodEnum.Unknown) {
                Program.Logger.Error($"[{logId}] Unknown HTTP method '{httpMethod.ToUpper()}'.");
                throw new WebApiMockException($"Unknown HTTP method '{httpMethod}'.", 10); }
            using var ctx = new DataContext();
            tmpResult = ctx.Requests.Where(r => r.Method == method).ToList();
            if(tmpResult.Count() < 1) {
                Program.Logger.Debug($"[{logId}] No existing request(s) found (no method match).");
                return false; }
            tmpResult = tmpResult.Where(r => r.Route.Equals(route, comp)).ToList();
            if (tmpResult.Count() < 1) {
                Program.Logger.Debug($"[{logId}] No existing request(s) found (no route match).");
                return false; }
            if(string.IsNullOrEmpty(query)) {
                tmpResult = tmpResult.Where(r => r.Query == null || string.IsNullOrEmpty(r.Query)).ToList(); }
            else {
                tmpResult = tmpResult.Where(r => r.Query.Equals(query, comp)).ToList(); }
            if (tmpResult.Count() < 1) {
                Program.Logger.Debug($"[{logId}] No existing request(s) found (no query match).");
                return false; }
            if(string.IsNullOrEmpty(body)) {
                tmpResult = tmpResult.Where(r => r.Body == null || string.IsNullOrEmpty(r.Body)).ToList(); }
            else {
                tmpResult = tmpResult.Where(r => r.Body.Equals(body, comp)).ToList(); }
            Program.Logger.Debug(tmpResult.Count > 0 ? $"[{logId}] Found existing request(s)." : $"[{logId}] No existing request(s) found (no body match).");
            return (tmpResult.Count() > 0);
        }

        /// <summary>
        /// Returns a request for a given id.
        /// </summary>
        /// <param name="id">The id for the request.</param>
        /// <returns>The record with the given id.</returns>
        /// <exception cref="WebApiMockException">Thrown with id #11 if no request with the given id exists.</exception>
        public static MockupRequest GetRequestById(int id) {
            using var ctx = new DataContext();
            if (!ctx.Requests.Any(req => req.Id.Equals(id))) { throw new WebApiMockException($"No request with id #{id} found.", 11); }
            return ctx.Requests.Single(req => req.Id.Equals(id)).ToMockupRequest();
        }

        /// <summary>
        /// Gets the mockup request record with the given values.
        /// </summary>
        /// <param name="httpMethod">The HTTP method for the request.</param>
        /// <param name="route">The route/relative URL path for the request.</param>
        /// <param name="query">The (optional) query parameters for the request.</param>
        /// <param name="body">The (optional) body for the request.</param>
        /// <param name="transactionId">The identifier for the current transaction. For logging purpose.</param>
        /// <returns>The mockup request record with the given values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #10 if the HTTP method is unknown.</exception>
        /// <exception cref="WebApiMockException">Thrown with error code #13 if no request with the given values was found.</exception>
        public static MockupRequest GetRequest(string httpMethod, string route, string query="", string body="", Guid? transactionId = null) {
            var logId = Guid.Empty;
            var method = httpMethod.ToMethodEnum();
            var comp = StringComparison.InvariantCultureIgnoreCase;
            List<RequestDefinition> tmpList;

            if (transactionId.HasValue) { logId = transactionId.Value; }
            if (logId.Equals(Guid.Empty)) { logId = Guid.NewGuid(); }
            if (method == HttpMethodEnum.Unknown) {
                Program.Logger.Error($"[{logId}] Unknown HTTP method '{httpMethod.ToUpper()}'.");
                throw new WebApiMockException($"Unknown HTTP method '{httpMethod}'.", 10); }
            if (route.StartsWith("/")) {
                route = route.Substring(1); }
            using var ctx = new DataContext();
            tmpList = ctx.Requests.Where(req => req.Method == method).ToList();
            if(tmpList.Count < 1) {
                Program.Logger.Error($"[{logId}] No request found.");
                throw new WebApiMockException("No request found.", 13); }
            tmpList = tmpList.Where(req => req.Route.Equals(route, comp)).ToList();
            if (tmpList.Count < 1) {
                Program.Logger.Error($"[{logId}] No request found.");
                throw new WebApiMockException("No request found.", 13); }
            if (string.IsNullOrEmpty(query)) {
                tmpList = tmpList.Where(r => r.Query == null || string.IsNullOrEmpty(r.Query)).ToList(); }
            else {
                tmpList = tmpList.Where(req => req.Query.Equals(query, comp)).ToList(); }
            if (tmpList.Count < 1) {
                Program.Logger.Error($"[{logId}] No request found.");
                throw new WebApiMockException("No request found.", 13); }
            if(string.IsNullOrEmpty(body)) {
                tmpList = tmpList.Where(r => r.Body == null || string.IsNullOrEmpty(r.Body)).ToList(); }
            else {
                tmpList = tmpList.Where(req => req.Body.Equals(body, comp)).ToList(); }
            if (tmpList.Count < 1) {
                Program.Logger.Error($"[{logId}] No request found.");
                throw new WebApiMockException("No request found.", 13); }
            return tmpList.Single().ToMockupRequest();
        }

        /// <summary>
        /// Adds a new request to the database.
        /// </summary>
        /// <param name="request">The request to add. Its id must be 0 (zero).</param>
        /// <param name="transactionId">An (optional) id for logging purpose.</param>
        /// <returns>The added request with an updated id.</returns>
        public static MockupRequest AddRequest(MockupRequest request, Guid? transactionId = null) {
            var logId = Guid.Empty;
            var method = request.HttpMethod.ToMethodEnum();
            bool exists;
            RequestDefinition newData;

            if (transactionId.HasValue) { logId = transactionId.Value; }
            if (logId.Equals(Guid.Empty)) { logId = Guid.NewGuid(); }
            if(method == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"Unknown HTTP method '{request.HttpMethod}'.", 16); }
            if(request.ResponseId == 0) {
                throw new WebApiMockException("No response set for the new request.", 17); }
            exists = ResponseExistsForId(request.ResponseId);
            if(!exists) {
                throw new WebApiMockException($"No response with id #{request.ResponseId} found.", 18); }
            if (request.Id != 0) {
                throw new WebApiMockException("The id for a new request must be 0.", 14); }
            if (RequestExists(request.HttpMethod, request.Route, request.Query, request.Body, logId)) {
                throw new WebApiMockException("The request already exists.", 15); }
            newData = new RequestDefinition {
                Method = request.HttpMethod.ToMethodEnum(),
                Body = request.Body,
                Query = request.Query,
                Route = request.Route,
                MockupResponseId = request.ResponseId };
            using var ctx = new DataContext();
            ctx.Requests.Add(newData);
            ctx.SaveChanges();
            Program.Logger.Info($"[{logId}] Successfully created request #{newData.Id} ({request.Route}) [{request.HttpMethod}].");
            return newData.ToMockupRequest();
        }

        /// <summary>
        /// Removes a request by its id.
        /// </summary>
        /// <param name="id">The id of the request record that shall be removed.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #19 if no record with the given id was found.</exception>
        public static void RemoveRequest(int id) {
            if (!RequestExistsForId(id)) {
                throw new WebApiMockException($"No request with id #{id} found.", 19); }
            using var ctx = new DataContext();
            var existingItem = ctx.Requests.Single(r => r.Id == id);
            var responseUseCount = ctx.Requests
                .Where(r => r.MockupResponseId == existingItem.MockupResponseId)
                .Count();
            if (responseUseCount == 1) {
                var responseToRemove = ctx.Responses.Single(r => r.Id == existingItem.MockupResponseId);
                ctx.Responses.Remove(responseToRemove); }
            ctx.Requests.Remove(existingItem);
            ctx.SaveChanges();
        }
        
        /// <summary>
        /// Sets the reponse id for a given request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="responseId">The new id of the response.</param>
        public static void SetRequestResponseId(int id, int responseId) {
            if (!RequestExistsForId(id)) { throw new WebApiMockException($"No request with id #{id} found.", 22); }
            if (!ResponseExistsForId(responseId)) { throw new WebApiMockException($"No response with id #{id} found.", 23); }
            using var ctx = new DataContext();
            var item = ctx.Requests.Single(r => r.Id == id);
            if(item.MockupResponseId == responseId) { return; }
            item.MockupResponseId = responseId;
            ctx.Requests.Update(item);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Returns all existing mockup request records.
        /// </summary>
        /// <returns>An array with all request definitions within the database.</returns>
        public static MockupRequest[] GetRequests() {
            MockupRequest[] result;
            using (var ctx = new DataContext()) { result = ctx.Requests.Select(r => r.ToMockupRequest()).ToArray(); }
            return result;
        }

        /// <summary>
        /// Sets a new route value for a request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="route">The new value for the route/relative URL part.</param>
        public static void SetRequestRoute(int id, string route) {
            if (!RequestExistsForId(id)) {
                throw new WebApiMockException($"No request with id #{id} found.", 22); }
            using var ctx = new DataContext();
            var item = ctx.Requests.Single(r => r.Id == id);
            if(item.Route.Equals(route, StringComparison.InvariantCultureIgnoreCase)) { return; }
            if(RequestExists(item.Method.ToMethodString(), route, item.Query, item.Body)) {
                throw new WebApiMockException("A request for the given parameters already exists.", 24); }
            item.Route = route;
            ctx.Requests.Update(item);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Sets a new query value for a request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="query">The new query value.</param>
        public static void SetRequestQuery(int id, string query) {
            if (!RequestExistsForId(id)) {
                throw new WebApiMockException($"No request with id #{id} found.", 22); }
            using var ctx = new DataContext();
            var item = ctx.Requests.Single(r => r.Id == id);
            if (item.Query.Equals(query, StringComparison.InvariantCultureIgnoreCase)) { return; }
            if (RequestExists(item.Method.ToMethodString(), item.Route, query, item.Body)) {
                throw new WebApiMockException("A request for the given parameters already exists.", 24); }
            item.Query = query;
            ctx.Requests.Update(item);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Sets a new body value for an existing request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="body">The new value for the body.</param>
        public static void SetRequestBody(int id, string body) {
            if (!RequestExistsForId(id)) {
                throw new WebApiMockException($"No request with id #{id} found.", 22); }
            using var ctx = new DataContext();
            var item = ctx.Requests.Single(r => r.Id == id);
            if (item.Body.Equals(body, StringComparison.InvariantCultureIgnoreCase)) { return; }
            if (RequestExists(item.Method.ToMethodString(), item.Route, item.Query, body)) {
                throw new WebApiMockException("A request for the given parameters already exists.", 24); }
            item.Body = body;
            ctx.Requests.Update(item);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Sets a new HTTP method for an existing request.
        /// </summary>
        /// <param name="id">The id of the request.</param>
        /// <param name="httpMethod">The new value for the HTTP method.</param>
        public static void SetRequestMethod(int id, string httpMethod) {
            if (!RequestExistsForId(id)) {
                throw new WebApiMockException($"No request with id #{id} found.", 22); }
            var method = httpMethod.ToMethodEnum();
            if(method == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"Unknown HTTP method '{httpMethod}'.", 27); }
            using var ctx = new DataContext();
            var item = ctx.Requests.Single(r => r.Id == id);
            if (item.Method.Equals(method)) { return; }
            if (RequestExists(httpMethod, item.Route, item.Query, item.Body)) {
                throw new WebApiMockException("A request for the given parameters already exists.", 24); }
            item.Method = method;
            ctx.Requests.Update(item);
            ctx.SaveChanges();

        }

        #endregion

        /// <summary>
        /// Sets the request method for a certain route to the given HTTP method.
        /// </summary>
        /// <param name="route">The route to change.</param>
        /// <param name="httpMethod">The new method.</param>
        public static void SetMethodForRoute(string route, string httpMethod) {
            var method = httpMethod.ToMethodEnum();
            if (method == HttpMethodEnum.Unknown) { throw new WebApiMockException($"Invalid HTTP method '{httpMethod}'.", 97); }
            using var ctx = new DataContext();
            foreach (var request in ctx.Requests) {
                if (!request.Route.Equals(route, StringComparison.InvariantCultureIgnoreCase)) { continue; }
                request.Method = method;
                ctx.Requests.Update(request); }
            ctx.SaveChanges();
        }

    }
}
