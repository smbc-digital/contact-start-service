using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using contact_start_service.Utils.HealthChecks;
using contact_start_service.Utils.ServiceCollectionExtensions;
using contact_start_service.Utils.StorageProvider;
using Microsoft.Extensions.Configuration;
using StockportGovUK.AspNetCore.Availability;
using StockportGovUK.AspNetCore.Availability.Middleware;
using StockportGovUK.AspNetCore.Middleware;
using StockportGovUK.NetStandard.Gateways.Extensions;
using StockportGovUK.NetStandard.Gateways.MailingService;
using StockportGovUK.NetStandard.Gateways.VerintService;

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
            services
                .AddControllers()
                .AddNewtonsoftJson();
            services.AddStorageProvider(Configuration);
            services.AddGateways(Configuration);
            services.AddServices();
            services.AddConfiguration(Configuration);
            services.AddSwagger();

            services
                .AddHealthChecks()
                .AddCheck<TestHealthCheck>("TestHealthCheck");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler($"/api/v1/error{(env.IsDevelopment() ? "/local" : string.Empty)}");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseHealthChecks("/healthcheck", HealthCheckConfig.Options);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Contact START Service API");
            });
        }
    }
}
