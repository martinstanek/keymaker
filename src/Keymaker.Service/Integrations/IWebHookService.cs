using System.Threading.Tasks;
using Keymaker.Service.Store;

namespace Keymaker.Service.Integrations;

public interface IWebHookService
{
    Task TriggerWebHookAsync(CertificatePersistenceInfo persistenceInfo);
}