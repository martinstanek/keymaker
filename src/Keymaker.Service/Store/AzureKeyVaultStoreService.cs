using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Keymaker.Model;
using Keymaker.Service.Configuration;
using Org.BouncyCastle.Utilities.Encoders; // TODO: ??

namespace Keymaker.Service.Store;

public sealed class AzureKeyVaultStoreService : ICertStoreService
{
    private const string DomainTag = "domain"; // TODO: not a better way?
    private const string IssuerTag = "Issuer";

    private readonly AzureKeyVaultStoreConfiguration _azKeyVaultStoreConfig;
    private readonly Lazy<CertificateClient> _client;

    public AzureKeyVaultStoreService(AzureConfiguration azConfig, AzureKeyVaultStoreConfiguration azKeyVaultStoreConfig)
    {
        _azKeyVaultStoreConfig = azKeyVaultStoreConfig;
        _client = new Lazy<CertificateClient>(GetCertificateClient(azConfig, azKeyVaultStoreConfig));
    }

    public async Task PersistCertificatesAsync(CertificatePersistenceInfo persistenceInfo)
    {
        var certBytes = Base64.Decode(persistenceInfo.Base64Pfx);
        var importOptions = new ImportCertificateOptions(_azKeyVaultStoreConfig.CertificateName, certBytes)
        {
            Password = persistenceInfo.Password,
            Enabled = true,
            Tags = { {DomainTag, persistenceInfo.Domain}, {IssuerTag, persistenceInfo.Issuer} }
        };

        await _client.Value.ImportCertificateAsync(importOptions);
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        var cert = await _client.Value.GetCertificateAsync(_azKeyVaultStoreConfig.CertificateName);

        cert.Value.Properties.Tags.TryGetValue(DomainTag, out var domain);
        cert.Value.Properties.Tags.TryGetValue(IssuerTag, out var issuer);

        return new CertificateInfo
        {
            Domain = domain ?? string.Empty,
            Issuer = issuer ?? string.Empty,
            Expiry = cert.Value.Properties.ExpiresOn?.DateTime ?? DateTime.MinValue,
            Obtained = cert.Value.Properties.CreatedOn?.DateTime ?? DateTime.MinValue
        };
    }

    private static CertificateClient GetCertificateClient(AzureConfiguration azConfiguration, AzureKeyVaultStoreConfiguration azKeyVaultStoreConfiguration)
    {
        var credential = new ClientSecretCredential
        (
            tenantId: azConfiguration.TenantId.ToString(),
            clientId: azConfiguration.ClientId.ToString(),
            clientSecret: azConfiguration.Secret
        );

        var client = new CertificateClient(
            vaultUri: new Uri(azKeyVaultStoreConfiguration.KeyVaultUrl),
            credential: credential);

        return client;
    }
}