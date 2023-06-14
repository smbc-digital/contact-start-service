using contact_start_service.Config;
using contact_start_service.Services;
using Microsoft.OpenApi.Models;
using StockportGovUK.NetStandard.Gateways.Extensions;
using StockportGovUK.NetStandard.Gateways.MailingService;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace contact_start_service.Utils.ServiceCollectionExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IContactSTARTService, ContactSTARTService>();

            return services;
        }

        public static IServiceCollection AddGateways(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IVerintServiceGateway, VerintServiceGateway>(configuration);
            services.AddHttpClient<IMailingServiceGateway, MailingServiceGateway>(configuration);

            return services;
        }

        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VerintConfiguration>(settings => configuration.GetSection("VerintConfiguration").Bind(settings));

            return services;
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contact START Service API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = "Authorization using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
