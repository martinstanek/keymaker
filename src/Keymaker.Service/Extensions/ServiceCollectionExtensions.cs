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

            if (keyMakerConfig.IsAutoRenewalEnabled)
            {
                services.AddHostedService<CheckerBackgroundService>();
            }

            return services
                .AddDns(keyMakerConfig.DnsMode)
                .AddStore(keyMakerConfig.StorageMode)
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

        private IServiceCollection AddOrValidateConfiguration<TConf, TVal>(TConf envConfig, Action<TConf>? process = null) where TVal : IValidator<TConf>, new() where TConf : class
        {
            return services.AddSingleton(sp =>
            {
                var presentConfig = sp.GetService<TConf>();
                var validator = new TVal();

                if (presentConfig is not null)
                {
                    validator.ValidateAndThrow(presentConfig);
                }
                else
                {
                    validator.ValidateAndThrow(envConfig);
                }

                var validatedConfig = presentConfig ?? envConfig;

                process?.Invoke(validatedConfig);

                return validatedConfig;
            });
        }

        private KeyMakerConfiguration AddConfiguration()
        {
            var keyMakerConfig = EnvironmentReader.GetKeyMakerConfigurationFromEnvironment();
            var certificateConfig = EnvironmentReader.GetCertificateConfigurationFromEnvironment();
            var volumeStoreConfig = EnvironmentReader.GetVolumeStoreConfigurationFromEnvironment();
            var azureConfig = EnvironmentReader.GetAzureConfigurationFromEnvironment();
            var azureDnsServiceConfig = EnvironmentReader.GetAzureDnsServiceConfigurationFromEnvironment();
            var azureKeyVaultStoreConfig = EnvironmentReader.GetAzureKeyVaultConfigurationFromEnvironment();
            var cloudFlareDnsServiceConfig = EnvironmentReader.GetCloudFlareDnsServiceConfigurationFromEnvironment();
            var validatedKeymakerConfig = KeyMakerConfiguration.Empty;

            services
                .AddOrValidateConfiguration<CertificateConfiguration, CertificateConfigurationValidator>(certificateConfig)
                .AddOrValidateConfiguration<KeyMakerConfiguration, KeyMakerConfigurationValidator>(keyMakerConfig, kc =>
                {
                    validatedKeymakerConfig = kc;
                });

            if (validatedKeymakerConfig.DnsMode == DnsMode.Azure)
            {
                services
                    .AddOrValidateConfiguration<AzureConfiguration, AzureConfigurationValidator>(azureConfig)
                    .AddOrValidateConfiguration<AzureDnsServiceConfiguration, AzureDnsServiceConfigurationValidator>(azureDnsServiceConfig);
            }

            if (validatedKeymakerConfig.DnsMode == DnsMode.CloudFlare)
            {
                services.AddOrValidateConfiguration<CloudFlareDnsServiceConfiguration, CloudFlareDnsServiceConfigurationValidator>(cloudFlareDnsServiceConfig);
            }

            if (validatedKeymakerConfig.StorageMode == StorageMode.Volume)
            {
                services.AddOrValidateConfiguration<VolumeStoreConfiguration, VolumeStoreConfigurationValidator>(volumeStoreConfig);
            }

            if (validatedKeymakerConfig.StorageMode == StorageMode.KeyVault)
            {
                services
                    .AddOrValidateConfiguration<AzureConfiguration, AzureConfigurationValidator>(azureConfig)
                    .AddOrValidateConfiguration<AzureKeyVaultStoreConfiguration, AzureKeyVaultStoreConfigurationValidator>(azureKeyVaultStoreConfig);
            }

            return validatedKeymakerConfig;
        }
    }
}