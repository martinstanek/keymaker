using Microsoft.Extensions.DependencyInjection;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Http;
using Keymaker.Service.Configuration;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;

namespace Keymaker.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeymaker(this IServiceCollection services)
    {
        var certificateParams = EnvironmentReader.GetCertificateParametersFromEnvironment();
        var cloudFlareDnsServiceConfig = EnvironmentReader.GetCloudFlareDnsServiceConfigurationFromEnvironment();
        var azureDnsServiceConfig = EnvironmentReader.GetAzureDnsServiceConfigurationFromEnvironment();
        var keyMakerConfig = EnvironmentReader.GetKeyMakerConfigurationFromEnvironment();

        return services
            .AddSingleton(keyMakerConfig)
            .AddSingleton(certificateParams)
            .AddSingleton(cloudFlareDnsServiceConfig)
            .AddSingleton(azureDnsServiceConfig)
            .AddSingleton<ICertStoreService, CertStoreService>()
            .AddSingleton<ICertProducer, CertProducer>()
            .AddSingleton<IHttpProvider, HttpProvider>()
            .AddSingleton<IDnsProvider, DnsProvider>()
            .AddSingleton<IDnsService, CloudFlareDnsService>()
            .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
            .AddSingleton<IAcmeCallback, AcmeCallback>()
            .AddSingleton<IAcmeService, AcmeService>()
            .AddSingleton<IKeymakerService, KeyMakerService>();
    }
}