using System.Threading.Tasks;

namespace Keymaker.Service.Expiration;

public interface IRenewalChecker
{
    Task<bool> ShouldTriggerChallengeAsync();
}