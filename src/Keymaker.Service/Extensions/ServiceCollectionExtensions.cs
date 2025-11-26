using System;
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
            var keyMakerConfig = AddConfiguration(services);

            if (keyMakerConfig is null)
            {
                return services.AddSingleton(new ServiceState(ValidationPassed: false));
            }

            if (keyMakerConfig.IsAutoRenewalEnabled)
            {
                services.AddHostedService<CheckerBackgroundService>();
            }

            return services
                .AddDns(keyMakerConfig.DnsMode)
                .AddStore(keyMakerConfig.StorageMode)
                .AddSingleton(new ServiceState(ValidationPassed: true))
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

        private IServiceCollection AddConfiguration<TConf, TVal>(TConf envConfig) where TVal : IValidator<TConf>, new() where TConf : class
        {
            var validator = new TVal();

            validator.ValidateAndThrow(envConfig);

            return services.AddSingleton(envConfig);
        }

        private KeyMakerConfiguration? AddConfiguration()
        {
            var keyMakerConfiguration = EnvironmentConfiguration.ReadKeyMakerConfiguration();
            var certificateConfig = EnvironmentConfiguration.ReadCertificateConfiguration();
            var volumeStoreConfig = EnvironmentConfiguration.ReadVolumeStoreConfiguration();
            var azureConfig = EnvironmentConfiguration.ReadAzureConfiguration();
            var azureDnsServiceConfig = EnvironmentConfiguration.ReadAzureDnsServiceConfiguration();
            var azureKeyVaultStoreConfig = EnvironmentConfiguration.ReadAzureKeyVaultConfiguration();
            var cloudFlareDnsServiceConfig = EnvironmentConfiguration.ReadCloudFlareDnsServiceConfiguration();

            try
            {
                services
                    .AddConfiguration<KeyMakerConfiguration, KeyMakerConfigurationValidator>(keyMakerConfiguration)
                    .AddConfiguration<CertificateConfiguration, CertificateConfigurationValidator>(certificateConfig);

                if (keyMakerConfiguration.DnsMode == DnsMode.Azure)
                {
                    services
                        .AddConfiguration<AzureConfiguration, AzureConfigurationValidator>(azureConfig)
                        .AddConfiguration<AzureDnsServiceConfiguration, AzureDnsServiceConfigurationValidator>(azureDnsServiceConfig);
                }

                if (keyMakerConfiguration.DnsMode == DnsMode.CloudFlare)
                {
                    services.AddConfiguration<CloudFlareDnsServiceConfiguration, CloudFlareDnsServiceConfigurationValidator>(cloudFlareDnsServiceConfig);
                }

                if (keyMakerConfiguration.StorageMode == StorageMode.Volume)
                {
                    services.AddConfiguration<VolumeStoreConfiguration, VolumeStoreConfigurationValidator>(volumeStoreConfig);
                }

                if (keyMakerConfiguration.StorageMode == StorageMode.KeyVault)
                {
                    services
                        .AddConfiguration<AzureConfiguration, AzureConfigurationValidator>(azureConfig)
                        .AddConfiguration<AzureKeyVaultStoreConfiguration, AzureKeyVaultStoreConfigurationValidator>(azureKeyVaultStoreConfig);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

            return keyMakerConfiguration;
        }
    }
}