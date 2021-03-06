﻿using Microsoft.AspNetCore.Http;
using System;
using WebApiMock.Data;

namespace WebApiMock.Web {
    /// <summary>
    /// Extension methods for the web/HTTP module(s) of the application.
    /// </summary>
    public static class WebExtensions {

        /// <summary>
        /// Gets the HTTP method from the request context.
        /// </summary>
        /// <param name="context">The HTTP context to get the HTTP method from.</param>
        /// <returns>The value of the HTTP method.</returns>
        public static HttpMethodEnum GetHttpMethod(this HttpContext context) => context.Request.Method switch {
            "GET" => HttpMethodEnum.GET,
            "PUT" => HttpMethodEnum.PUT,
            "DELETE" => HttpMethodEnum.DELETE,
            "POST" => HttpMethodEnum.POST,
            "PATCH" => HttpMethodEnum.PATCH,
            _ => HttpMethodEnum.Unknown, };

        /// <summary>
        /// Extracts the infos that are needed for mock-up detection from the HTTP request context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Route (relative path), query part, body and HTTP method of the request.</returns>
        public static (string Path, string Query, string Body, HttpMethodEnum Method) GetRequestInfo(this HttpContext context) {
            string path;
            string query;
            var body = "";
            HttpMethodEnum method;

            path = context.Request.Path.Value;
            if(path.IndexOf($"/{Program.MockupRelativePath}", System.StringComparison.InvariantCultureIgnoreCase) == 0) {
                path = path.Substring($"/{Program.MockupRelativePath}".Length); }
            if (string.IsNullOrEmpty(path)) {
                path = "/"; }
            if(path.StartsWith("/")) { path = path.Substring(1); }
            query = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : "";
            if (query.StartsWith("?")) { query = query.Substring(1); }
            if (context.Request.Body != null) {
                body = context.Request.Body.ReadToEnd(); }
            method = context.GetHttpMethod();
            return (Path: path, Query: query, Body: body, Method: method);
        }

        /// <summary>
        /// Checks if the incoming request is a mock-up one.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>True if this is a request for the mock-up route.</returns>
        public static bool IsMockupRequest(this HttpRequest request)
            => request.Path.HasValue && request.Path.Value.IndexOf($"/{Program.MockupRelativePath}", StringComparison.InvariantCultureIgnoreCase) == 0;

    }
}
