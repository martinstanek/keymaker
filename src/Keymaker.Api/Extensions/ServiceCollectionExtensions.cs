using Keymaker.Api.Handlers;
using Keymaker.Api.Services.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var defaults = configuration.GetSection(nameof(CertificateParameters)).Get<CertificateParameters>()
                       ?? CertificateParameters.Empty;

        return services
            .AddSingleton(defaults)
            .AddSingleton<CertificateHandler>();
    }
}