using Keymaker.Api.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        return services.AddSingleton<RequestHandler>();
    }
}