using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service.Store;

public interface ICertStoreService
{
    Task PersistCertificatesAsync(CertificatePersistenceInfo persistenceInfo);

    Task<CertificateInfo> GetMostRecentCertificateInfoAsync();
}