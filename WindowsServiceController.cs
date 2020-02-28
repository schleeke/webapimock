using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Topshelf;
using WebApiMock.Web;

namespace WebApiMock {
    /// <summary>
    /// Controller class for the Topshelf service wrapper.
    /// </summary>
    public class WindowsServiceController : IDisposable {
        private readonly string[] _cmdlineArgs;
        private IHost _host;

        public WindowsServiceController(string[] args) => _cmdlineArgs = args;

        /// <summary>
        /// Starts the service/program.
        /// </summary>
        /// <param name="hostCrtl">The <see cref="HostControl"/> of the current service controller.</param>
        /// <returns>True if successful.</returns>
        public bool Start(HostControl hostCrtl) {
            _host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(_cmdlineArgs).ConfigureWebHostDefaults(bld => {
                bld.UseStartup<Startup>();
                bld.UseKestrel(options => {
                    options.AllowSynchronousIO = true;
                });
            }).Build();
            _host.RunAsync();
            return true;
        }

        /// <summary>
        /// Stops the service/program.
        /// </summary>
        /// <param name="hostCrtl">The <see cref="HostControl"/> of the current service controller.</param>
        /// <returns>True if successful.</returns>
        public bool Stop(HostControl hostCrtl) {
            _host?.StopAsync();
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (disposedValue) { return; }
            if (disposing) {
                _host?.StopAsync();
                _host.Dispose(); }
            disposedValue = true;
        }

        public void Dispose() { Dispose(true); }
        #endregion

    }
}
