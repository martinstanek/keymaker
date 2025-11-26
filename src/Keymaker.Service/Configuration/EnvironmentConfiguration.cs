using System;
using System.Collections.Generic;
using Keymaker.Service.Model;
using Keymaker.Service.Support;
using Keymaker.Service.Configuration.Azure;
using Keymaker.Service.Configuration.Certificate;
using Keymaker.Service.Configuration.CloudFlare;
using Keymaker.Service.Configuration.Service;
using Keymaker.Service.Configuration.Volume;

namespace Keymaker.Service.Configuration;

public static class EnvironmentConfiguration
{
    public static CertificateConfiguration ReadCertificateConfiguration()
    {
        return new CertificateConfiguration
        {
            Contact = Env.ReadString("KEYMAKER_CONTACT", secretFile: "KEYMAKER_CONTACT_FILE"),
            Domain = Env.ReadString("KEYMAKER_DOMAIN"),
            CertificateName = Env.ReadString("KEYMAKER_CERTNAME"),
            Password = Env.ReadString("KEYMAKER_PASSWORD", secretFile: "KEYMAKER_PASSWORD_FILE"),
            CountryName = Env.ReadString("KEYMAKER_COUNTRY"),
            State = Env.ReadString("KEYMAKER_STATE"),
            Locality = Env.ReadString("KEYMAKER_LOCALITY"),
            Organization = Env.ReadString("KEYMAKER_ORG"),
            OrganizationUnit = Env.ReadString("KEYMAKER_UNIT")
        };
    }

    public static IReadOnlyDictionary<string, string> ConstructCertificateConfiguration(CertificateConfiguration configuration)
    {
        return new Dictionary<string, string>
        {
            { "KEYMAKER_CONTACT", configuration.Contact },
            { "KEYMAKER_DOMAIN", configuration.Domain },
            { "KEYMAKER_CERTNAME", configuration.CertificateName },
            { "KEYMAKER_PASSWORD", configuration.Password },
            { "KEYMAKER_COUNTRY", configuration.CountryName },
            { "KEYMAKER_STATE", configuration.State },
            { "KEYMAKER_LOCALITY", configuration.Locality },
            { "KEYMAKER_ORG", configuration.Organization },
            { "KEYMAKER_UNIT", configuration.OrganizationUnit }
        };
    }

    public static CloudFlareDnsServiceConfiguration ReadCloudFlareDnsServiceConfiguration()
    {
        return new CloudFlareDnsServiceConfiguration
        {
            Email = Env.ReadString("KEYMAKER_CFDNSAPIEMAIL", secretFile: "KEYMAKER_CFDNSAPIEMAIL_FILE"),
            Key = Env.ReadString("KEYMAKER_CFDNSAPIKEY", secretFile: "KEYMAKER_CFDNSAPIKEY_FILE"),
            Zone = Env.ReadString("KEYMAKER_CFDNSAPIZONE", secretFile: "KEYMAKER_CFDNSAPIZONE_FILE"),
            DnsChallengeCheckDomain = Env.ReadString("KEYMAKER_CFDNSCHECKDOMAIN"),
            DnsChallengeSetDomain = Env.ReadString("KEYMAKER_CFDNSSETDOMAIN")
        };
    }

    public static IReadOnlyDictionary<string, string> ConstructCloudFlareDnsServiceConfiguration(CloudFlareDnsServiceConfiguration configuration)
    {
        return new Dictionary<string, string>
        {
            { "KEYMAKER_CFDNSAPIEMAIL", configuration.Email },
            { "KEYMAKER_CFDNSAPIKEY", configuration.Key },
            { "KEYMAKER_CFDNSAPIZONE", configuration.Zone },
            { "KEYMAKER_CFDNSCHECKDOMAIN", configuration.DnsChallengeCheckDomain },
            { "KEYMAKER_CFDNSSETDOMAIN", configuration.DnsChallengeCheckDomain }
        };
    }

    public static AzureConfiguration ReadAzureConfiguration()
    {
        return new AzureConfiguration
        {
            ClientId = Env.ReadGuid("KEYMAKER_AZCLIENTID"),
            TenantId = Env.ReadGuid("KEYMAKER_AZTENANTID"),
            Secret = Env.ReadString("KEYMAKER_AZSECRET", secretFile: "KEYMAKER_AZSECRET_FILE")
        };
    }

    public static AzureDnsServiceConfiguration ReadAzureDnsServiceConfiguration()
    {
        return new AzureDnsServiceConfiguration
        {
            DnsZoneResourceId = Env.ReadString("KEYMAKER_AZDNSRESOURCEID"),
            CheckDomain = Env.ReadString("KEYMAKER_AZDNSCHECKDOMAIN"),
            SetDomain = Env.ReadString("KEYMAKER_AZDNSSETDOMAIN")
        };
    }

    public static AzureKeyVaultStoreConfiguration ReadAzureKeyVaultConfiguration()
    {
        return new AzureKeyVaultStoreConfiguration
        {
            KeyVaultUrl = Env.ReadString("KEYMAKER_AZKVURL"),
            CertificateName = Env.ReadString("KEYMAKER_AZKVCERTIFICATENAME")
        };
    }

    public static VolumeStoreConfiguration ReadVolumeStoreConfiguration()
    {
        return new VolumeStoreConfiguration
        {
            ToplevelFolder = Env.ReadString("KEYMAKER_FOLDER")
        };
    }

    public static IReadOnlyDictionary<string, string> ConstructVolumeStoreConfiguration(VolumeStoreConfiguration configuration)
    {
        return new Dictionary<string, string>
        {
            { "KEYMAKER_FOLDER", configuration.ToplevelFolder }
        };
    }

    public static KeyMakerConfiguration ReadKeyMakerConfiguration()
    {
        return new KeyMakerConfiguration
        {
            DnsMode = Env.ReadEnum("KEYMAKER_DNSMODE", DnsMode.CloudFlare),
            StorageMode = Env.ReadEnum("KEYMAKER_STORAGEMODE", StorageMode.Volume),
            ChallengeMode = Env.ReadEnum("KEYMAKER_CHALLENGEMODE", ChallengeMode.Http),
            IsAutoRenewalEnabled = Env.ReadBool("KEYMAKER_AUTORENEW", false),
            IsWebHookEnabled = Env.ReadBool("KEYMAKER_WEBHOOK", false),
            IsChallengeTriggerEnabled = Env.ReadBool("KEYMAKER_ENABLECHALLENGETRIGGER", true),
            WebHookUrl = Env.ReadString("KEYMAKER_WEBHOOKURL"),
            RenewEveryHours = Env.ReadInt("KEYMAKER_RENEWEVERYHOURS", 240),
            CheckForExpirationEveryMinutes = Env.ReadInt("KEYMAKER_CHECKEVERYMINUTES", 10)
        };
    }

    public static IReadOnlyDictionary<string, string> ConstructKeyMakerConfiguration(KeyMakerConfiguration configuration)
    {
        return new Dictionary<string, string>()
        {
            { "KEYMAKER_DNSMODE", configuration.DnsMode.ToString() },
            { "KEYMAKER_STORAGEMODE", configuration.StorageMode.ToString() },
            { "KEYMAKER_CHALLENGEMODE", configuration.ChallengeMode.ToString() },
            { "KEYMAKER_AUTORENEW", configuration.IsAutoRenewalEnabled.ToString() },
            { "KEYMAKER_WEBHOOK", configuration.IsWebHookEnabled.ToString() },
            { "KEYMAKER_ENABLECHALLENGETRIGGER", configuration.IsChallengeTriggerEnabled.ToString() },
            { "KEYMAKER_WEBHOOKURL", configuration.WebHookUrl },
            { "KEYMAKER_RENEWEVERYHOURS", configuration.RenewEveryHours.ToString() },
            { "KEYMAKER_CHECKEVERYMINUTES", configuration.CheckForExpirationEveryMinutes.ToString() }
        };
    }

    public static void WriteConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        foreach (var config in configuration)
        {
            Environment.SetEnvironmentVariable(config.Key, config.Value);
        }
    }

    public static void RemoveConfiguration(IReadOnlyDictionary<string, string> configuration)
    {
        foreach (var config in configuration)
        {
            Environment.SetEnvironmentVariable(config.Key, null);
        }
    }
}