using System;
using System.Linq;
using System.Text;
using WebApiMock.Data;

namespace WebApiMock {
    /// <summary>
    /// General purpose extension methods.
    /// </summary>
    public static class Extensions {

        /// <summary>
        /// Gets the content of a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The content of the stream.</returns>
        public static string ReadToEnd(this System.IO.Stream stream) {
            var retVal = string.Empty;

            using(var rd = new System.IO.StreamReader(stream)) {
                retVal = rd.ReadToEnd(); }
            return retVal;
        }

        /// <summary>
        /// Converts a <see cref="HttpMethodEnum"/> into a proper string.
        /// </summary>
        /// <param name="method">The <see cref="HttpMethodEnum"/> to get the string from.</param>
        /// <returns>A string representation of the HTTP method.</returns>
        public static string ToMethodString(this HttpMethodEnum method) => method switch {
            HttpMethodEnum.GET => "GET",
            HttpMethodEnum.POST => "POST",
            HttpMethodEnum.PUT => "PUT",
            HttpMethodEnum.DELETE => "DELETE",
            HttpMethodEnum.PATCH => "PATCH",
            _ => "Unknown", };

        /// <summary>
        /// Converts a string into a HTTP method enumeration value (<see cref="HttpMethodEnum"/>).
        /// </summary>
        /// <param name="methodString">The string to convert into a HTTP method enumeration value.</param>
        /// <returns>The proper enumeration value.</returns>
        public static HttpMethodEnum ToMethodEnum(this string methodString) => (methodString.ToUpper()) switch {
            "DELETE" => HttpMethodEnum.DELETE,
            "GET" => HttpMethodEnum.GET,
            "PATCH" => HttpMethodEnum.PATCH,
            "POST" => HttpMethodEnum.POST,
            "PUT" => HttpMethodEnum.PUT,
            _ => HttpMethodEnum.Unknown, };

        /// <summary>
        /// Gets the response string from a legacy mockup service direcory.
        /// </summary>
        /// <param name="dir">The direcory to extract the response from.</param>
        /// <returns>The response string (default is an empty string).</returns>
        public static string GetMockupResponse(this System.IO.DirectoryInfo dir) {
            var hasResponse = dir.GetFiles("response.json").Any();
            if(!hasResponse) { return string.Empty; }
            return System.IO.File.ReadAllText(System.IO.Path.Combine(dir.FullName, "response.json"));
        }

        /// <summary>
        /// Gets the status code from a legacy mockup service directory.
        /// </summary>
        /// <param name="dir">The directory to extract the status code from.</param>
        /// <returns>The HTTP status code for the directory (default is 200).</returns>
        public static int GetMockupStatusCode(this System.IO.DirectoryInfo dir) {
            var statusCode = 200;
            var hasStatusCode = dir.GetFiles("*.statuscode").Any();
            if(!hasStatusCode) { return statusCode; }
            var errorCodes = dir.GetFiles("*.statuscode")
                .Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length))
                .ToList();
            foreach (var code in errorCodes) {
                if (!int.TryParse(code, out int codeValue)) {
                    continue; }
                statusCode = codeValue; }
            return statusCode;

        }

        /// <summary>
        /// Returns the message for an exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>The full exception description including the inner exception's messages.</returns>
        public static string GetFullMessage(this Exception ex) {
            var bld = new StringBuilder();
            var currentException = ex;
            var tmpString = currentException.Message;

            bld.Append(tmpString);
            if(!tmpString.EndsWith(".")) { bld.Append("."); }
            while(currentException.InnerException != null) {
                if (tmpString.IndexOf(currentException.InnerException.Message, StringComparison.InvariantCultureIgnoreCase) >= 0) {
                    break; }
                currentException = currentException.InnerException;
                bld.Append(" ");
                bld.Append(currentException.Message);
                tmpString = bld.ToString();
                if (!tmpString.EndsWith(".")) { bld.Append("."); } }
            return bld.ToString();
        }
    }
}
