using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;
using Keymaker.Model;

namespace Keymaker.Service;

public interface IKeymakerService
{
    bool RequestCertificate(CertificateRequestChallengeType challengeType, CancellationToken token);

    void CancelCurrentChallenge();

    ChallengeStatus GetCurrentRequestStatus();

    Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync();
}