using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Keymaker.Service.Configuration.Azure;
using Keymaker.Service.Model;
using Org.BouncyCastle.Utilities.Encoders; // TODO: ??

namespace Keymaker.Service.Store;

public sealed class AzureKeyVaultStoreService : ICertStoreService
{
    private const string DomainTag = "domain"; // TODO: not a better way?
    private const string IssuerTag = "Issuer";

    private readonly AzureKeyVaultStoreConfiguration _azKeyVaultStoreConfig;
    private readonly ILogger<AzureKeyVaultStoreService> _logger;
    private readonly Lazy<CertificateClient> _client;

    public AzureKeyVaultStoreService(
        AzureConfiguration azConfig,
        AzureKeyVaultStoreConfiguration azKeyVaultStoreConfig,
        ILogger<AzureKeyVaultStoreService> logger)
    {
        _azKeyVaultStoreConfig = azKeyVaultStoreConfig;
        _logger = logger;
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

        _logger.LogDebug($"Certificate imported: {_azKeyVaultStoreConfig.CertificateName}");
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        var cert = await _client.Value.GetCertificateAsync(_azKeyVaultStoreConfig.CertificateName);

        if (!cert.HasValue)
        {
            return CertificateInfo.Empty;
        }

        var c = await _client.Value.GetCertificatePolicyAsync(_azKeyVaultStoreConfig.CertificateName);
        var domain = c.HasValue ? c.Value.IssuerName ?? "neni" : "fuck";
        var issuer = c.HasValue ? c.Value.SubjectAlternativeNames.DnsNames.FirstOrDefault() ?? "neni" : "fuck";

        // cert.Value.Properties.Tags.TryGetValue(DomainTag, out var domain);
        // cert.Value.Properties.Tags.TryGetValue(IssuerTag, out var issuer);

        return new CertificateInfo
        {
            Domain = domain ?? string.Empty,
            Issuer = issuer ?? string.Empty ,
            Expiry = cert.Value.Properties.ExpiresOn?.DateTime ?? DateTime.MinValue,
            Obtained = cert.Value.Properties.CreatedOn?.DateTime ?? DateTime.MinValue
        };
    }

    public string StoreName => _azKeyVaultStoreConfig.CertificateName;

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