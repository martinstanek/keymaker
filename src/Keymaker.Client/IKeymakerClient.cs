using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Client;

public interface IKeymakerClient
{
    Task<ChallengeInfo> GetChallengeInfoInfoAsync();

    Task<string> ConfirmHttpChallengeAsync(string challenge);

    Task CancelCurrentChallengeAsync();

    Task TriggerChallengeAsync();
}