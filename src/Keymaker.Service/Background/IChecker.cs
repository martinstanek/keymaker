using System.Threading.Tasks;

namespace Keymaker.Service.Background;

public interface IChecker
{
    Task<bool> ShouldTriggerChallengeAsync();
}