﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using contact_start_service.Utils.HealthChecks;
using contact_start_service.Utils.ServiceCollectionExtensions;
using contact_start_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Availability;
using StockportGovUK.AspNetCore.Availability.Middleware;
using StockportGovUK.AspNetCore.Middleware;

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
            services.RegisterServices();
            services.AddAvailability();
            services.AddSwagger();

            services
                .AddHealthChecks()
                .AddCheck<TestHealthCheck>("TestHealthCheck");

            services
                .AddMvc()
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
                c.SwaggerEndpoint("v1/swagger.json", "Contact START Service API");
            });
        }
    }
}
