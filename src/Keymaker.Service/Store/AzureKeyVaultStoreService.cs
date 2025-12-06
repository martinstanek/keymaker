using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Keymaker.Service.Configuration.Azure;
using Keymaker.Service.Model;

namespace Keymaker.Service.Store;

public sealed class AzureKeyVaultStoreService : ICertStoreService
{
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
        try
        {
            var certBytes = Convert.FromBase64String(persistenceInfo.Base64Pfx);
            var importOptions = new ImportCertificateOptions(_azKeyVaultStoreConfig.CertificateName, certBytes)
            {
                Password = persistenceInfo.Password,
                Enabled = true
            };

            await _client.Value.ImportCertificateAsync(importOptions);
            
            _logger.LogDebug($"Certificate imported: {_azKeyVaultStoreConfig.CertificateName}");
        }
        catch
        {
            _logger.LogError("Failed to persist the certificate.");
        }
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        try
        {
            var x509Response = await _client.Value.DownloadCertificateAsync(_azKeyVaultStoreConfig.CertificateName);
            var certResponse = await _client.Value.GetCertificateAsync(_azKeyVaultStoreConfig.CertificateName);
            
            if (!x509Response.HasValue || !certResponse.HasValue)
            {
                return CertificateInfo.Empty;
            }
            
            var x509Cert = x509Response.Value;
            
            return new CertificateInfo
            {
                Domain = x509Cert.Subject.Replace("CN=", string.Empty),
                Issuer = x509Cert.IssuerName.Name,
                Expiry = certResponse.Value.Properties.ExpiresOn?.DateTime ?? DateTime.MinValue,
                Obtained = certResponse.Value.Properties.CreatedOn?.DateTime ?? DateTime.MinValue
            };
        }
        catch
        {
            _logger.LogError("Failed to obtain the certificate, check the name and permissions.");
            
            return CertificateInfo.Empty;
        }
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
    
    public string StoreName => _azKeyVaultStoreConfig.CertificateName;
}