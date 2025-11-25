using Microsoft.Extensions.DependencyInjection;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Http;
using Keymaker.Service.Configuration;
using Keymaker.Service.Configuration.Azure;
using Keymaker.Service.Configuration.CloudFlare;
using Keymaker.Service.Configuration.Service;
using Keymaker.Service.Configuration.Volume;
using Keymaker.Service.Configuration.Certificate;
using Keymaker.Service.Dns;
using Keymaker.Service.Expiration;
using Keymaker.Service.Integrations;
using Keymaker.Service.Store;
using FluentValidation;

namespace Keymaker.Service.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKeymaker()
        {
            var keyMakerConfig = EnvironmentReader.GetKeyMakerConfigurationFromEnvironment();
            var certificateConfig = EnvironmentReader.GetCertificateParametersFromEnvironment();
            var cloudFlareDnsServiceConfig = EnvironmentReader.GetCloudFlareDnsServiceConfigurationFromEnvironment();
            var volumeStoreConfig = EnvironmentReader.GetVolumeStoreConfigurationFromEnvironment();
            var azureKeyVaultStoreConfig = EnvironmentReader.GetAzureKeyVaultConfigurationFromEnvironment();
            var azureDnsServiceConfig = EnvironmentReader.GetAzureDnsServiceConfigurationFromEnvironment();
            var azureConfig = EnvironmentReader.GetAzureConfigurationFromEnvironment();

            if (keyMakerConfig.IsAutoRenewalEnabled)
            {
                services.AddHostedService<CheckerBackgroundService>();
            }

            new KeyMakerConfigurationValidator().ValidateAndThrow(keyMakerConfig);
            new CertificateConfigurationValidator().ValidateAndThrow(certificateConfig);

            switch (keyMakerConfig.DnsMode)
            {
                case DnsMode.Azure:
                    new AzureConfigurationValidator().ValidateAndThrow(azureConfig);
                    new AzureDnsServiceConfigurationValidator().ValidateAndThrow(azureDnsServiceConfig);
                    break;
                case DnsMode.CloudFlare:
                    new CloudFlareDnsServiceConfigurationValidator().ValidateAndThrow(cloudFlareDnsServiceConfig);
                    break;
            }

            switch (keyMakerConfig.StorageMode)
            {
                case StorageMode.Volume:
                    new VolumeStoreConfigurationValidator().ValidateAndThrow(volumeStoreConfig);
                    break;
                case StorageMode.KeyVault:
                    new AzureConfigurationValidator().ValidateAndThrow(azureConfig);
                    new AzureKeyVaultStoreConfigurationValidator().ValidateAndThrow(azureKeyVaultStoreConfig);
                    break;
            }

            return services
                .AddDns(keyMakerConfig.DnsMode)
                .AddStore(keyMakerConfig.StorageMode)
                .AddSingleton(azureConfig)
                .AddSingleton(keyMakerConfig)
                .AddSingleton(certificateConfig)
                .AddSingleton(volumeStoreConfig)
                .AddSingleton(azureDnsServiceConfig)
                .AddSingleton(azureKeyVaultStoreConfig)
                .AddSingleton(cloudFlareDnsServiceConfig)
                .AddSingleton<IWebHookService, WebHookService>()
                .AddSingleton<IRenewalChecker, RenewalChecker>()
                .AddSingleton<ICertProducer, CertProducer>()
                .AddSingleton<IHttpProvider, HttpProvider>()
                .AddSingleton<IDnsProvider, DnsProvider>()
                .AddSingleton<IDnsLookupService, DnsLookupService>()
                .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
                .AddSingleton<IAcmeCallback, AcmeCallback>()
                .AddSingleton<IAcmeService, AcmeService>()
                .AddSingleton<IKeymakerService, KeyMakerService>()
                .AddHostedService<IKeymakerService>(sp => sp.GetRequiredService<IKeymakerService>());
        }

        private IServiceCollection AddStore(StorageMode mode)
        {
            return mode == StorageMode.Volume
                ? services.AddSingleton<ICertStoreService, VolumeCertStoreService>()
                : services.AddSingleton<ICertStoreService, AzureKeyVaultStoreService>();
        }

        private IServiceCollection AddDns(DnsMode mode)
        {
            return mode == DnsMode.CloudFlare
                ? services.AddSingleton<IDnsService, CloudFlareDnsService>()
                : services.AddSingleton<IDnsService, AzureDnsService>();
        }
    }
}