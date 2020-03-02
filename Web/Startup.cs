using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace WebApiMock.Web {
    /// <summary>
    /// Configuration for the web server.
    /// </summary>
    public class Startup {
        private readonly IConfiguration _config;

        /// <inheritdoc/>
        public Startup(IConfiguration config) => _config = config;


        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services) {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            Uri.TryCreate("/gui", UriKind.Relative, out Uri httpsUri);
            services.AddControllers();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo {
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact {
                        Name = "Dashboard",
                        Url = httpsUri },
                    Title = "Web API mockup service",
                    Version = "v1",
                    Description = "Add, remove or alter existing mockup definitions." });
                if(File.Exists(xmlPath)) {
                    options.IncludeXmlComments(xmlPath); }
            });
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Implements base class method.")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage(); }
            app.UseSwagger();
            app.UseMiddleware<MockupMiddleware>();
            app.UseCors(bld => {
                bld.AllowAnyOrigin();
                bld.AllowAnyHeader();
                bld.AllowAnyMethod(); });
            app.UseDefaultFiles(new DefaultFilesOptions {
                RequestPath = "/gui" });
            app.UseStaticFiles("/gui");
            app.UseSwaggerUI(options => {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API mockup service"); });
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers(); });
            var httpsUrl = GetHttpsUri();
            var httpUrl = GetHttpUri();
            if(!string.IsNullOrEmpty(httpsUrl)) {
                Program.Logger.Info($"Listening @{httpsUrl}");
                Program.HttpsUrl = httpsUrl; }
            if (!string.IsNullOrEmpty(httpUrl)) {
                Program.Logger.Info($"Listening @{httpUrl}");
                Program.HttpUrl = httpUrl; }
            Program.AutoGenerate = GetAutoGenerate();
            Program.MockupRelativePath = GetRelativeMockupPath();
            Program.Logger.Debug($"Settings: auto-generate={Program.AutoGenerate}, mockup-path={Program.MockupRelativePath}");
        }

        private string GetHttpsUri() {
            var configProviders = ((ConfigurationRoot)_config).Providers;
            foreach (var provider in configProviders) {
                if (typeof(JsonConfigurationProvider) != provider.GetType()) { continue; }
                var propertyInfo = provider.GetType().BaseType.BaseType.GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var configValues = propertyInfo.GetValue(provider) as System.Collections.Generic.IDictionary<string, string>;
                foreach (var key in configValues.Keys) {
                    if (!key.Equals("Kestrel:Endpoints:Https:Url", StringComparison.InvariantCulture)) { continue; }
                    var url = configValues[key];
                    return url; } }
            return string.Empty;
        }

        private string GetHttpUri() {
            var configProviders = ((ConfigurationRoot)_config).Providers;
            foreach (var provider in configProviders) {
                if (typeof(JsonConfigurationProvider) != provider.GetType()) { continue; }
                var propertyInfo = provider.GetType().BaseType.BaseType.GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var configValues = propertyInfo.GetValue(provider) as System.Collections.Generic.IDictionary<string, string>;
                foreach (var key in configValues.Keys) {
                    if (!key.Equals("Kestrel:Endpoints:Http:Url", StringComparison.InvariantCulture)) { continue; }
                    var url = configValues[key];
                    return url; } }
            return string.Empty;
        }

        private bool GetAutoGenerate() {
            var configProviders = ((ConfigurationRoot)_config).Providers;
            foreach (var provider in configProviders) {
                if (typeof(JsonConfigurationProvider) != provider.GetType()) { continue; }
                var propertyInfo = provider.GetType().BaseType.BaseType.GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var configValues = propertyInfo.GetValue(provider) as System.Collections.Generic.IDictionary<string, string>;
                foreach (var key in configValues.Keys) {
                    if (!key.Equals("WebApiMockSettings:AutoGenerateAnswer", StringComparison.InvariantCulture)) { continue; }
                    var boolString = configValues[key];
                    return boolString.Equals("True", StringComparison.InvariantCultureIgnoreCase); } }
            return false;
        }

        private string GetRelativeMockupPath() {
            var relPath = Program.MockupRelativePath;
            var configProviders = ((ConfigurationRoot)_config).Providers;
            
            foreach (var provider in configProviders) {
                if (typeof(JsonConfigurationProvider) != provider.GetType()) { continue; }
                var propertyInfo = provider.GetType().BaseType.BaseType.GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);
                var configValues = propertyInfo.GetValue(provider) as System.Collections.Generic.IDictionary<string, string>;
                foreach (var key in configValues.Keys) {
                    if (!key.Equals("WebApiMockSettings:MockupPrefixRoute", StringComparison.InvariantCulture)) { continue; }
                    relPath = configValues[key]; } }
            return relPath;
        }
    }
}
