using System;
using System.Web;

namespace WebApiMock
{

    public partial class WindowsService {
        public class UriInformation {

            public UriInformation(string requestUri) {
                if(!Uri.TryCreate(requestUri, UriKind.Absolute, out var uri)) { throw new ArgumentException($"The uri string '{requestUri}' is not valid.", nameof(requestUri)); }
                RequestUri = uri;
            }

            public UriInformation(Uri uri) {
                RequestUri = uri;
            }


            public string RequestQuery {
                get {
                    var queryString = RequestUri.Query;
                    if(queryString.IsNullOrEmpty()) { return queryString; }
                    return queryString.Substring(1);
                }
            }

            public string RequestBaseUri {
                get {
                    var hostName = RequestUri.Host;
                    var returnValue = WindowsService.BaseUri;
                    if(WindowsService.BaseUri.IndexOf("https://*", StringComparison.InvariantCultureIgnoreCase) > -1) { returnValue = returnValue.Replace("*", hostName); }
                    if (WindowsService.BaseUri.IndexOf("http://*", StringComparison.InvariantCultureIgnoreCase) > -1) { returnValue = returnValue.Replace("*", hostName); }
                    return returnValue;
                }
            }

            public string RequestRelativePath {
                get {
                    var relativePath = ToString().Substring(RequestBaseUri.Length);
                    var queryIndex = relativePath.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
                    if(relativePath.StartsWith("/")) { relativePath = relativePath.Substring(1); }
                    if(relativePath.EndsWith("/")) { relativePath = relativePath.Substring(0, relativePath.Length - 1); }
                    if(queryIndex < 0) { return relativePath;  }
                    relativePath = relativePath.Substring(0, queryIndex - 1);
                    return relativePath;
                }
            }

            public Uri RequestUri { get; }

            public System.IO.DirectoryInfo RequestLocalPath {
                get {
                    var encodedLocalPath = HttpUtility.UrlEncode(RequestRelativePath);
                    string path;
                    string encodedQuery;
                    System.IO.DirectoryInfo returnValue;

                    if (!RequestQuery.IsNullOrEmpty()) {
                        if(RequestQuery.Length > 100) {
                            encodedQuery = HttpUtility.UrlEncode(RequestQuery.Substring(0, 100));
                            path = System.IO.Path.Combine(WindowsService.ApplicationDirectory, encodedLocalPath, encodedQuery);
                        } else {
                            encodedQuery = HttpUtility.UrlEncode(RequestQuery);
                            path = System.IO.Path.Combine(WindowsService.ApplicationDirectory, encodedLocalPath, encodedQuery);
                        }
                    } else {
                        path = System.IO.Path.Combine(WindowsService.ApplicationDirectory, encodedLocalPath);
                    }
                    try {
                        returnValue = new System.IO.DirectoryInfo(path);
                    } catch (Exception ex) {
                        if(Program.Logger.IsErrorEnabled) { Program.Logger.Error($"Unable to get a valid directory path: {ex.GetFullMessage()}", ex); }
                        throw;
                    }
                    return returnValue;
                }
            }

            public override string ToString() {
                return RequestUri.ToString();
            }



        }
    }
}
