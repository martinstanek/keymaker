using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task RequestCertificateViaDnsChallengeAsync(
        CertificateParameters certificateParameters,
        DnsServiceConfiguration dnsConfig,
        CancellationToken cancellationToken);

    Task RequestCertificateViaHttpChallengeAsync(
        CertificateParameters certificateParameters,
        CancellationToken cancellationToken);
}