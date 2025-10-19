using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service.Store;

public sealed class AzureKeyVaultStoreService : ICertStoreService
{
    public Task PersistCertificatesAsync(string domain, string fullChainPem, string privateKeyPem, string base64Pfx)
    {
        return Task.CompletedTask;
    }

    public Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        return Task.FromResult(CertificateInfo.Empty);
    }
}