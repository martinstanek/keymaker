using System;
using Keymaker.Model;

namespace Keymaker.Service.Configuration;

public static class EnvironmentReader
{
    public static CertificateParameters GetCertificateParametersFromEnvironment()
    {
        return new CertificateParameters
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
    }

    public static CloudFlareDnsServiceConfiguration GetCloudFlareDnsServiceConfigurationFromEnvironment()
    {
        return new CloudFlareDnsServiceConfiguration
        {
            Email = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIEMAIL") ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIKEY") ?? string.Empty,
            Zone = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSAPIZONE") ?? string.Empty,
            DnsChallengeCheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSCHECKDOMAIN") ?? string.Empty,
            DnsChallengeSetDomain = Environment.GetEnvironmentVariable("KEYMAKER_CFDNSSETDOMAIN") ?? string.Empty
        };
    }

    public static AzureDnsServiceConfiguration GetAzureDnsServiceConfigurationFromEnvironment()
    {
        return new AzureDnsServiceConfiguration
        {
            ClientId = Guid.Parse(Environment.GetEnvironmentVariable("KEYMAKER_AZDNSCLIENTID") ?? Guid.Empty.ToString()),
            TenantId = Guid.Parse(Environment.GetEnvironmentVariable("KEYMAKER_AZDNSTENANTID") ?? Guid.Empty.ToString()),
            Secret = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSSECRET") ?? string.Empty,
            DnsZoneResourceId = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSRESOURCEID") ?? string.Empty,
            CheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSCHECKDOMAIN") ?? string.Empty,
            SetDomain = Environment.GetEnvironmentVariable("KEYMAKER_AZDNSSETDOMAIN") ?? string.Empty
        };
    }

    public static VolumeStoreConfiguration GetVolumeStoreConfiguration()
    {
        return new VolumeStoreConfiguration
        {
            ToplevelFolder = Environment.GetEnvironmentVariable("KEYMAKER_VSTOPLEVEL") ?? string.Empty
        };
    }

    public static KeyMakerConfiguration GetKeyMakerConfigurationFromEnvironment()
    {
        return new KeyMakerConfiguration
        {
            DnsMode = Enum.Parse<DnsMode>(Environment.GetEnvironmentVariable("KEYMAKER_DNSMODE") ?? nameof(DnsMode.CloudFlare)),
            StorageMode = Enum.Parse<StorageMode>(Environment.GetEnvironmentVariable("KEYMAKER_STORAGEMODE") ?? nameof(StorageMode.Volume)),
            ChallengeMode = Enum.Parse<ChallengeMode>(Environment.GetEnvironmentVariable("KEYMAKER_CHALLENGEMODE") ?? nameof(ChallengeMode.Http))
        };
    }
}