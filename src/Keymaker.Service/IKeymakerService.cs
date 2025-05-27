using System.Threading.Tasks;
using System.Collections.Immutable;
using Keymaker.Model;

namespace Keymaker.Service;

public interface IKeymakerService
{
    void RequestCertificate(CertificateRequestChallengeType challengeType);

    ChallengeParameters GetChallengeParameters();

    CertificateRequestInfo GetCurrentRequestStatus();

    Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync();
}