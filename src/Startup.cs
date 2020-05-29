using System.Diagnostics.CodeAnalysis;
using contact_start_service.Utils.HealthChecks;
using contact_start_service.Utils.ServiceCollectionExtensions;
using contact_start_service.Utils.StorageProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockportGovUK.AspNetCore.Middleware;
using StockportGovUK.AspNetCore.Availability;
using StockportGovUK.AspNetCore.Availability.Middleware;
using StockportGovUK.NetStandard.Gateways;
using contact_start_service.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using contact_start_service.Config;

namespace contact_start_service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStorageProvider(Configuration);
            services.AddResilientHttpClients<IGateway, Gateway>(Configuration);
            services.Configure<VerintConfiguration>(settings => Configuration.GetSection("VerintConfiguration").Bind(settings));
            services.AddSingleton<IContactSTARTService, ContactSTARTService>();
            services.AddAvailability();
            services.AddSwagger();
            services.AddHealthChecks()
                    .AddCheck<TestHealthCheck>("TestHealthCheck");

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddMvcOptions(_ => _.AllowEmptyInputInBodyModelBinding = true)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Culture = new CultureInfo("en-GB");
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseMiddleware<Availability>();
            app.UseMiddleware<ApiExceptionHandling>();
            
            app.UseHealthChecks("/healthcheck", HealthCheckConfig.Options);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "contact_start_service API");
            });
        }
    }
}
