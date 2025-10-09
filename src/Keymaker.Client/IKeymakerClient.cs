using System.Collections.Immutable;
using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Client;

public interface IKeymakerClient
{
    Task<ImmutableArray<CertificateInfo>> GetCertificatesAsync();

    Task CancelCurrentChallengeAsync();

    Task TriggerDnsChallengeAsync();

    Task TriggerHttpChallengeAsync();
}