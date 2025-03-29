using System.Threading.Tasks;

namespace Keymaker.Service.Store;

public interface ICertStoreService
{
    Task PersistCertificatesAsync(string domain, string fullChainPem, string privateKeyPem);
}