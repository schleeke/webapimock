using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Topshelf;
using Topshelf.Logging;

namespace WebApiMock {
    /// <inheritdoc/>
    public static class Program {

        private static LogWriter _log = null;

        /// <summary>
        /// Main entry point of the program.
        /// </summary>
        /// <param name="args">A list of command line arguments.</param>
        /// <returns>0 (zero) if no errors occured.</returns>
        /// <remarks>
        ///  The value corresponds to the <see cref="TopshelfExitCode"/> enumeration.
        /// </remarks>
        public static int Main(string[] args) {
          var retVal = HostFactory.Run(config => {
              config.OnException((ex) => Console.WriteLine($"An unexpected error occured: {ex.Message}"));
              config.RunAsLocalSystem();
              config.UseLog4Net("logging.config", true);
              config.StartAutomaticallyDelayed();
              config.SetServiceName("webapimock.core");
              config.SetDescription(".NET Core mock-up service/tool for web APIs.");
              config.SetDisplayName(".NET Core Web API Mockup Service");
              config.Service<WindowsServiceController>(svc => {
                  svc.ConstructUsing(() => new WindowsServiceController(args));
                  svc.WhenStarted((wsc, crtl) => wsc.Start(crtl));
                  svc.WhenStopped((wsc, crtl) => wsc.Stop(crtl)); });
          });
          return (int)retVal;
        }

        /// <summary>
        /// The application's directory.
        /// </summary>
        public static System.IO.DirectoryInfo ApplicationDirectory
            => new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        /// <summary>
        /// The application's logger.
        /// </summary>
        public static LogWriter Logger {
            get {
                if(_log == null) {
                    _log = HostLogger.Get(typeof(Program)); }
                return _log;
            }
        }
    }
}
