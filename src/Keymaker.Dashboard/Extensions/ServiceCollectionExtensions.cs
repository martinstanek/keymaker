using Keymaker.Dashboard.Eventing;

namespace Keymaker.Dashboard.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventingServices(this IServiceCollection services)
    {
        return services.AddSingleton<IEventingService, EventingService>();
    }
}