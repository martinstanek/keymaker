using System.Collections.Immutable;
using System.Threading.Tasks;
using Keymaker.Service.Model;

namespace Keymaker.Service;

public interface IKeymakerService
{
    void RequestCertificate(CertificateParameters parameters, CertificateRequestChallengeType challengeType);

    CertificateRequestInfo GetCurrentRequestStatus();

    Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync();
}