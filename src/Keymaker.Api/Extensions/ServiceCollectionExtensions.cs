using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Configuration;
using Keymaker.Api.Handlers;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureHandlers()
        {
            return services.AddSingleton<RequestHandler>();
        }

        public IServiceCollection ConfigureSerialization()
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            return services;
        }

        public IServiceCollection AddOpenApiDocs()
        {
            var conf = KeyMakerApiConfiguration.ReadFromEnvironment();

            services.AddSingleton(conf);

            return conf.IsOpenApiDocEnabled
                ? services.AddEndpointsApiExplorer().AddSwaggerGen()
                : services;
        }
    }
}