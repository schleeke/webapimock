using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using WebApiMock.Data;

namespace WebApiMock.Web {
    /// <summary>
    /// Configuration for the web server.
    /// </summary>
    public class Startup {
        private readonly IConfiguration _config;

        /// <inheritdoc/>
        public Startup(IConfiguration config) => _config = config;



        public void ConfigureServices(IServiceCollection services) {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            var uri = GetHttpsUri();
            services.AddControllers();
            services.AddSingleton<DataService>();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact {
                        Name = "Dashboard",
                        Url = uri },
                    Title = "Web API mockup service",
                    Version = "v1",
                    Description = "Add, remove or alter existing mockup definitions." });
                options.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseMiddleware<MockupMiddleware>();
            app.UseDefaultFiles(new DefaultFilesOptions {
                RequestPath = "/gui"
            });
            app.UseStaticFiles("/gui");
            app.UseSwaggerUI(options => {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API mockup service");
            });
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
        
        private Uri GetHttpsUri() {
            foreach (var category in _config.GetChildren()) {
                if(category.Key.Equals("ASPNETCORE_URLS", StringComparison.InvariantCultureIgnoreCase) == false) { continue; }
                var urlStrings = category.Value.Split(';');
                foreach (var url in urlStrings) {
                    var newUri = new Uri(url + "/gui");
                    if(newUri.Scheme.IndexOf("https", StringComparison.InvariantCultureIgnoreCase) < 0) { continue; }
                    return newUri; } }
            return null;
        }
    }
}
