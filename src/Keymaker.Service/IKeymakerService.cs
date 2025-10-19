using System.Threading.Tasks;
using System.Threading;
using Keymaker.Model;

namespace Keymaker.Service;

public interface IKeymakerService
{
    bool RequestCertificate(CancellationToken token);

    void CancelCurrentChallenge();

    ChallengeStatus GetCurrentRequestStatus();

    Task<CertificateInfo> GetMostRecentCertificateInfoAsync();
}