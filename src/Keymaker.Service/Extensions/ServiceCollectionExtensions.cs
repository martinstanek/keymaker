using Microsoft.Extensions.DependencyInjection;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Http;
using Keymaker.Service.Configuration;
using Keymaker.Service.Dns;
using Keymaker.Service.Expiration;
using Keymaker.Service.Store;

namespace Keymaker.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKeymaker(this IServiceCollection services)
    {
        var keyMakerConfig = EnvironmentReader.GetKeyMakerConfigurationFromEnvironment();
        var certificateParams = EnvironmentReader.GetCertificateParametersFromEnvironment();
        var cloudFlareDnsServiceConfig = EnvironmentReader.GetCloudFlareDnsServiceConfigurationFromEnvironment();
        var volumeStoreConfig = EnvironmentReader.GetVolumeStoreConfigurationFromEnvironment();
        var azureKeyVaultStoreConfig = EnvironmentReader.GetAzureKeyVaultConfigurationFromEnvironment();
        var azureDnsServiceConfig = EnvironmentReader.GetAzureDnsServiceConfigurationFromEnvironment();
        var azureConfig = EnvironmentReader.GetAzureConfigurationFromEnvironment();

        if (keyMakerConfig.IsAutoRenewalEnabled)
        {
            services.AddHostedService<CheckerBackgroundService>();
        }

        return services
            .AddDns(keyMakerConfig.DnsMode)
            .AddStore(keyMakerConfig.StorageMode)
            .AddSingleton(azureConfig)
            .AddSingleton(keyMakerConfig)
            .AddSingleton(certificateParams)
            .AddSingleton(volumeStoreConfig)
            .AddSingleton(azureDnsServiceConfig)
            .AddSingleton(azureKeyVaultStoreConfig)
            .AddSingleton(cloudFlareDnsServiceConfig)
            .AddSingleton<IRenewalChecker, RenewalChecker>()
            .AddSingleton<ICertProducer, CertProducer>()
            .AddSingleton<IHttpProvider, HttpProvider>()
            .AddSingleton<IDnsProvider, DnsProvider>()
            .AddSingleton<IDnsLookupService, DnsLookupService>()
            .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
            .AddSingleton<IAcmeCallback, AcmeCallback>()
            .AddSingleton<IAcmeService, AcmeService>()
            .AddSingleton<IKeymakerService, KeyMakerService>();
    }

    private static IServiceCollection AddStore(this IServiceCollection services, StorageMode mode)
    {
        return mode == StorageMode.Volume
            ? services.AddSingleton<ICertStoreService, VolumeCertStoreService>()
            : services.AddSingleton<ICertStoreService, AzureKeyVaultStoreService>();
    }

    private static IServiceCollection AddDns(this IServiceCollection services, DnsMode mode)
    {
        return mode == DnsMode.CloudFlare
            ? services.AddSingleton<IDnsService, CloudFlareDnsService>()
            : services.AddSingleton<IDnsService, AzureDnsService>();
    }
}