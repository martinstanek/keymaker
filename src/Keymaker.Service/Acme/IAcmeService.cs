using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme.Model;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task RequestCertificateViaDnsChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken);

    Task RequestCertificateViaHttpChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken);
}