using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(opts =>
            {
                opts.Conventions.Insert(0, new LocalizationConvention());
                opts.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));
            });

            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("de"),
                new CultureInfo("fr-FR"),
            };

            var options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            options.RequestCultureProviders = new[] { new RouteDataRequestCultureProvider() { Options = options } };

            services.AddSingleton(options);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{culture}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
