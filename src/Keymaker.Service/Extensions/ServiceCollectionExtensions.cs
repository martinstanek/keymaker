using Microsoft.Extensions.DependencyInjection;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Configuration;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;

namespace Keymaker.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeymaker(this IServiceCollection services)
    {
        var certificateParams = EnvironmentReader.GetCertificateParametersFromEnvironment();
        var dnsServiceConfig = EnvironmentReader.GetDnsServiceConfigurationFromEnvironment();

        return services
            .AddSingleton(dnsServiceConfig)
            .AddSingleton(certificateParams)
            .AddSingleton<ICertStoreService, CertStoreService>()
            .AddSingleton<IDnsService, DnsService>()
            .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
            .AddSingleton<IAcmeCallback, AcmeCallback>()
            .AddSingleton<IAcmeService, AcmeService>()
            .AddSingleton<IKeymakerService, KeyMakerService>();
    }
}