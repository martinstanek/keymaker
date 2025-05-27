using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Threading;
using Keymaker.Model;

namespace Keymaker.Service;

public interface IKeymakerService
{
    void RequestCertificate(CertificateRequestChallengeType challengeType, CancellationToken token);

    ChallengeParameters GetChallengeParameters();

    ChallengeStatus GetCurrentRequestStatus();

    Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync();
}