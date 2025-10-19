using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service.Store;

public interface ICertStoreService
{
    Task PersistCertificatesAsync(string domain, string fullChainPem, string privateKeyPem, string base64Pfx);

    Task<CertificateInfo> GetMostRecentCertificateInfoAsync();
}