using Keymaker.Service.Model;
using Keymaker.Service.Support;

namespace Keymaker.Service.Configuration;

public static class EnvironmentReader
{
    public static CertificateParameters GetCertificateParametersFromEnvironment()
    {
        return new CertificateParameters
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

    public static CloudFlareDnsServiceConfiguration GetCloudFlareDnsServiceConfigurationFromEnvironment()
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

    public static AzureConfiguration GetAzureConfigurationFromEnvironment()
    {
        return new AzureConfiguration
        {
            ClientId = Env.ReadGuid("KEYMAKER_AZCLIENTID"),
            TenantId = Env.ReadGuid("KEYMAKER_AZTENANTID"),
            Secret = Env.ReadString("KEYMAKER_AZSECRET", secretFile: "KEYMAKER_AZSECRET_FILE")
        };
    }

    public static AzureDnsServiceConfiguration GetAzureDnsServiceConfigurationFromEnvironment()
    {
        return new AzureDnsServiceConfiguration
        {
            DnsZoneResourceId = Env.ReadString("KEYMAKER_AZDNSRESOURCEID"),
            CheckDomain = Env.ReadString("KEYMAKER_AZDNSCHECKDOMAIN"),
            SetDomain = Env.ReadString("KEYMAKER_AZDNSSETDOMAIN")
        };
    }

    public static AzureKeyVaultStoreConfiguration GetAzureKeyVaultConfigurationFromEnvironment()
    {
        return new AzureKeyVaultStoreConfiguration
        {
            KeyVaultUrl = Env.ReadString("KEYMAKER_AZKVURL"),
            CertificateName = Env.ReadString("KEYMAKER_AZKVCERTIFICATENAME")
        };
    }

    public static VolumeStoreConfiguration GetVolumeStoreConfigurationFromEnvironment()
    {
        return new VolumeStoreConfiguration
        {
            ToplevelFolder = Env.ReadString("KEYMAKER_FOLDER")
        };
    }

    public static KeyMakerConfiguration GetKeyMakerConfigurationFromEnvironment()
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
}