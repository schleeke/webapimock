﻿using System;
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
    public class DataService {
        
        /// <inheritdoc />
        public DataService() {
            using var context = new DataContext();
            context.Database.EnsureCreated();
        }

        #region "Response"

        /// <summary>
        /// Checks if a response with the give record id exists.
        /// </summary>
        /// <param name="id">The id of the response record.</param>
        /// <returns>True if a response record with the given id exists.</returns>
        public bool ResponseExistsForId(int id) {
            using var ctx = new DataContext();
            return ctx.Responses.Any(r => r.Id.Equals(id));
        }

        /// <summary>
        /// Checks if a given respond exists.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="response">An (optional) return value for the response.</param>
        /// <param name="mimeType">The MIME type of the (optional) return value for the response.</param>
        /// <returns>True, if a response with the given values was found.</returns>
        /// <remarks>
        /// If an empty response is passed, the records in the database will be checked for NULL values or empty strings.
        /// The method will also skip MIME type checking if no response value was passed.
        /// </remarks>
        public bool ResponseExists(int statusCode, string response="", string mimeType="") {
            List<ResponseDefinition> tmpResponses;
            var executionId = Guid.NewGuid();
            var comp = StringComparison.InvariantCultureIgnoreCase;
            if (string.IsNullOrEmpty(response)) {
                Logger.Debug($"Checking if response for HTTP {statusCode} exists (NO RESPONSE)."); }
            else {
                Logger.Debug($"Checking if response for HTTP {statusCode} and response for MIME type '{mimeType}' exists."); }

            using var ctx = new DataContext();
            tmpResponses = ctx.Responses.Where(r => r.StatusCode == statusCode).ToList();
            if(tmpResponses.Count < 1) {
                Logger.Debug($"No response found.");
                return false; }
            if (string.IsNullOrEmpty(response)) {
                tmpResponses = tmpResponses.Where(r => r.Response == null || string.IsNullOrEmpty(response)).ToList(); }
            else {
                tmpResponses = tmpResponses.Where(r => r.Response.Equals(response, comp)).ToList();
                if (tmpResponses.Count < 1) {
                    Logger.Debug($"No response found.");
                    return false; }
                tmpResponses = tmpResponses.Where(r => r.MimeType.Equals(mimeType, comp)).ToList(); }
            if (tmpResponses.Count < 1) {
                Logger.Debug($"No response found.");
                return false; }
            Logger.Debug($"Found existing response.");
            return true;
        }

        /// <summary>
        /// Gets all defined mockup responses.
        /// </summary>
        /// <returns>An array with all defined mockup responses.</returns>
        public MockupResponse[] GetResponses() {
            using var ctx = new DataContext();
            return ctx.Responses.Select(r => r.ToMockupResponse()).ToArray();
        }

        /// <summary>
        /// Gets/returns a mockup response with a given record id.
        /// </summary>
        /// <param name="id">The id of the mockup response record.</param>
        /// <returns>The response record with the mockup values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #11 if no response record with the given id was found.</exception>
        public MockupResponse GetResponseById(int id) {
            if(!ResponseExistsForId(id)) { throw new WebApiMockException($"Unable to find a response definition for the id #{id}.", 11); }
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
        public MockupResponse[] GetAllResponsesForStatusCode(int statusCode) {
            if(!ResponseExists(statusCode)) { throw new WebApiMockException($"Unable to find a response for the status code #{statusCode}.", 1); }
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
        /// <returns>The mockup response record for the given values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #2 if no matching response was found.</exception>
        public MockupResponse GetResponse(int statusCode, string response="", string mimeType="") {
            if (string.IsNullOrEmpty(response)) {
                Logger.Debug($"Reading response for HTTP status {statusCode}."); }
            else {
                Logger.Debug($"Reading response for HTTP status {statusCode} and response for MIME type '{mimeType}'."); }
            if (!ResponseExists(statusCode, response, mimeType)) {
                throw new WebApiMockException($"Unable to find a response for the status code #{statusCode}.", 2); }
            var comp = StringComparison.InvariantCultureIgnoreCase;
            List<ResponseDefinition> tmpList;
            using var ctx = new DataContext();
            tmpList = ctx.Responses.Where(r => r.StatusCode == statusCode).ToList();
            if (string.IsNullOrEmpty(response)) {
                tmpList = tmpList.Where(r => r.Response == null || string.IsNullOrEmpty(r.Response)).ToList();
                Logger.Debug($"Returning response with no response and HTTP {statusCode} only.");
                return tmpList.Single().ToMockupResponse(); }
            tmpList = tmpList.Where(r => r.Response.Equals(response, comp)).ToList();
            tmpList = tmpList.Where(r => r.MimeType.Equals(mimeType, comp)).ToList();
            Logger.Debug($"Retunring response for HTTP status {statusCode} and response with MIME type '{mimeType}'.");
            return tmpList.Single().ToMockupResponse();
        }

        /// <summary>
        /// Adds a new mockup response to the database.
        /// </summary>
        /// <param name="response">The new response.</param>
        /// <returns>The new mockup response with the matching id.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #3 if the id of the given response is not 0 (zero).</exception>
        /// <exception cref="WebApiMockException">Thrown with error code #4 if a response with the same values already exists.</exception>
        public MockupResponse AddResponse(MockupResponse response) {
            if(response.Id != 0) { throw new WebApiMockException("The id for a new response must be 0.", 3); }
            if(ResponseExists(response.StatusCode, response.Response, response.MimeType)) {
                throw new WebApiMockException("The response already exists.", 4); }
            var newData = new ResponseDefinition {
                StatusCode = response.StatusCode,
                Response = response.Response,
                MimeType = response.MimeType };
            using var ctx = new DataContext();
            ctx.Responses.Add(newData);
            ctx.SaveChanges();
            Logger.Info($"Successfully created response (HTTP {response.StatusCode}) with id #{newData.Id}.");
            return newData.ToMockupResponse();
        }

        /// <summary>
        /// Sets the value of an existing mockup response record HTTP status code to a new one.
        /// </summary>
        /// <param name="id">The id of the mockup response record.</param>
        /// <param name="statusCode">The value of the new HTTP status code.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #5 if no mockup response record with the given id exists.</exception>
        public void SetResponseStatusCode(int id, int statusCode) {
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
        public void SetResponseResponse(int id, string response) {
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
        public void SetResponseMimeType(int id, string mimeType) {
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
        public void RemoveResponse(int id) {
            if (!ResponseExistsForId(id)) { throw new WebApiMockException($"No response exists with the id #{id}.", 5); }
            using var ctx = new DataContext();
            var item = ctx.Responses.Single(r => r.Id.Equals(id));
            ctx.Responses.Remove(item);
            ctx.SaveChanges();
        }

        #endregion

        #region "Request"

        public bool RequestExistsForId(int id) {
            using var ctx = new DataContext();
            return ctx.Requests.Any(r => r.Id.Equals(id));
        }

        public bool RequestExists(string httpMethod, string route, string query="", string body="") {
            var method = httpMethod.ToMethodEnum();
            var comp = StringComparison.InvariantCultureIgnoreCase;
            if(method == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"Unknown HTTP method '{httpMethod}'.", 10); }
            if (route.StartsWith("/")) { route = route.Substring(1); }
            using var ctx = new DataContext();

            var tmpResult = ctx.Requests.Where(r => r.Method == method).ToList();
            if(tmpResult.Count() < 1) {
                return false; }
            tmpResult = tmpResult.Where(r => r.Route.Equals(route, comp)).ToList();
            if (tmpResult.Count() < 1) {
                return false; }
            if(string.IsNullOrEmpty(query)) {
                tmpResult = tmpResult.Where(r => r.Query == null || string.IsNullOrEmpty(r.Query)).ToList(); }
            else {
                tmpResult = tmpResult.Where(r => r.Query.Equals(query, comp)).ToList(); }
            if (tmpResult.Count() < 1) {
                return false; }
            if(string.IsNullOrEmpty(body)) {
                tmpResult = tmpResult.Where(r => r.Body == null || string.IsNullOrEmpty(r.Body)).ToList(); }
            else {
                tmpResult = tmpResult.Where(r => r.Body.Equals(body, comp)).ToList(); }
            return (tmpResult.Count() > 0);
        }

        public MockupRequest GetRequestById(int id) {
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
        /// <returns>The mockup request record with the given values.</returns>
        /// <exception cref="WebApiMockException">Thrown with error code #10 if the HTTP method is unknown.</exception>
        /// <exception cref="WebApiMockException">Thrown with error code #13 if no request with the given values was found.</exception>
        public MockupRequest GetRequest(string httpMethod, string route, string query="", string body="") {
            var method = httpMethod.ToMethodEnum();
            var comp = StringComparison.InvariantCultureIgnoreCase;
            List<RequestDefinition> tmpList;

            if (method == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"Unknown HTTP method '{httpMethod}'.", 10); }
            if (route.StartsWith("/")) {
                route = route.Substring(1); }
            using var ctx = new DataContext();
            tmpList = ctx.Requests.Where(req => req.Method == method).ToList();
            if(tmpList.Count < 1) {
                throw new WebApiMockException("No request found.", 13); }
            tmpList = tmpList.Where(req => req.Route.Equals(route, comp)).ToList();
            if (tmpList.Count < 1) {
                throw new WebApiMockException("No request found.", 13); }
            if (string.IsNullOrEmpty(query)) {
                tmpList = tmpList.Where(r => r.Query == null || string.IsNullOrEmpty(r.Query)).ToList(); }
            else {
                tmpList = tmpList.Where(req => req.Query.Equals(query, comp)).ToList(); }
            if (tmpList.Count < 1) {
                throw new WebApiMockException("No request found.", 13); }
            if(string.IsNullOrEmpty(body)) {
                tmpList = tmpList.Where(r => r.Body == null || string.IsNullOrEmpty(r.Body)).ToList(); }
            else {
                tmpList = tmpList.Where(req => req.Body.Equals(body, comp)).ToList(); }
            if (tmpList.Count < 1) {
                throw new WebApiMockException("No request found.", 13); }
            return tmpList.Single().ToMockupRequest();
        }

        public MockupRequest AddRequest(MockupRequest request) {
            var method = request.HttpMethod.ToMethodEnum();
            bool exists;
            RequestDefinition newData;

            if(method == HttpMethodEnum.Unknown) {
                throw new WebApiMockException($"Unknown HTTP method '{request.HttpMethod}'.", 16); }
            if(request.ResponseId == 0) {
                throw new WebApiMockException("No response set for the new request.", 17); }
            exists = ResponseExistsForId(request.ResponseId);
            if(!exists) {
                throw new WebApiMockException($"No response with id #{request.ResponseId} found.", 18); }
            if (request.Id != 0) {
                throw new WebApiMockException("The id for a new request must be 0.", 14); }
            if (RequestExists(request.HttpMethod, request.Route, request.Query, request.Body)) {
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
            Logger.Info($"Successfully created request #{newData.Id} ({request.Route}) [{request.HttpMethod}].");
            return newData.ToMockupRequest();
        }

        /// <summary>
        /// Removes a request by its id.
        /// </summary>
        /// <param name="id">The id of the request record that shall be removed.</param>
        /// <exception cref="WebApiMockException">Thrown with error code #19 if no record with the given id was found.</exception>
        public void RemoveRequest(int id) {
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
        
        public void SetRequestResponseId(int id, int responseId) {
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
        public MockupRequest[] GetRequests() {
            MockupRequest[] result;
            using (var ctx = new DataContext()) { result = ctx.Requests.Select(r => r.ToMockupRequest()).ToArray(); }
            return result;
        }

        public void SetRequestRoute(int id, string route) {
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

        public void SetRequestQuery(int id, string query) {
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

        public void SetRequestBody(int id, string body) {
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

        public void SetRequestMethod(int id, string httpMethod) {
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
        /// The program's logger.
        /// </summary>
        private static Topshelf.Logging.LogWriter Logger => Topshelf.Logging.HostLogger.Get(typeof(Program));

    }
}