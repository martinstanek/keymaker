using System.Threading.Tasks;

namespace Keymaker.Service.Integrations;

public interface IWebHookService
{
    Task TriggerWebHookAsync(string fullChain, string privateKey);
}