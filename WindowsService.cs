using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using System;
using System.Threading.Tasks;
using Topshelf;

namespace WebApiMock
{

    public partial class WindowsService {
        private IDisposable _client;
        private static string _baseUri = string.Empty;
        private static bool _createDefaultResponse = false;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="baseUri">The base URI for the web API.</param>
        /// <param name="createDefaultResponse">Indicates if a default response should be created.</param>
        public WindowsService(string baseUri, bool createDefaultResponse) {
            _baseUri = baseUri;
            _createDefaultResponse = createDefaultResponse;
        }

        /// <summary>
        /// Starts the web API / service.
        /// </summary>
        /// <param name="serviceHost">The Topshelf service host.</param>
        /// <returns></returns>
        public bool Start(HostControl serviceHost) {
            try {
                _client = WebApp.Start<Startup>(url: _baseUri);
            } catch (Exception ex) {
                if (Program.Logger.IsFatalEnabled) { Program.Logger.Fatal($"Unable to start the web service: {ex.GetFullMessage()}"); }
                return false;
            }
            if (Program.Logger.IsInfoEnabled) { Program.Logger.Info($"Web server started an listening @{_baseUri}."); }
            return true;
        }

        /// <summary>
        /// Stops the web API / service.
        /// </summary>
        /// <param name="serviceHost">The Topshelf service host.</param>
        /// <returns></returns>
        public bool Stop(HostControl serviceHost) {
            try {
                _client.Dispose();
            } catch (Exception ex) {
                if (Program.Logger.IsWarnEnabled) { Program.Logger.Warn($"Unable to dispose/tear down the running instance: {ex.GetFullMessage()}."); }
            }
            if (Program.Logger.IsInfoEnabled) { Program.Logger.Info("Web server stopped."); }
            return true;
        }

        /// <summary>
        /// Gets the mock up response for the given context.
        /// </summary>
        /// <param name="context">The web request context.</param>
        /// <returns>An <see cref="HttpResponseInformation"/>-object with the data to return.</returns>
        public static async Task<HttpResponseInformation> GetHttpResponse(IOwinContext context) {
            return await Task.Run(() => {

                if(Program.Logger.IsDebugEnabled) { Program.Logger.Debug($"Incoming request [{context.Request.Method}]: {context.Request.Uri}"); }
                var uriInfo = new UriInformation(context.Request.Uri);

                //return index.html
                if(uriInfo.RequestRelativePath.Equals("index.html", StringComparison.InvariantCultureIgnoreCase)) {
                    var returnValue = new HttpResponseInformation {
                        ContentType = "text/html",
                        Content = context.Request.Uri.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase)
                        ? LocalhostIndexPage
                        : DefaultIndexPage };
                    if (Program.Logger.IsInfoEnabled) { Program.Logger.Info("index.html delivered."); }
                    return returnValue;
                }

                if(uriInfo.RequestRelativePath.Equals("readme.html", StringComparison.InvariantCultureIgnoreCase)) {
                    var returnValue = new HttpResponseInformation {
                        ContentType = "text/html",
                        Content = System.IO.File.ReadAllText(System.IO.Path.Combine(ApplicationDirectory, "readme.html")) };
                    if (Program.Logger.IsInfoEnabled) { Program.Logger.Info("readme.html delivered."); }
                    return returnValue;
                }


                //return mock response
                return GetResponse(uriInfo.RequestLocalPath.FullName);
            });
        }

        /// <summary>
        /// The application directory.
        /// </summary>
        internal static string ApplicationDirectory => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Gets the response from the local path.
        /// </summary>
        /// <param name="localResponsePath">The full path to the response file.</param>
        /// <returns></returns>
        private static HttpResponseInformation GetResponse(string localResponsePath) {
            var returnValue = new HttpResponseInformation();
            var responseDir = new System.IO.DirectoryInfo(localResponsePath);
            var relativePath = localResponsePath.Substring(ApplicationDirectory.Length);
            if (relativePath.StartsWith("\\")) { relativePath = relativePath.Substring(1); }

            if (Program.Logger.IsDebugEnabled) { Program.Logger.Debug($"Processing relative path '{relativePath}'..."); }

            // create default response if no directory exists and creating a default response is enabled.
            if (!responseDir.Exists && _createDefaultResponse) {
                responseDir.Create();
                var fileName = System.IO.Path.Combine(localResponsePath, "response.json");
                System.IO.File.WriteAllText(fileName, "{\"Message\":\"No content\"}");
                if (Program.Logger.IsDebugEnabled) { Program.Logger.Debug($"The directory '{relativePath}' for the response was created with a default response."); }
                returnValue.Content = "{\"Message\":\"No content\"}";
                returnValue.StatusCode = 404;
                returnValue.ContentType = "application/json";
                return returnValue;
            }

            // return if no response was defined/present.
            if (!responseDir.Exists) {
                returnValue.StatusCode = 404;
                returnValue.ContentType = "text/html";
                returnValue.Content = GetErrorHtmlPage("No mock up response defined");
                if(Program.Logger.IsWarnEnabled) { Program.Logger.Warn("No mock up response defined."); }
                return returnValue;
            }

            // handle status code files.
            var statusCodes = responseDir.GetFiles("*.statuscode");
            if(statusCodes.GetUpperBound(0) >= 0) {
                var statusCodeString = statusCodes[0].Name.Substring(0, 3);
                if(!int.TryParse(statusCodeString, out var parsedResult)) {
                    if(Program.Logger.IsErrorEnabled) { Program.Logger.Error($"Unable to parse the status code '{statusCodeString}'."); }
                    returnValue.StatusCode = 900;
                    returnValue.ContentType = "text/html";
                    returnValue.Content = GetErrorHtmlPage("The server was unable to determine the mocked status code for this URI");
                    return returnValue;
                } else {
                    returnValue.StatusCode = parsedResult;
                    returnValue.ContentType = "text/html";
                    returnValue.Content = GetStatusCodeHtmlPage(parsedResult);
                    if(Program.Logger.IsDebugEnabled) { Program.Logger.Debug($"Returned status code {statusCodeString} for the request."); }
                    return returnValue;
                }
            }

            // handle repsponse file.
            var responseFile = new System.IO.FileInfo(System.IO.Path.Combine(localResponsePath, "response.json"));
            if(!responseFile.Exists) {
                returnValue.StatusCode = 901;
                returnValue.ContentType = "text/html";
                returnValue.Content = GetErrorHtmlPage("No response file was found");
                if (Program.Logger.IsWarnEnabled) { Program.Logger.Warn("No mock up response defined."); }
                return returnValue;
            }
            returnValue.Content = System.IO.File.ReadAllText(responseFile.FullName);
            returnValue.ContentType = "application/json";
            returnValue.StatusCode = 200;
            if (Program.Logger.IsDebugEnabled) { Program.Logger.Debug("Returned JSON response."); }
            return returnValue;
        }

        /// <summary>
        /// Replaces the asterisks in the base URI with the host name that is used in the client request.
        /// </summary>
        /// <param name="requestUri">The URI the client has requested.</param>
        /// <returns>The base URI with the host name of the request.</returns>
        private static string GetBaseUriWithHostFromRequest(string requestUri) {
            var reqAdr = new Uri(requestUri);
            var hostName = reqAdr.Host;
            var returnValue = _baseUri;
            if(_baseUri.IndexOf("http://*", StringComparison.InvariantCultureIgnoreCase) == 0) {
                returnValue = _baseUri.ToLower().Replace("http://*", $"http://{hostName}");
            }
            if (_baseUri.IndexOf("https://*", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                returnValue = _baseUri.ToLower().Replace("https://*", $"https://{hostName}");
            }
            if (!returnValue.EndsWith("/")) { returnValue += "/"; }
            return returnValue;
        }

        /// <summary>
        /// Returns HTML code for an error page.
        /// </summary>
        /// <param name="errorDescription">The description for the error.</param>
        private static string GetErrorHtmlPage(string errorDescription) {
            var page = $@"
<!DOCTYPE html>
<html>
<head>
  <title>ERROR | WebAPI Mockup service</title>
</head>
<body>
  <pre>An error occured ({errorDescription})</pre>
</body>
</html>
";
            return page;
        }

        /// <summary>
        /// Returns HTML code for an error page for Status Codes != 200.
        /// </summary>
        /// <param name="statusCode">The status code that will be returned to the request.</param>
        /// <returns></returns>
        private static string GetStatusCodeHtmlPage(int statusCode) {
            var page = $@"
<!DOCTYPE html>
<html>
<head>
  <title>ERROR | WebAPI Mockup service</title>
</head>
<body>
  <pre>The status code is {statusCode}.</pre>
</body>
</html>
";
            return page;
        }

        /// <summary>
        /// The value for the default index.html answer.
        /// </summary>
        private static string DefaultIndexPage
        {
            get
            {
                var page = @"
<!DOCTYPE html>
<html>
<head>
  <title>WebAPI Mockup service</title>
</head>
<body>
  <h1>Welcome</h1>
</body>
</html>
";
                return page;
            }
        }

        /// <summary>
        /// The value for the localhost index.html answer.
        /// </summary>
        private static string LocalhostIndexPage
        {
            get
            {
                var page = $@"
<!DOCTYPE html>
<html>
<head>
  <title>WebAPI Mockup service</title>
</head>
<body>
  <h1>Welcome</h1>
  <pre>The working directory is: {ApplicationDirectory}</pre>
</body>
</html>
";
                return page;
            }
        }

        internal static string BaseUri => _baseUri;
    }
}
