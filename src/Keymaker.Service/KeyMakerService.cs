using System.Collections.Immutable;
using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health

public sealed class KeyMakerService : IKeymakerService
{
    public void RequestCertificate(CertificateRequestChallengeType challengeType)
    {
        /*
        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateViaDnsChallengeAsync(_parameters, CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        */

        throw new System.NotImplementedException();
    }

    public ChallengeParameters GetChallengeParameters()
    {
        throw new System.NotImplementedException();
    }

    public CertificateRequestInfo GetCurrentRequestStatus()
    {
        throw new System.NotImplementedException();
    }

    public Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync()
    {
        throw new System.NotImplementedException();
    }
}