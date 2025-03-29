using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeymaker(this IServiceCollection services)
    {
        services
            .AddSingleton<ICertStoreService, CertStoreService>()
            .AddSingleton<IDnsService, DnsService>()
            .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
            .AddSingleton<IAcmeCallback, AcmeCallback>()
            .AddSingleton<IAcmeService, AcmeService>();

        return services;
    }
}