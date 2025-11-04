using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Keymaker.Model;

namespace Keymaker.Service;

public interface IKeymakerService : IHostedService
{
    bool RequestCertificate(CancellationToken token);

    void CancelCurrentChallenge();

    ChallengeStatus GetCurrentRequestStatus();

    Task<CertificateInfo> GetMostRecentCertificateInfoAsync();

    Task<ChallengeInfo> GetChallengeInfoAsync();
}