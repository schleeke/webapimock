using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebApiMock.Startup))]
namespace WebApiMock {
    public class Startup {

        public void Configuration(IAppBuilder appBuilder) {
            appBuilder.Use( async(context, next) => {
                var httpResult = await WindowsService.GetHttpResponse(context);
                context.Response.StatusCode = httpResult.StatusCode;
                context.Response.ContentType = httpResult.ContentType;
                await context.Response.WriteAsync(httpResult.Content);
                await next();
            });
        }

    }
}
