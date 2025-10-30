using System.Threading.Tasks;

namespace Keymaker.Service.Background;

public interface IRenewalChecker
{
    Task<bool> ShouldTriggerChallengeAsync();
}