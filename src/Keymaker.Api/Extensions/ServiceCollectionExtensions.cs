using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Api.Configuration;
using Keymaker.Api.Handlers;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        return services.AddSingleton<RequestHandler>();
    }

    public static IServiceCollection ConfigureSerialization(this IServiceCollection services)
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

    public static IServiceCollection AddOpenApiDocs(this IServiceCollection services)
    {
        var conf = KeyMakerApiConfiguration.ReadFromEnvironment();

        return conf.IsOpenApiDocEnabled
            ? services.AddEndpointsApiExplorer().AddSwaggerGen()
            : services;
    }
}