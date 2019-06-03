using System.Configuration;
using Topshelf;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace WebApiMock {
    public static class Program {

        public static int Main() {
            Logger = log4net.LogManager.GetLogger(typeof(Program));
            var baseUri = ConfigurationManager.AppSettings["BaseUrl"];
            var createDefaultResponse = false;
            var createDefaultResponseString = ConfigurationManager.AppSettings["CreateDefaultResponse"];
            if(createDefaultResponseString.Equals("true", System.StringComparison.InvariantCultureIgnoreCase)) { createDefaultResponse = true; }

            // configure and set up Topshelf
            var returnValue = HostFactory.Run(config => {
                config.UseLog4Net();
                config.Service<WindowsService>(svc => {
                    svc.ConstructUsing(factory => new WindowsService(baseUri, createDefaultResponse));
                    svc.WhenStarted((instance, host) => instance.Start(host));
                    svc.WhenStopped((instance, host) => instance.Stop(host));
                });
                config.RunAsLocalSystem();
                config.SetServiceName("WebApiMock");
                config.SetDescription("Webservice / WebAPI with mocked values.");
                config.SetDisplayName("WebAPI Mockup Service");
            });
            return (int)returnValue;
        }

        internal static log4net.ILog Logger { get; private set; }
    }
}