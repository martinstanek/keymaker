using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Keymaker.Service.Model;

namespace Keymaker.Service;

public interface IKeymakerService : IHostedService
{
    bool RequestCertificate(CancellationToken token);

    internal bool RequestCertificateInternal(CancellationToken token);

    void CancelCurrentChallenge();

    Task<ChallengeInfo> GetChallengeInfoAsync();
}