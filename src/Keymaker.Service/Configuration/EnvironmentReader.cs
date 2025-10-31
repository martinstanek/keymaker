using System;
using Keymaker.Model;

namespace Keymaker.Service.Configuration;

public static class EnvironmentReader
{
    public static CertificateParameters GetCertificateParametersFromEnvironment()
    {
        var result = new CertificateParameters
        {
            Contact = Environment.GetEnvironmentVariable("KEYMAKER_CONTACT") ?? string.Empty,
            Domain = Environment.GetEnvironmentVariable("KEYMAKER_DOMAIN") ?? string.Empty,
            CertificateName = Environment.GetEnvironmentVariable("KEYMAKER_CERTNAME") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("KEYMAKER_PASSWORD") ?? string.Empty,
            CountryName = Environment.GetEnvironmentVariable("KEYMAKER_COUNTRY") ?? string.Empty,
            State = Environment.GetEnvironmentVariable("KEYMAKER_STATE") ?? string.Empty,
            Locality = Environment.GetEnvironmentVariable("KEYMAKER_LOCALITY") ?? string.Empty,
            Organization = Environment.GetEnvironmentVariable("KEYMAKER_ORG") ?? string.Empty,
            OrganizationUnit = Environment.GetEnvironmentVariable("KEYMAKER_UNIT") ?? string.Empty
        };

        return result;
    }

    public static CloudFlareDnsServiceConfiguration GetCloudFlareDnsServiceConfigurationFromEnvironment()
    {
        var result = new CloudFlareDnsServiceConfiguration
        {
            Email = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIEMAIL") ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIKEY") ?? string.Empty,
            Zone = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIZONE") ?? string.Empty,
            DnsChallengeCheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSCHECKDOMAIN") ?? string.Empty,
            DnsChallengeSetDomain = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSSETDOMAIN") ?? string.Empty
        };

        return result;
    }

    public static AzureConfiguration GetAzureConfigurationFromEnvironment()
    {
        var result = new AzureConfiguration
        {
            ClientId = Guid.Parse(Environment.GetEnvironmentVariable("KEYMAKER_AZCLIENTID") ?? Guid.Empty.ToString()),
            TenantId = Guid.Parse(Environment.GetEnvironmentVariable("KEYMAKER_AZTENANTID") ?? Guid.Empty.ToString()),
            Secret = Environment.GetEnvironmentVariable("KEYMAKER_AZSECRET") ?? string.Empty
        };

        return result;
    }

    public static AzureDnsServiceConfiguration GetAzureDnsServiceConfigurationFromEnvironment()
    {
        var result = new AzureDnsServiceConfiguration
        {
            DnsZoneResourceId = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSRESOURCEID") ?? string.Empty,
            CheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSCHECKDOMAIN") ?? string.Empty,
            SetDomain = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSSETDOMAIN") ?? string.Empty
        };

        return result;
    }

    public static AzureKeyVaultStoreConfiguration GetAzureKeyVaultConfigurationFromEnvironment()
    {
        var result = new AzureKeyVaultStoreConfiguration
        {
            KeyVaultUrl = Environment.GetEnvironmentVariable("KEYMAKER_AZKVURL") ?? string.Empty,
            CertificateName = Environment.GetEnvironmentVariable("KEYMAKER_AZKVCERTIFICATENAME") ?? string.Empty
        };

        return result;
    }

    public static VolumeStoreConfiguration GetVolumeStoreConfigurationFromEnvironment()
    {
        var result = new VolumeStoreConfiguration
        {
            ToplevelFolder = Environment.GetEnvironmentVariable("KEYMAKER_FOLDER") ?? string.Empty
        };

        return result;
    }

    public static KeyMakerConfiguration GetKeyMakerConfigurationFromEnvironment()
    {
        var result = new KeyMakerConfiguration
        {
            DnsMode = Enum.Parse<DnsMode>(Environment.GetEnvironmentVariable("KEYMAKER_DNSMODE") ?? nameof(DnsMode.CloudFlare)),
            StorageMode = Enum.Parse<StorageMode>(Environment.GetEnvironmentVariable("KEYMAKER_STORAGEMODE") ?? nameof(StorageMode.Volume)),
            ChallengeMode = Enum.Parse<ChallengeMode>(Environment.GetEnvironmentVariable("KEYMAKER_CHALLENGEMODE") ?? nameof(ChallengeMode.Http)),
            IsAutoRenewalEnabled = bool.TryParse(Environment.GetEnvironmentVariable("KEYMAKER_AUTORENEW"), out var autoRenew) && autoRenew,
            IsWebHookEnabled = bool.TryParse(Environment.GetEnvironmentVariable("KEYMAKER_WEBHOOK"), out var webhook) && webhook,
            WebHookUrl = Environment.GetEnvironmentVariable("KEYMAKER_WEBHOOKURL") ?? string.Empty,
            RenewEveryHours = int.TryParse(Environment.GetEnvironmentVariable("KEYMAKER_RENEWEVERYHOURS"), out var renewEveryHours) ? renewEveryHours : 240,
            CheckForExpirationEveryMinutes = int.TryParse(Environment.GetEnvironmentVariable("KEYMAKER_CHECKEVERYMINUTES"), out var checkEveryMinutes) ? checkEveryMinutes : 10,
        };

        return result;
    }
}