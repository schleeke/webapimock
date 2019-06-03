using System;
using System.Text;

namespace WebApiMock
{
    public static class Extensions {
        /// <summary>
        /// No explanation needed ;)
        /// </summary>
        /// <param name="s">The current string.</param>
        /// <returns>True if the string is NULL or empty.</returns>
        public static bool IsNullOrEmpty(this string s) {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// Returns the full exception message containing all the messages from inner exceptions.
        /// </summary>
        /// <param name="ex">The exception to get the full message from.</param>
        /// <param name="stackTrace">Indicates if the stack trace should be returned as well.</param>
        /// <returns>A full error message string for the given exception.</returns>
        public static string GetFullMessage(this Exception ex, bool stackTrace = false) {
            var returnValue = new StringBuilder();
            var currentException = ex;

            returnValue.Append(currentException.Message);

            if (!currentException.Message.EndsWith(".")) { returnValue.Append("."); }            
            while(currentException.InnerException != null) {
                var completeString = returnValue.ToString();
                currentException = currentException.InnerException;
                if(completeString.IndexOf(currentException.Message, StringComparison.InvariantCultureIgnoreCase) >= 0) { break; }
                returnValue.Append(" ");
                returnValue.Append(currentException.Message);
                if (!currentException.Message.EndsWith(".")) { returnValue.Append("."); }
            }
            if (stackTrace) {
                returnValue.AppendLine();
                returnValue.Append(ex.StackTrace);
            }
            return returnValue.ToString();
        }
    }
}
