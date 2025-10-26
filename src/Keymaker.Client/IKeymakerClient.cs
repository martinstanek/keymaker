using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Client;

public interface IKeymakerClient
{
    Task<CertificateInfo> GetMostRecentCertificateInfoAsync();

    Task<ChallengeInfo> GetChallengeInfoInfoAsync();

    Task<ChallengeStatus> GetChallengeStatusAsync();

    Task<string> ConfirmHttpChallengeAsync(string challenge);

    Task CancelCurrentChallengeAsync();

    Task TriggerChallengeAsync();
}